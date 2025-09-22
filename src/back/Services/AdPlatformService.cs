using System.Collections.Frozen;
using AdPlatforms.Back.Common.Results;
using AdPlatforms.Back.Models;
using AdPlatforms.Back.Models.Exceptions;

namespace AdPlatforms.Back.Services;

public class AdPlatformServiceException(string? msg = null, Exception? inner = null) : ApplicationException(msg, inner);
public class UninitializedException(string? msg = null, Exception? inner = null) : AdPlatformServiceException(msg, inner);
public class ParseException(string? msg = null, Exception? inner = null) : AdPlatformServiceException(msg, inner);
public class ValidationException(string? msg = null, Exception? inner = null) : AdPlatformServiceException(msg, inner);
public class NotFoundException(string? msg = null, Exception? inner = null) : AdPlatformServiceException(msg, inner);

public sealed class AdPlatformService
{
    readonly static Lock _lock = new();
    static FrozenDictionary<Location, FrozenSet<AdPlatform>>? _platforms;

    public Result TryParseAndLoad(string data) => Result.Try(() =>
    {
        lock (_lock) _platforms = data.Split('\n')
            .Select((l, i) => l.Split(':') is [var platform, var paths]
                ? new AdPlatform(platform, paths.Split(',').Select(p => new Location(p)))
                : throw new ParseException($"The wrong format of data. Occurred in {i}th row"))
            .SelectMany(p => p.Locations, (p, l) => (location: l, platform: p))
            .GroupBy(lp => lp.location, lp => lp.platform)
            .ToFrozenDictionary(group => group.Key, group => group.ToFrozenSet());
    });

    public Result<IEnumerable<AdPlatform>> FindAtLocation(Location location)
    {
        lock (_lock) return _platforms is { }
            ? _platforms.TryGetValue(location, out var value) ? value: new NotFoundException()
            : new UninitializedException();
    }

    public Result<IEnumerable<AdPlatform>> FindAtLocation(string path) => Result.Try(
        () => new Location(path)).Match(FindAtLocation,
            e => e is ModelException ? new ValidationException(e.Message, e) : e);
}