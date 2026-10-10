using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Wolfgang.DbContextBuilderCore;

namespace Wolfgang.DbContextBuilder.Tests.DocExamples;

/// <summary>
/// Guards against documentation example rot (#306, #600): every <c>&lt;example&gt;&lt;code&gt;</c>
/// block in the library's XML-doc comments and every <c>```csharp</c> fence in the README,
/// examples/README and docfx pages must still compile against the current public API. A sample
/// that references a renamed or removed member becomes a failing test instead of silently
/// outliving the API it documents.
/// </summary>
public sealed class DocExampleCompilationTests
{
    [Fact]
    public async Task DiscoverAllAsync_finds_the_documented_examples()
    {
        var examples = await DocExampleSource.DiscoverAllAsync();

        // Floor guards: if extraction (or tree location) ever silently breaks, the compile test
        // below would pass vacuously. Set at today's counts, not padded: 2 XML-doc examples
        // (ISeedProfile.cs, BogusRandomEntityCreator.cs) and 6 Markdown samples (README.md,
        // examples/README.md, docfx introduction.md and 3 in getting-started.md).
        Assert.True(
            examples.Count(e => e.File.EndsWith(".cs", StringComparison.Ordinal)) >= 2,
            $"Expected the documented <example><code> blocks in the library source; found {string.Join(", ", examples)}.");
        Assert.True(
            examples.Count(e => e.File.EndsWith(".md", StringComparison.Ordinal)) >= 6,
            $"Expected the csharp samples in the Markdown docs; found {string.Join(", ", examples)}.");
    }


    [Fact]
    public async Task Documented_examples_still_compile()
    {
        // The message is built for every sample (not only failing ones) so this test's own failure
        // path is executed, and covered, on every run.
        var results = (await DocExampleSource.DiscoverAllAsync())
            .Select(example =>
            {
                var errors = DocExampleCompiler.Compile(example);
                return (Errors: errors, Message: BuildFailureMessage(example, errors));
            })
            .ToList();
        var failures = results.Where(result => result.Errors.Count > 0).Select(result => result.Message).ToList();

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine + Environment.NewLine, failures));
    }


    private static string BuildFailureMessage(DocExample example, IReadOnlyList<Diagnostic> errors)
    {
        var rendered = string.Join(Environment.NewLine, errors.Select(e => "    " + e));

        return $"The documentation sample at {example.File}:{example.Line} no longer compiles "
            + $"against the current public API:{Environment.NewLine}{rendered}"
            + $"{Environment.NewLine}--- snippet ---{Environment.NewLine}{example.Code}";
    }


    [Fact]
    public void ExtractFromMarkdown_strips_a_list_items_indentation_and_reports_the_first_code_line()
    {
        string[] lines = ["Text", "", "  ```csharp", "  var a = 1;", "  var b = 2;", "  ```", "after"];

        var example = Assert.Single(DocExampleSource.ExtractFromMarkdown(lines, "doc.md"));

        Assert.Equal("doc.md:4", example.ToString());
        Assert.Equal("var a = 1;\nvar b = 2;", example.Code);
    }


    [Fact]
    public void ExtractFromMarkdown_skips_a_fence_after_the_skip_marker()
    {
        string[] lines = [DocExampleSource.SkipMarker, "```csharp", ".Fragment()", "```", "```csharp", "var kept = 1;", "```"];

        var example = Assert.Single(DocExampleSource.ExtractFromMarkdown(lines, "doc.md"));

        Assert.Equal("var kept = 1;", example.Code);
    }


    [Fact]
    public void Compile_lifts_a_using_directive_out_of_the_wrapper_method()
    {
        var example = new DocExample("doc.md", 1, "using System.Text;\nvar builder = new StringBuilder();");

        Assert.Empty(DocExampleCompiler.Compile(example));
    }


    // ---- helper coverage: the branches the doc-example corpus never exercises ----

    [Theory]
    [InlineData("var x = myawait;", "await", false)]           // only inside a longer identifier
    [InlineData("myawait; await y;", "await", true)]           // first hit is embedded, a later one is whole
    [InlineData("await", "await", true)]                       // whole input
    [InlineData("awaitable", "await", false)]                  // prefix of a longer identifier
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "InlineData supplies non-null literals; the analyzer follows the call into ContainsWord.")]
    public void ContainsWord_matches_whole_identifiers_only(string code, string word, bool expected)
    {
        Assert.Equal(expected, DocExampleCompiler.ContainsWord(code, word));
    }


    [Fact]
    public void AddIfUnseen_adds_a_path_once_and_skips_empty_and_repeated_paths()
    {
        var references = new List<MetadataReference>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = typeof(DbContextBuilder<>).Assembly.Location;

        DocExampleCompiler.AddIfUnseen(references, seen, null);
        DocExampleCompiler.AddIfUnseen(references, seen, string.Empty);
        DocExampleCompiler.AddIfUnseen(references, seen, path);
        DocExampleCompiler.AddIfUnseen(references, seen, path);

        Assert.Single(references);
        Assert.Contains(path, seen);
    }


    [Fact]
    public void LocateSourceDirectory_throws_when_no_source_tree_is_above_the_start_directory()
    {
        var start = Path.GetTempPath();

        var exception = Assert.Throws<DirectoryNotFoundException>(() => DocExampleSource.LocateSourceDirectory(start));

        Assert.Contains(start, exception.Message, StringComparison.Ordinal);
    }


    // Neither real doc example contains a `yield` or a snippet free of both `yield` and
    // `await` - the corpus only ever exercises the `await` wrapper branch. These synthetic
    // examples pin the other two branches of DocExampleCompiler's wrapper-shape selection.

    [Fact]
    public void Compile_wraps_a_yield_snippet_in_an_async_iterator()
    {
        var example = new DocExample("synthetic.cs", 1, "yield return \"value\";");

        var errors = DocExampleCompiler.Compile(example);

        Assert.Empty(errors);
    }


    [Fact]
    public void Compile_wraps_a_plain_snippet_in_a_synchronous_method()
    {
        var example = new DocExample("synthetic.cs", 1, "var sum = 1 + 1;");

        var errors = DocExampleCompiler.Compile(example);

        Assert.Empty(errors);
    }
}
