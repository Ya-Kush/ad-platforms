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
    public void ParseAndLoad_Success() => Assert.True(Service.ParseAndLoad(dataExample).Success);

    [Theory,
    InlineData(":/ru"),
    InlineData("Some:"),
    InlineData(" :\t"),
    InlineData("Some:/ru:/r")]
    public void ParseAndLoad_ParseException(string data)
    {
        var res = Service.ParseAndLoad(data);
        Assert.True(res.Failure && res.Exception is ParseException);
    }

    [Theory,
    InlineData("some:ru"),
    InlineData("some:/RU"),
    InlineData("some:/ru."),
    InlineData("some:/ru-msk")]
    public void ParseAndLoad_ValidationException(string data)
    {
        var res = Service.ParseAndLoad(data);
        Assert.True(res.Failure && res.Exception is ValidationException);
    }

    [Fact]
    public void FindAtLocation()
    {
        var res = Service.ParseAndLoad(dataExample);
        (string, int)[] expected = [
            ("/ru", 1),
            ("/ru/chelobl", 2),
            ("/ru/msk", 2),
            ("/ru/permobl", 2),
            ("/ru/svrd", 2),
            ("/ru/svrd/revda", 3),
            ("/ru/svrd/pervik", 3),
        ];

        var results = expected.Select(pair => Service.FindAtLocation(pair.Item1));

        Assert.True(results.All(r => r.Success));
        var actual = results.Select(r => r.Value).ToArray();
        Assert.Equal(expected.Select(e => e.Item2), actual.Select(a => a.Count()));
    }

    [Fact]
    public void FindAtLocation_Uninitial()
    {
        var res = Service.FindAtLocation(new Location("/ru"));
        Assert.True(res.Failure);
        Assert.IsType<UninitializedException>(res.Exception);
    }

    [Fact]
    public void FindAtLocation_NotFound()
    {
        Service.ParseAndLoad("a:/a");
        var res = Service.FindAtLocation(new Location("/some/not/stored/path"));
        Assert.True(res.Failure);
        Assert.IsType<NotFoundException>(res.Exception);
    }

    [Fact]
    public void FindAtLocation_InvalidModel()
    {
        var res = Service.FindAtLocation("/some/WRONG/path");
        Assert.True(res.Failure);
        Assert.IsType<ValidationException>(res.Exception);
    }

    public class LocatorTest
    {
        [Fact]
        internal void UnravelAndGroupLocations()
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
            ]).Select(gs => (
                    gen: new Location(gs.gen),
                    subs: gs.subs.Select(s => new Location(s)).ToArray()))
                .ToArray();

            var actual = AdPlatformService.Locator.UnravelAndGroupLocations(locations);

            Assert.Equal(expected.Length, actual.Count);
            foreach (var (gen, subs) in expected)
                Assert.Equal(subs, actual[gen]);
        }

        [Fact]
        internal void LocatePlatforms()
        {
            string qwer, jkl, asdf, zxcv;
            (qwer, jkl, asdf, zxcv) = (nameof(qwer), nameof(jkl), nameof(asdf), nameof(zxcv));
            var data = (string[])[
                $"{qwer}:/a,/x",
                $"{jkl}:/a/b/c/d",
                $"{asdf}:/a/aa,/x/y",
                $"{zxcv}:/a/b,/x/y/z",
            ];
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

            var lookup = AdPlatformService.Parser.ParseDataLines(data);
            var actual = AdPlatformService.Locator.LocatePlatforms(lookup);

            Assert.Equal(expected.Count, actual.Count);
            foreach (var (location, platforms) in expected)
                Assert.Equal(platforms, actual[location]);
        }
    }

    public class ParserTest
    {
        [Fact]
        internal void GetDataLines()
        {
            var data = $"""
                qwer:/a,/x
                jkl:/a/b/c/d
                asdf:/a/aa,/x/y
                zxcv:/a/b,/x/y/z
                """;
            var expected = (string[])
            [
                "qwer:/a,/x",
                "jkl:/a/b/c/d",
                "asdf:/a/aa,/x/y",
                "zxcv:/a/b,/x/y/z"
            ];

            var res = AdPlatformService.Parser.GetDataLines(data);

            Assert.Equal(expected, res);
        }

        [Fact]
        internal void GetDataLines_ValidationException()
        {
            var wrong = (string[])["", " ", "\t\n"];
            foreach (var w in wrong) Assert.Throws<ValidationException>(
                () => AdPlatformService.Parser.GetDataLines(w));
        }

        [Fact]
        internal void Parser_ParseDataLines()
        {
            string qwer, jkl, asdf, zxcv;
            (qwer, jkl, asdf, zxcv) = (nameof(qwer), nameof(jkl), nameof(asdf), nameof(zxcv));
            var data = (string[])[
                $"{qwer}:/a,/x",
                $"{jkl}:/a/b/c/d",
                $"{asdf}:/a/aa,/x/y",
                $"{zxcv}:/a/b,/x/y/z",
            ];
            var expected = (((string, string[])[])
            [
                ("/a", [qwer]),
                ("/a/aa", [asdf]),
                ("/a/b", [zxcv]),
                ("/a/b/c/d", [jkl]),
                ("/x", [qwer]),
                ("/x/y", [asdf]),
                ("/x/y/z", [zxcv]),
            ])
                .SelectMany(x => x.Item2, (x, y) => (x.Item1, y))
                .ToLookup(x => new Location(x.Item1), x => new AdPlatform(x.Item2));

            var actual = AdPlatformService.Parser.ParseDataLines(data);

            Assert.Equal(expected.Count, actual.Count);
            foreach (var group in expected)
                Assert.Equal(group, actual[group.Key]);
        }
    }
}