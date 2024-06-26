using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using USOSConnector.Functions.Extensions;
using USOSConnector.Functions.Options;
using USOSConnector.Functions.Services.JwtService;
using USOSConnector.Functions.Services.UsosService;
using USOSConnector.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseMiddleware<JwtAuthorizationMiddleware>();
    })
    .ConfigureServices(services =>
    {
        // Options
        services.AddOptionsWithValidation<USOSOptions>(USOSOptions.SectionName);
        services.AddOptionsWithValidation<JwtOptions>(JwtOptions.SectionName);

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUsosService, UsosService>();

        // Other
        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddHttpClient<IUsosService, UsosService>();
    })
    .Build();

host.Run();
