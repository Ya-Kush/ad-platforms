namespace AdPlatforms.Back.Models;

public sealed class AdPlatform(string name)
{
    public string Name { get; init; } = name;
    public List<Location> Locations { get; } = [];

    public bool WorksAt(Location location) => Locations.Any(l => l.PartOf(location));
}