using AdPlatforms.Back.Models.Exceptions;

namespace AdPlatforms.Back.Models;

public readonly partial record struct AdPlatform(string Name)
{
    public string Name { get; } = !Name.IsWhiteSpace() ? Name
        : throw new ModelException("The wrong name format of ad platform", new ArgumentException("The wrong format", nameof(Name)));
}