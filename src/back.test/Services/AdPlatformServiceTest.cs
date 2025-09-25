using AdPlatforms.Back.Models;

namespace AdPlatforms.Back.Services.Tests;

public sealed class AdPlatformServiceTest
{
    const string dataExample = """
        Яндекс.Директ:/ru
        Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
        Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
        Крутая реклама:/ru/svrd
        """;

    AdPlatformService Service { get; } = new();

    [Fact]
    internal void ParseAndGroupLocations()
    {
        var locations = ((string[])
        [
            "/a/aa",
            "/a/b/c/d",
            "/a/b",
            "/x/y/z"
        ]).Select(p => new Location(p));
        var expected = (((string gen, string[] subs)[])
        [
            ("/a", ["/a", "/a/aa", "/a/b", "/a/b/c", "/a/b/c/d"]),
            ("/a/aa", ["/a/aa"]),
            ("/a/b", ["/a/b", "/a/b/c", "/a/b/c/d"]),
            ("/a/b/c", ["/a/b/c", "/a/b/c/d"]),
            ("/a/b/c/d", ["/a/b/c/d"]),
            ("/x", ["/x", "/x/y", "/x/y/z"]),
            ("/x/y", ["/x/y", "/x/y/z"]),
            ("/x/y/z", ["/x/y/z"]),
        ]).Select(gs => (gen: new Location(gs.gen), subs: gs.subs.Select(s => new Location(s)).ToArray())).ToArray();

        var actual = AdPlatformService.ParseAndGroupLocations(locations);

        Assert.Equal(expected.Length, actual.Count);
        foreach (var (gen, subs) in expected)
            Assert.Equal(subs, actual[gen]);
    }

    [Fact]
    internal void ParseInlineData()
    {
        string qwer, jkl, asdf, zxcv;
        (qwer, jkl, asdf, zxcv) =
        (nameof(qwer), nameof(jkl), nameof(asdf), nameof(zxcv));
        var inlineData = $"""
            {qwer}:/a,/x
            {jkl}:/a/b/c/d
            {asdf}:/a/aa,/x/y
            {zxcv}:/a/b,/x/y/z
            """;
        var expected = (((string, string[])[])
        [
            ("/a", [qwer]),
            ("/a/aa", [qwer, asdf]),
            ("/a/b", [qwer, zxcv]),
            ("/a/b/c", [qwer, zxcv]),
            ("/a/b/c/d", [qwer, jkl, zxcv]),
            ("/x", [qwer]),
            ("/x/y", [qwer, asdf]),
            ("/x/y/z", [qwer, asdf, zxcv]),
        ]).ToDictionary(gs => new Location(gs.Item1), gs => gs.Item2.Select(s => new AdPlatform(s)).ToHashSet());

        var actual = AdPlatformService.ParseInlineData(inlineData);

        Assert.Equal(expected.Count, actual.Count);
        foreach (var (location, platforms) in expected)
            Assert.Equal(platforms, actual[location]);
    }

    [Fact]
    public async Task ParseAndLoad_Success()
        => Assert.True((await Service.ParseAndLoadAsync(dataExample)).Success);

    [Theory,
    InlineData(":/ru"),
    InlineData("Some:"),
    InlineData(" :\t"),
    InlineData("Some:/ru:/r")]
    public async Task ParseAndLoad_ParseException(string data)
    {
        var res = await Service.ParseAndLoadAsync(data);
        Assert.True(res.Failure);
        Assert.True(res.Exception is ParseException);
    }

    [Theory,
    InlineData("some:ru"),
    InlineData("some:/RU"),
    InlineData("some:/ru."),
    InlineData("some:/ru-msk")]
    public async Task ParseAndLoad_ValidationException(string data)
    {
        var res = await Service.ParseAndLoadAsync(data);
        Assert.True(res.Failure);
        Assert.True(res.Exception is ValidationException);
    }

    [Fact]
    public async Task FindAtLocation()
    {
        var res = await Service.ParseAndLoadAsync(dataExample);
        (string, int)[] expected = [
            ("/ru", 1),
            ("/ru/chelobl", 2),
            ("/ru/msk", 2),
            ("/ru/permobl", 2),
            ("/ru/svrd", 2),
            ("/ru/svrd/revda", 3),
            ("/ru/svrd/pervik", 3),
        ];

        var results = await Task.WhenAll(expected.Select(pair => Service.FindAtLocationAsync(pair.Item1).AsTask()));

        Assert.True(results.All(r => r.Success));
        var actual = results.Select(r => r.Value).ToArray();
        Assert.Equal(expected.Select(e => e.Item2), actual.Select(a => a.Count()));
    }

    [Fact]
    public async Task FindFindAtLocation_Uninitial()
    {
        var res = await Service.FindAtLocationAsync(new Location("/ru"));
        Assert.True(res.Failure);
        Assert.IsType<UninitializedException>(res.Exception);
    }

    [Fact]
    public async Task FindFindAtLocation_NotFound()
    {
        await Service.ParseAndLoadAsync("a:/a");
        var res = await Service.FindAtLocationAsync(new Location("/some/not/stored/path"));
        Assert.True(res.Failure);
        Assert.IsType<NotFoundException>(res.Exception);
    }

    [Fact]
    public async Task FindFindAtLocation_InvalidModel()
    {
        var res = await Service.FindAtLocationAsync("/some/WRONG/path");
        Assert.True(res.Failure);
        Assert.IsType<ValidationException>(res.Exception);
    }
}