using System.Text.RegularExpressions;
using AdPlatforms.Back.Models.Exceptions;

namespace AdPlatforms.Back.Models;

public readonly partial record struct Location(string Name)
{
    public string Path { get; } = SlashedAlphaRegex().IsMatch(Name) ? Name : throw new ModelException(inner: new ArgumentException());

    public bool IsPartOf(Location location) => Path.Contains(location.Path);

    [GeneratedRegex("^[a-z]+(/[a-z]+)*$")]
    private static partial Regex SlashedAlphaRegex();
}