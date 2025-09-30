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
    Result<IEnumerable<AdPlatform>> FindAtLocation(Location location);
    Result<IEnumerable<AdPlatform>> FindAtLocation(string path);
    Result ParseAndLoad(string data);
    Result ParseAndLoad(Stream data);
    Task<Result> ParseAndLoadAsync(Stream data);
}

public sealed class AdPlatformService : IAdPlatformService
{
    FrozenDictionary<Location, FrozenSet<AdPlatform>>? _data;
    readonly ScopedReaderWriterLockSlim _lock = new();

    public Result<IEnumerable<AdPlatform>> FindAtLocation(Location location)
    {
        using (_lock.ScopedRead())
        {
            return _data is null ? new UninitializedException("Attempt to get value from an uninitialized service")
                : _data.TryGetValue(location, out var value) ? value : new NotFoundException();
        }
    }

    public Result<IEnumerable<AdPlatform>> FindAtLocation(string path)
        => Result.Try(() => new Location(path))
            .Match(FindAtLocation, e => e is ModelException ? new ValidationException(e.Message, e) : e);

    public Result ParseAndLoad(string text) => Result.Try(() => Parser.GetDataLines(text)).Match(ParseAndLoad, _ => _);
    public Result ParseAndLoad(Stream stream) => Result.Try(() => Parser.GetDataLinesAsync(stream).ToBlockingEnumerable()).Match(ParseAndLoad, _ => _);
    public async Task<Result> ParseAndLoadAsync(Stream stream) => await Result.Try(() => Parser.GetDataLinesAsync(stream)).Match(ParseAndLoadAsync, e => Task.FromResult(Result.Fail(e)));

#warning need to test ParseAndLoad
    internal Result ParseAndLoad(IEnumerable<string> lines)
    {
        using (_lock.ScopedWrite())
        {
            return Result.Try(() =>
                    Locator.LocatePlatforms(Parser.ParseDataLines(lines)))
                .Match(
                    v => { _data = v.ToFrozen(); return Result.Ok(); },
                    e => e is ModelException ? new ValidationException(e.Message, e) : e);
        }
    }

#warning need to test ParseAndLoadAsync
    internal async Task<Result> ParseAndLoadAsync(IAsyncEnumerable<string> lines)
    {
        using (_lock.ScopedWrite())
        {
            return await Result.TryAsync(async () =>
                    Locator.LocatePlatforms(await Parser.ParseDataLinesAsync(lines)))
                .MatchAsync(
                    v => { _data = v.ToFrozen(); return Result.Ok(); },
                    e => e is ModelException ? new ValidationException(e.Message, e) : e);
        }
    }

    internal sealed class HashSetDictionary<K, V> : Dictionary<K, HashSet<V>> where K : notnull
    {
        public void Add(K key, V val) => (TryGetValue(key, out var set) ? set : this[key] = []).Add(val);
        public FrozenDictionary<K, FrozenSet<V>> ToFrozen() => this.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToFrozenSet());
    }

    internal static class Locator
    {
        public static HashSetDictionary<Location, AdPlatform> LocatePlatforms(ILookup<Location, AdPlatform> data)
        {
            HashSetDictionary<Location, AdPlatform> res = [];

            var platformsAt = data.ToFrozenDictionary(x => x.Key, x => x.ToFrozenSet());
            var locations = UnravelAndGroupLocations(platformsAt.Keys);

            foreach (var (location, subs) in locations)
                foreach (var sub in subs)
                    foreach (var platform in platformsAt.TryGetValue(location, out var platforms) ? platforms : [])
                        res.Add(sub, platform);

            return res;
        }

        public static Dictionary<Location, HashSet<Location>> UnravelAndGroupLocations(IEnumerable<Location> locations)
        {
            HashSetDictionary<Location, Location> res = [];
            foreach (var location in locations)
            {
                if (res.ContainsKey(location)) continue;
                var generals = UnravelLocation(location).ToArray();
                for (var i = 1; i <= generals.Length; i++)
                    for (var j = i; j <= generals.Length; j++)
                        res.Add(generals[^i], generals[^j]);
            }
            return res;
        }

#warning need to test Locator.UnravelLocation
        public static IEnumerable<Location> UnravelLocation(Location location)
        {
            for (var i = location.Path.Length; i > 0; i = location.Path.LastIndexOf('/', i - 1))
                yield return new Location(location.Path[0..i]);
        }
    }

    internal static class Parser
    {
        const StringSplitOptions TrimAndSift = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        internal static IEnumerable<string> GetDataLines(string data)
        {
            if (data.IsWhiteSpace()) throw new ValidationException("Data was blank");
            var lines = data.Split(['\n', '\r'], TrimAndSift);
            return lines;
        }

#warning need to test GetDataLinesAsync
        internal static async IAsyncEnumerable<string> GetDataLinesAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync()) is { })
                if (line.IsWhiteSpace()) continue;
                else yield return line.Trim();
        }

        public static ILookup<Location, AdPlatform> ParseDataLines(IEnumerable<string> lines)
        {
            return lines
                .Select(ParseDataLine)
                .SelectMany(p => p.locations, (p, l) => (location: l, p.platform))
                .ToLookup(lp => lp.location, lp => lp.platform);
        }

#warning need to test Parser.ParseDataLinesAsync
        public static async Task<ILookup<Location, AdPlatform>> ParseDataLinesAsync(IAsyncEnumerable<string> lines)
        {
            HashSetDictionary<Location, AdPlatform> res = [];
            await foreach (var line in lines)
            {
                var (platform, locations) = ParseDataLine(line, res.Count);
                foreach (var loc in locations) res.Add(loc, platform);
            }
            return res
                .SelectMany(x => x.Value, (x, y) => (x.Key, Value: y))
                .ToLookup(x => x.Key, x => x.Value);
        }

#warning need to test Parser.ParseDataLine
        public static (AdPlatform platform, IEnumerable<Location> locations) ParseDataLine(string line, int i)
            => line.Split(':', TrimAndSift) is [var platform, var paths]
                ? (new(platform), paths.Split(',', TrimAndSift).Select(p => new Location(p)))
                : throw new ParseException($"The wrong format of data. Occurred in {i}th row");
    }
}