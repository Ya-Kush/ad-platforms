using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatforms.Back.Endpoints;

public static class Platforms
{
    public static void MapPlatforms(this IEndpointRouteBuilder router)
    {
        var platforms = router.MapGroup("/platforms");
        platforms.MapGet("/", Get).WithName("GetPlatforms").WithDescription("Get ad platforms. Can be filtered by location");
        platforms.MapPut("/", Load).WithName("LoadPlatforms").WithDescription("Load ad platfroms to the service. It operation overwrite all old data");
    }

    public record struct GetResponse(string Name);
    public static async Task<Results<Ok<GetResponse[]>, BadRequest>> Get([FromQuery(Name = "at")] string? location, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }

    public static async Task<Results<NoContent, ValidationProblem>> Load([FromBody] string list, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}