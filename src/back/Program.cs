using AdPlatforms.Back.Endpoints;
using AdPlatforms.Back.Middlewares;
using AdPlatforms.Back.Services;
using Scalar.AspNetCore;

var bldr = WebApplication.CreateBuilder(args);
var conf = bldr.Configuration;
var envi = bldr.Environment;
{
    bldr.WebHost.UseKestrel(opts => opts.AddServerHeader = false);
    bldr.Host.UseDefaultServiceProvider(opts =>
        opts.ValidateScopes = opts.ValidateOnBuild = envi.IsDevelopment());
}

var srvs = bldr.Services;
{
    srvs.AddLogging();
    srvs.AddProblemDetails();
    srvs.AddExceptionHandler<GlobalExceptionHandler>();

    srvs.AddScoped<IAdPlatformService, AdPlatformService>();

    srvs.AddOpenApi();
    srvs.AddHealthChecks();
}

var appl = bldr.Build();
{
    appl.UseExceptionHandler();

    if (envi.IsDevelopment())
    {
        appl.MapOpenApi();
        appl.MapScalarApiReference();
    }

    appl.MapHealthChecks("/healthz");
    appl.MapPlatforms();

    appl.Run();
}