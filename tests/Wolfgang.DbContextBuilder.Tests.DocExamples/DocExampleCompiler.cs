using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Wolfgang.DbContextBuilderCore;

namespace Wolfgang.DbContextBuilder.Tests.DocExamples;

/// <summary>
/// Compiles an extracted doc <see cref="DocExample"/> against the real
/// <c>Wolfgang.DbContextBuilder-Core</c> (and, where an example uses it,
/// <c>Wolfgang.DbContextBuilder.Bogus</c>) assemblies. The snippet is wrapped in a synthetic
/// harness that supplies the imports and the placeholder identifiers the illustrative
/// snippets reference (<c>ShopDbContext</c>, <c>Product</c>, …) while the actual API calls
/// (<c>UseInMemory</c>/<c>SeedWith</c>/<c>SeedWithRandom</c>/<c>UseBogus</c>/<c>UseSeedProfile</c>/
/// <c>BuildAsync</c>/…) bind against the shipped types — so a renamed or removed member turns a
/// stale example into a compile error.
/// </summary>
public static class DocExampleCompiler
{
    // Matches a top-level type-declaration line, e.g. "public sealed class Foo : IBar<Baz>".
    // Snippets that declare a helper type (an ISeedProfile implementation, say) alongside
    // usage code need that declaration emitted as a sibling member of the generated namespace,
    // not folded into the wrapper method body - a class cannot be declared inside a method.
    private static readonly Regex TypeDeclarationStart = new(
        @"^\s*(?:public\s+|internal\s+|private\s+)?(?:sealed\s+|abstract\s+|static\s+|partial\s+)*(?:class|interface|struct|record)\b",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture,
        TimeSpan.FromSeconds(1));


