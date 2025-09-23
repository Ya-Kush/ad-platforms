using AdPlatforms.Back.Models.Exceptions;

namespace AdPlatforms.Back.Models.Tests;

public sealed class LocationTest
{
    [Theory,
    InlineData(""),
    InlineData(" \t"),
    InlineData("."),
    InlineData("/"),
    InlineData("ru"),
    InlineData("/Ru"),
    InlineData("/ru-msk"),
    InlineData("//ru"),
    InlineData(" /ru"),
    InlineData("/ru.")]
    public void Ctor_WhrongPathFormat(string path) => Assert.Throws<ModelException>(() => new Location(path));

    [Theory,
    InlineData("/ru"),
    InlineData("/ru/msk"),
    InlineData("/a/b/c/d")]
    public void Ctor_CorrectPathFormat(string path) => new Location(path);
}