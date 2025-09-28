using AdPlatforms.Back.Models;
using AdPlatforms.Back.Services;
using AdPlatforms.Common.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace AdPlatforms.Back.Endpoints;

public static class Platforms
{
    public static void MapPlatforms(this IEndpointRouteBuilder router)
    {
        var platforms = router.MapGroup("/platforms");
        platforms.MapGet("/", Get).WithName("GetPlatforms").WithDescription("Get ad platforms. Can be filtered by location");
        platforms.MapPut("/", LoadFile).WithName("LoadPlatforms").WithDescription("Load file of ad platfroms to the service. It operation overwrite all old data");
    }

    public static Results<Ok<IEnumerable<AdPlatform>>, NotFound, BadRequest<string>> Get([FromQuery(Name = "at")] string path, [FromServices] IAdPlatformService service)
        => service.FindAtLocation(path)
           .Match<IEnumerable<AdPlatform>, Results<Ok<IEnumerable<AdPlatform>>, NotFound, BadRequest<string>>>(
               platforms => Ok(platforms),
               e => e switch
               {
                   NotFoundException => NotFound(),
                   AdPlatformServiceException => BadRequest(e.Message),
                   _ => throw e
               });

    public static Results<NoContent, BadRequest<string>> LoadFile(Stream data, [FromServices] IAdPlatformService service)
        => service.ParseAndLoad(data)
            .Match<Results<NoContent, BadRequest<string>>>(
                () => NoContent(),
                e => e switch
                {
                    AdPlatformServiceException => BadRequest(e.Message),
                    _ => throw e
                });
}