    /// <summary>
    /// Wraps and compiles <paramref name="example"/>, returning only the
    /// error-severity diagnostics (an empty list means the snippet is valid).
    /// </summary>
    public static IReadOnlyList<Diagnostic> Compile(DocExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        var source = BuildSource(example);

        var tree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Latest));

        var compilation = CSharpCompilation.Create(
            assemblyName: "DocExampleScratch",
            syntaxTrees: [tree],
            references: ReferenceAssemblies(),
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Disable));

        return compilation
            .GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();
    }


    private static string BuildSource(DocExample example)
    {
        var (headerLines, bodyLines, bodyStartLine) = SplitHeaderAndBody(example.Code, example.Line);
        var (signature, closer) = WrapperSignature(string.Join('\n', bodyLines));
        var location = example.File; // repository-relative, already forward-slashed

        var header = headerLines.Count == 0
            ? string.Empty
            : $$"""
                #line {{example.Line}} "{{location}}"
                {{string.Join('\n', headerLines)}}
                #line default

                """;

        return $$"""
            using System;
            using System.IO;
            using System.Linq;
            using System.Threading;
            using System.Threading.Tasks;
            using System.Collections.Generic;
            using Microsoft.EntityFrameworkCore;
            using Wolfgang.DbContextBuilderCore;

            namespace DocExamples.Generated
            {
                // A sample DbContext and entity type the illustrative snippets build against.
                // Never constructed (the snippets are compiled, not run) - only their shape
                // (a DbContext subtype; a class with a settable Name) matters, chosen so the
                // real API calls in the snippets resolve exactly as a consumer's would. Public,
                // not internal: a snippet may declare its own public type (e.g. a public
                // ISeedProfile implementation) with a public member referencing these by name,
                // and an internal type there would be an accessibility mismatch (CS0051) that
                // has nothing to do with the snippet actually being wrong.
                public sealed class ShopDbContext : DbContext
                {
                }

                public sealed class Product
                {
                    public string Name { get; set; } = string.Empty;
                }

            {{header}}
                internal sealed class Example
                {
                    public {{signature}}
                    {
            #line {{bodyStartLine}} "{{location}}"
            {{string.Join('\n', bodyLines)}}
            #line default
                    }{{closer}}
                }
            }
            """;
    }


    // Splits a snippet into a leading run of top-level type declarations (which must be
    // emitted as sibling members of the generated namespace - a class cannot be declared
    // inside a method body) and the remaining executable statements (wrapped into the
    // Example.Run() method body). Tracks brace depth so a declaration's own body doesn't
    // fool the scan. Snippets with no leading type declaration (the common case) return an
    // empty header and the whole snippet as the body, unchanged from before this split existed.
    private static (List<string> Header, List<string> Body, int BodyStartLine) SplitHeaderAndBody(string code, int startLine)
    {
        var lines = code.Split('\n');
        var header = new List<string>();
        var depth = 0;
        var inHeaderBlock = false;
        var sawOpenBrace = false;
        var splitIndex = 0;

        for (; splitIndex < lines.Length; splitIndex++)
        {
            var line = lines[splitIndex];

            if (!inHeaderBlock)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    header.Add(line);
                    continue;
                }

                if (!TypeDeclarationStart.IsMatch(line))
                {
                    break; // first non-blank, non-declaration line at depth 0: body starts here
                }

                inHeaderBlock = true;
                sawOpenBrace = false;
            }

            header.Add(line);
            depth += line.Count(c => c == '{') - line.Count(c => c == '}');
            sawOpenBrace = sawOpenBrace || depth > 0;

            // Only close the header block once the declaration's own braces have opened AND
            // closed - a multi-line signature (e.g. an expression-bodied member spanning
            // several lines before its first '{') must not be mistaken for "already closed"
            // just because depth hasn't gone positive yet.
            if (inHeaderBlock && sawOpenBrace && depth <= 0)
            {
                inHeaderBlock = false;
                depth = 0;
            }
        }

        var body = lines.Skip(splitIndex).ToList();
        return (header, body, startLine + splitIndex);
    }


    // Chooses the wrapper method shape that lets the snippet's BODY compile. A `yield`
    // snippet needs an async-iterator wrapper, an `await` snippet needs an async wrapper, and
    // anything else (e.g. a plain `foreach`) gets a synchronous, non-async wrapper - the last
    // one avoids a spurious CS1998 "async method lacks await" on those.
    private static (string Signature, string Closer) WrapperSignature(string code)
    {
        if (ContainsWord(code, "yield"))
        {
            return ("async IAsyncEnumerable<string> Run()", string.Empty);
        }

        if (ContainsWord(code, "await"))
        {
            return ("async Task Run()", string.Empty);
        }

        return ("void Run()", string.Empty);
    }


    // Internal so the boundary-scanning loop (a match inside a longer identifier must be
    // skipped, and a later whole-word match still found) can be pinned by a test.
    internal static bool ContainsWord(string code, string word)
    {
        var index = code.IndexOf(word, StringComparison.Ordinal);
        while (index >= 0)
        {
            var before = index == 0 || !char.IsLetterOrDigit(code[index - 1]);
            var afterIndex = index + word.Length;
            var after = afterIndex >= code.Length || !char.IsLetterOrDigit(code[afterIndex]);
            if (before && after)
            {
                return true;
            }

            index = code.IndexOf(word, index + 1, StringComparison.Ordinal);
        }

        return false;
    }


    // The compiler needs the full framework reference set plus the library under test. The
    // trusted-platform-assemblies list is the reference closure of the running test host
    // (net10.0), which already includes the project-referenced -Core and .Bogus assemblies.
    private static IReadOnlyList<MetadataReference> ReferenceAssemblies()
    {
        var references = new List<MetadataReference>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
        foreach (var path in trusted.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && seen.Add(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        // Belt-and-braces: guarantee the libraries under test are referenced even if they are
        // ever loaded from outside the TPA closure.
        AddIfUnseen(references, seen, typeof(DbContextBuilder<>).Assembly.Location);
        AddIfUnseen(references, seen, typeof(BogusRandomEntityCreator).Assembly.Location);

        return references;
    }



    // Adds a reference for `path` unless it is empty or already in `seen`. Split out so the
    // outside-the-TPA case (never hit under the test host) is exercised directly.
    internal static void AddIfUnseen(ICollection<MetadataReference> references, ISet<string> seen, string? path)
    {
        if (!string.IsNullOrEmpty(path) && seen.Add(path))
        {
            references.Add(MetadataReference.CreateFromFile(path));
        }
    }
}
