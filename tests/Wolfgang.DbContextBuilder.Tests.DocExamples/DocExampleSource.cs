using System.Net;
using Xunit.Abstractions;

namespace Wolfgang.DbContextBuilder.Tests.DocExamples;

/// <summary>
/// A single <c>&lt;example&gt;&lt;code&gt;</c> block extracted from the library's
/// XML-doc comments, together with the source location it came from so a failure
/// can point back at the exact doc comment to fix.
/// </summary>
public sealed class DocExample : IXunitSerializable
{
    // Parameterless ctor + settable members are required by IXunitSerializable so
    // xunit can round-trip the case through the test runner.
    public DocExample()
    {
    }


    public DocExample(string file, int line, string code)
    {
        File = file;
        Line = line;
        Code = code;
    }


    /// <summary>Repository-relative path (forward-slashed) of the file the example lives in.</summary>
    public string File { get; set; } = string.Empty;


    /// <summary>1-based line number of the first line of the <c>&lt;code&gt;</c> block.</summary>
    public int Line { get; set; }


    /// <summary>The decoded snippet text (XML entities resolved, <c>///</c> prefixes stripped).</summary>
    public string Code { get; set; } = string.Empty;


    public void Deserialize(IXunitSerializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        File = info.GetValue<string>("file");
        Line = info.GetValue<int>("line");
        Code = info.GetValue<string>("code");
    }


    public void Serialize(IXunitSerializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        info.AddValue("file", File);
        info.AddValue("line", Line);
        info.AddValue("code", Code);
    }


    // Drives the theory's per-case display name.
    public override string ToString() => $"{File}:{Line}";
}


/// <summary>
/// Locates the library's <c>src/</c> tree and scans it for <c>&lt;example&gt;&lt;code&gt;</c>
/// blocks in <c>///</c> documentation comments. Scans every project under <c>src/</c> (not just
/// one), because this repo's examples span multiple packages (e.g. -Core and .Bogus).
/// </summary>
public static class DocExampleSource
{
    /// <summary>
    /// Finds every <c>&lt;example&gt;&lt;code&gt;</c> block in the library's source files.
    /// </summary>
    public static IReadOnlyList<DocExample> DiscoverAll()
    {
        var sourceDirectory = LocateSourceDirectory(AppContext.BaseDirectory);
        var examples = new List<DocExample>();

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (IsGeneratedPath(file))
            {
                continue;
            }

            var relative = Path.GetRelativePath(sourceDirectory, file).Replace('\\', '/');
            examples.AddRange(ExtractFromFile(file, relative));
        }

        return examples;
    }


    private static IEnumerable<DocExample> ExtractFromFile(string file, string relative)
    {
        var lines = File.ReadAllLines(file);

        var inExample = false;
        var inCode = false;
        var codeStartLine = 0;
        var buffer = new List<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            var content = StripDocPrefix(lines[i]);
            if (content is null)
            {
                // A non-doc line cannot appear inside a well-formed doc block; reset defensively.
                inExample = false;
                inCode = false;
                continue;
            }

            var tag = content.Trim();
            switch (tag)
            {
                case "<example>":
                    inExample = true;
                    break;

                case "</example>":
                    inExample = false;
                    inCode = false;
                    break;

                case "<code>" when inExample:
                    inCode = true;
                    buffer.Clear();
                    codeStartLine = i + 2; // first code line, 1-based
                    break;

                case "</code>" when inCode:
                    inCode = false;
                    yield return new DocExample(relative, codeStartLine, Decode(string.Join("\n", buffer)));
                    break;

                default:
                    if (inCode)
                    {
                        buffer.Add(content);
                    }

                    break;
            }
        }
    }


    // Strips a leading `///` (and the single conventional space after it) while preserving
    // the snippet's own indentation. Returns null for lines that are not doc comments.
    private static string? StripDocPrefix(string line)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith("///", StringComparison.Ordinal))
        {
            return null;
        }

        var rest = trimmed.Substring(3);
        if (rest.StartsWith(" ", StringComparison.Ordinal))
        {
            rest = rest.Substring(1);
        }

        return rest;
    }


    // XML doc comments escape `<`, `>` and `&` as entities; decode them back to real C#.
    private static string Decode(string code) => WebUtility.HtmlDecode(code);


    private static bool IsGeneratedPath(string file)
    {
        var sep = Path.DirectorySeparatorChar;
        return file.Contains($"{sep}bin{sep}", StringComparison.Ordinal)
            || file.Contains($"{sep}obj{sep}", StringComparison.Ordinal)
            // extra-projects/AdventureWorks-EF{N} are EF-scaffolded demo models, not hand-written
            // library source, and are not expected to carry XML-doc examples.
            || file.Contains($"{sep}extra-projects{sep}", StringComparison.Ordinal);
    }


    // Walks up from the test assembly's base directory to the checked-out tree. Deliberately
    // avoids [CallerFilePath], which bakes in the build-machine path and resolves to a
    // non-existent '/_/...' location under CI's deterministic-build settings. Anchors on the
    // whole 'src' directory (not one project under it) because examples in this repo span
    // multiple packages.
    internal static string LocateSourceDirectory(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src");
            if (Directory.Exists(candidate)
                && File.Exists(Path.Combine(candidate, "Wolfgang.DbContextBuilder-Core", "Wolfgang.DbContextBuilder-Core.csproj")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate 'src' above '{startDirectory}'.");
    }
}
