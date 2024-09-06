using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CardGameManagement.Api.Configuration.HealthChecks;

public static class HealthCheck
{
    public static void ConfigureHealthChecks(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddHealthChecks().AddOracle(
            configuration.GetConnectionString("FiapOracleConnection") ?? string.Empty, 
            healthQuery: "SELECT 1 FROM DUAL", 
            name: "Oracle Health Check", 
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] {"feedback", "database", "oracle"});

        services.AddHealthChecksUI(opt =>
        {
            opt.SetEvaluationTimeInSeconds(15);
            opt.MaximumHistoryEntriesPerEndpoint(60);
            opt.SetApiMaxActiveRequests(1);
            opt.AddHealthCheckEndpoint("feedback", "/api/health");
        }).AddInMemoryStorage();
    }
}