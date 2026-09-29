using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Wolfgang.DbContextBuilderCore;
using Xunit.Abstractions;

namespace Wolfgang.DbContextBuilder.Tests.DocExamples;

/// <summary>
/// Guards against XML-doc example rot (#306): every <c>&lt;example&gt;&lt;code&gt;</c>
/// block in the library's documentation comments must still compile against the
/// current public API. A snippet that references a renamed or removed member becomes
/// a failing test instead of silently outliving the API it documents.
/// </summary>
public sealed class DocExampleCompilationTests
{
    public static IEnumerable<object[]> Examples()
        => DocExampleSource.DiscoverAll().Select(example => new object[] { example });


    [Fact]
    public void Source_scan_finds_the_documented_examples()
    {
        var examples = DocExampleSource.DiscoverAll();

        // Floor guard: if extraction (or source-tree location) ever silently breaks, the
        // theory below would pass vacuously with zero cases. This fails loudly instead.
        // Two are known today (ISeedProfile.cs, BogusRandomEntityCreator.cs) - the floor is
        // set at that count rather than padded, so a regression to zero is caught immediately
        // rather than waiting for a third example to be added.
        Assert.True(
            examples.Count >= 2,
            $"Expected to find the documented <example><code> blocks in the library source, "
            + $"but found {examples.Count}. Doc-example extraction or source-tree discovery is broken.");
    }


    [Theory]
    [MemberData(nameof(Examples))]
    public void Documented_example_still_compiles(DocExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        var errors = DocExampleCompiler.Compile(example);

        Assert.True(
            errors.Count == 0,
            BuildFailureMessage(example, errors));
    }


    private static string BuildFailureMessage(DocExample example, IReadOnlyList<Diagnostic> errors)
    {
        var rendered = string.Join(Environment.NewLine, errors.Select(e => "    " + e));

        return $"The XML-doc <example> at {example.File}:{example.Line} no longer compiles "
            + $"against the current public API:{Environment.NewLine}{rendered}"
            + $"{Environment.NewLine}--- snippet ---{Environment.NewLine}{example.Code}";
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


    [Fact]
    public void DocExample_parameterless_constructor_produces_empty_defaults()
    {
        var example = new DocExample();

        Assert.Equal(string.Empty, example.File);
        Assert.Equal(0, example.Line);
        Assert.Equal(string.Empty, example.Code);
    }


    [Fact]
    public void DocExample_serializes_and_deserializes_all_properties()
    {
        var original = new DocExample("tests/Foo.cs", 42, "await Bar();");
        var info = new FakeXunitSerializationInfo();

        original.Serialize(info);

        var restored = new DocExample();
        restored.Deserialize(info);

        Assert.Equal(original.File, restored.File);
        Assert.Equal(original.Line, restored.Line);
        Assert.Equal(original.Code, restored.Code);
    }


    [Fact]
    public void DocExample_Serialize_and_Deserialize_reject_a_null_info()
    {
        var example = new DocExample("tests/Foo.cs", 1, "// nothing");

        Assert.Throws<ArgumentNullException>(() => example.Serialize(null!));
        Assert.Throws<ArgumentNullException>(() => example.Deserialize(null!));
    }


    // DocExample.Deserialize only ever calls the generic GetValue<T>(key) - the fake's
    // non-generic GetValue(key, type) exists solely to satisfy IXunitSerializationInfo and
    // is never reached through that path, so it needs its own direct test.
    [Fact]
    public void FakeXunitSerializationInfo_non_generic_GetValue_returns_the_stored_value()
    {
        IXunitSerializationInfo info = new FakeXunitSerializationInfo();
        info.AddValue("key", "stored-value");

        var value = info.GetValue("key", typeof(string));

        Assert.Equal("stored-value", value);
    }


    // Minimal in-memory IXunitSerializationInfo so the round-trip above can run as a plain
    // unit test instead of depending on the xunit runner's own (environment-dependent)
    // decision to actually invoke Serialize/Deserialize for a given test host.
    private sealed class FakeXunitSerializationInfo : IXunitSerializationInfo
    {
        private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);


        public void AddValue(string key, object? value, Type? type = null) => _values[key] = value;


        public T GetValue<T>(string key) => (T)_values[key]!;


        public object GetValue(string key, Type type) => _values[key]!;
    }
}
