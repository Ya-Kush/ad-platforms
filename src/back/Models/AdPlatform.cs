using AdPlatforms.Back.Models.Exceptions;

namespace AdPlatforms.Back.Models;

public sealed partial record AdPlatform(string Name, IEnumerable<Location> Locations)
{
    readonly HashSet<Location> _locations = [.. Locations];

    public string Name { get; } = !Name.IsWhiteSpace() ? Name : throw new ModelException(inner: new ArgumentException("The wrong format", nameof(Name)));
    public IEnumerable<Location> Locations => _locations;

    public bool WorksAt(Location location) => Locations.Contains(location);
}