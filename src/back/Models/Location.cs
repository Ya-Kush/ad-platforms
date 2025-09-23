using System.Text.RegularExpressions;
using AdPlatforms.Back.Models.Exceptions;

namespace AdPlatforms.Back.Models;

public readonly partial record struct Location(string Path)
{
    public string Path { get; } = SlashedAlphaRegex().IsMatch(Path) ? Path
        : throw new ModelException("The wronh format of location", new ArgumentException());

    [GeneratedRegex("^(/[a-z]+)+$")]
    private static partial Regex SlashedAlphaRegex();
}