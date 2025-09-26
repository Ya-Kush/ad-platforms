using System.Collections.Frozen;
using AdPlatforms.Back.Models;
using AdPlatforms.Back.Models.Exceptions;
using AdPlatforms.Common.Results;
using AdPlatforms.Common.Synchronization;

namespace AdPlatforms.Back.Services;

public class AdPlatformServiceException(string? msg = null, Exception? inner = null) : ApplicationException(msg, inner);
public class UninitializedException(string? msg = null, Exception? inner = null) : AdPlatformServiceException(msg, inner);
public class ParseException(string? msg = null, Exception? inner = null) : AdPlatformServiceException(msg, inner);
public class ValidationException(string? msg = null, Exception? inner = null) : AdPlatformServiceException(msg, inner);
public class NotFoundException(string? msg = null, Exception? inner = null) : AdPlatformServiceException(msg, inner);

public interface IAdPlatformService
{
    Result<IEnumerable<AdPlatform>> FindAtLocationAsync(Location location);
    Result<IEnumerable<AdPlatform>> FindAtLocation(string path);
    Result ParseAndLoad(string data);
}

public sealed class AdPlatformService : IAdPlatformService
{
    FrozenDictionary<Location, FrozenSet<AdPlatform>>? _data;
    readonly ScopedReaderWriterLockSlim _lock = new();

    public Result<IEnumerable<AdPlatform>> FindAtLocationAsync(Location location)
    {
        using (_lock.ScopedRead())
        {
            return _data is null ? new UninitializedException("Attempt to get value from an uninitialized service")
                : _data.TryGetValue(location, out var value) ? value : new NotFoundException();
        }
    }

    public Result<IEnumerable<AdPlatform>> FindAtLocation(string path)
        => Result.Try(() => new Location(path))
            .Match(FindAtLocationAsync, e => e is ModelException ? new ValidationException(e.Message, e) : e);

    public Result ParseAndLoad(string data)
    {
        using (_lock.ScopedWrite())
        {
            var res = Result.Try(() => ParseInlineData(data));
            if (res.Success) _data = res.Value.ToFrozenDictionary(x => x.Key, x => x.Value.ToFrozenSet());

            return res.Match(_ => Result.Ok(), e => e is ModelException ? new ValidationException(e.Message, e) : e);
        }
    }


    internal static Dictionary<Location, HashSet<AdPlatform>> ParseInlineData(string data)
    {
        var trimAndSift = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
        var lines = data.Split(["\n", "\n\r"], trimAndSift);
        if (lines is []) throw new ValidationException("Data was blank");

        Dictionary<Location, HashSet<AdPlatform>> res = new(lines.Length);
        void add(Location l, AdPlatform p) => (res.TryGetValue(l, out var set) ? set : res[l] = []).Add(p);

        var adPlatformsAt = lines
            .Select(parseDataLineToAdPlatform)
            .SelectMany(p => p.locations, (p, l) => (location: l, p.platform))
            .GroupBy(lp => lp.location, lp => lp.platform)
            .ToFrozenDictionary(g => g.Key, g => g.ToFrozenSet());

        var locations = ParseAndGroupLocations(adPlatformsAt.Keys);

        foreach (var (location, subs) in locations)
            foreach (var sub in subs)
                foreach (var platform in adPlatformsAt.TryGetValue(location, out var platforms) ? platforms : [])
                    add(sub, platform);

        return res;

        (AdPlatform platform, IEnumerable<Location> locations) parseDataLineToAdPlatform(string line, int i)
            => line.Split(':', trimAndSift) is [var platform, var paths]
            ? (new(platform), paths.Split(',', trimAndSift).Select(p => new Location(p)))
            : throw new ParseException($"The wrong format of data. Occurred in {i}th row");
    }

    internal static IReadOnlyDictionary<Location, IReadOnlySet<Location>> ParseAndGroupLocations(IEnumerable<Location> locations)
    {
        Dictionary<Location, HashSet<Location>> res = [];
        foreach (var location in locations)
        {
            if (res.ContainsKey(location)) continue;
            var generals = parseLocation(location).ToArray();
            for (var i = 1; i <= generals.Length; i++)
                for (var j = i; j <= generals.Length; j++)
                    add(generals[^i], generals[^j]);
        }
        return res.ToFrozenDictionary(d => d.Key, d => (IReadOnlySet<Location>)d.Value.ToFrozenSet()).AsReadOnly();

        void add(Location l, Location p) => (res.TryGetValue(l, out var set) ? set : res[l] = []).Add(p);
        static IEnumerable<Location> parseLocation(Location location)
        {
            for (var i = location.Path.Length; i > 0; i = location.Path.LastIndexOf('/', i - 1))
                yield return new Location(location.Path[0..i]);
        }
    }
}
