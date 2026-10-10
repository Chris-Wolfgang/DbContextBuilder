using System.Net;
using System.Text.RegularExpressions;

namespace Wolfgang.DbContextBuilder.Tests.DocExamples;

/// <summary>
/// A single code sample extracted from the documentation, together with the source location it
/// came from so a failure can point back at the exact sample to fix. It is either an
/// <c>&lt;example&gt;&lt;code&gt;</c> block from an XML-doc comment or a <c>```csharp</c> fence in
/// a Markdown page.
/// </summary>
public sealed class DocExample
{
    public DocExample(string file, int line, string code)
    {
        File = file;
        Line = line;
        Code = code;
    }


    /// <summary>Repository-relative path (forward-slashed) of the file the example lives in.</summary>
    public string File { get; }


    /// <summary>1-based line number of the first line of the sample's code.</summary>
    public int Line { get; }


    /// <summary>The snippet text (XML entities resolved, <c>///</c> prefixes or fence indentation stripped).</summary>
    public string Code { get; }


    public override string ToString() => $"{File}:{Line}";
}


/// <summary>
/// Finds the documentation code samples (#306, #600):
/// <list type="bullet">
///   <item><c>&lt;example&gt;&lt;code&gt;</c> blocks in <c>///</c> comments under <c>src/</c>;</item>
///   <item><c>```csharp</c> fences in <c>README.md</c>, <c>examples/README.md</c> and the docfx
///   pages under <c>docfx_project/</c>.</item>
/// </list>
/// A fence that is deliberately not compilable is skipped when the line before it is
/// <c>&lt;!-- doc-example: skip --&gt;</c>.
/// </summary>
public static class DocExampleSource
{
    /// <summary>The marker that excludes the Markdown fence after it from compilation.</summary>
    public const string SkipMarker = "<!-- doc-example: skip -->";

    private static readonly Regex FenceOpen = new(@"^(?<indent>[ \t]*)```csharp\s*$", RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));

    private static readonly Regex FenceClose = new(@"^[ \t]*```\s*$", RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));


    /// <summary>
    /// Finds every documentation code sample in the repository.
    /// </summary>
    public static async Task<IReadOnlyList<DocExample>> DiscoverAllAsync()
    {
        var sourceDirectory = LocateSourceDirectory(AppContext.BaseDirectory);
        var root = Directory.GetParent(sourceDirectory)!.FullName;
        var examples = new List<DocExample>();

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories))
        {
            if (!IsGeneratedPath(file))
            {
                examples.AddRange(ExtractFromSource(await File.ReadAllLinesAsync(file).ConfigureAwait(false), Relative(root, file)));
            }
        }

        foreach (var file in MarkdownFiles(root))
        {
            examples.AddRange(ExtractFromMarkdown(await File.ReadAllLinesAsync(file).ConfigureAwait(false), Relative(root, file)));
        }

        return examples;
    }


    private static IEnumerable<string> MarkdownFiles(string root)
    {
        yield return Path.Combine(root, "README.md");
        yield return Path.Combine(root, "examples", "README.md");

        foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "docfx_project"), "*.md", SearchOption.AllDirectories))
        {
            if (!IsGeneratedPath(file))
            {
                yield return file;
            }
        }
    }


    private static string Relative(string root, string file) => Path.GetRelativePath(root, file).Replace('\\', '/');


    internal static IEnumerable<DocExample> ExtractFromSource(IReadOnlyList<string> lines, string relative)
    {
        var inExample = false;
        var inCode = false;
        var codeStartLine = 0;
        var buffer = new List<string>();

        for (var i = 0; i < lines.Count; i++)
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


    internal static IEnumerable<DocExample> ExtractFromMarkdown(IReadOnlyList<string> lines, string relative)
    {
        var previous = string.Empty;
        for (var i = 0; i < lines.Count; i++)
        {
            var open = FenceOpen.Match(lines[i]);
            if (!open.Success)
            {
                // Every non-fence line replaces `previous`, a blank one included: the marker counts
                // only on the line immediately before the fence, so it cannot reach a later sample.
                previous = lines[i].Trim();
                continue;
            }

            var skip = string.Equals(previous, SkipMarker, StringComparison.Ordinal);
            var indent = open.Groups["indent"].Value;
            var startLine = i + 2; // first code line, 1-based
            var buffer = new List<string>();
            for (i++; i < lines.Count && !FenceClose.IsMatch(lines[i]); i++)
            {
                // A fence nested in a list item is indented; drop that indentation.
                buffer.Add(lines[i].StartsWith(indent, StringComparison.Ordinal) ? lines[i].Substring(indent.Length) : lines[i]);
            }

            previous = string.Empty;
            if (!skip)
            {
                yield return new DocExample(relative, startLine, string.Join("\n", buffer));
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
            // docfx output (_site) and generated API pages are build products, not documentation sources.
            || file.Contains($"{sep}_site{sep}", StringComparison.Ordinal)
            || file.Contains($"{sep}docfx_project{sep}api{sep}", StringComparison.Ordinal)
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
