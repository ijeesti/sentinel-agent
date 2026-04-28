using SentinelAgent.Application.Agents;
using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Infrastructure.Configuration;
using SentinelAgent.Infrastructure.Reports;
using SentinelAgent.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace SentinelAgent.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddAgenttInfrastructure(
        this IServiceCollection services)
    {
        services.AddOllamaKernel();
        services.AddAgents();
        services.AddRepositories();
        return services;
    }

    private static IServiceCollection AddOllamaKernel(this IServiceCollection services)
    {
        // Register the Semantic Kernel with Ollama as the chat completion backend
        services.AddTransient<Kernel>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;

            var builder = Kernel.CreateBuilder();
            var client = new HttpClient 
            { 
                Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
            };
            builder.AddOpenAIChatCompletion(
                modelId: options.ModelId,
                apiKey: "not-needed",
                endpoint: new Uri(options.BaseUrl),
                httpClient: client);
            return builder.Build();
        });
        return services;
    }

    private static IServiceCollection AddAgents(this IServiceCollection services)
    {
        services.AddTransient<ITicketGeneratorAgent, TicketGeneratorAgent>();
        services.AddTransient<IRootCauseAnalyzerAgent, RootCauseAnalyzerAgent>();
        services.AddTransient<ITicketAwareTestRunner, TicketAwareTestRunnerAgent>();
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // In-memory by default; swap for a real persistence implementation in production
        services.AddSingleton<IIncidentRepository, InMemoryIncidentRepository>();
        services.AddSingleton<IReportGenerator, HtmlReportGenerator>();
        return services;
    }
}
