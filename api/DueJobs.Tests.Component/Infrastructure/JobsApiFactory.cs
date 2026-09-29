using DueJobs.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DueJobs.Tests.Component.Infrastructure;

public sealed class JobsApiFactory : WebApplicationFactory<Program>
{
    public TestClock Clock { get; } = new();
    public RecordingDeliveryGateway Delivery { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("SeedExampleJobs", "false");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(Clock);
            services.RemoveAll<IDeliveryGateway>();
            services.AddSingleton<IDeliveryGateway>(Delivery);
        });
    }
}

public sealed class TestClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);
}
