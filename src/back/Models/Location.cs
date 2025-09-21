namespace AdPlatforms.Back.Models;

public sealed record Location(string Name, Location? Owner)
{
    public bool PartOf(Location location) => location == this || Owner is { } && Owner.PartOf(location);
}