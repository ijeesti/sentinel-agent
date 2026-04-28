using SentinelAgent.Application.Plugins;
using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Enums;
using SentinelAgent.Domain.Interfaces;
using SentinelAgent.Host;
using SentinelAgent.Infrastructure;
using SentinelAgent.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Centralized Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Configuration.AddEnvironmentVariables("QAPILOT_");
builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection(OllamaOptions.SectionName));

builder.Services.AddAgenttInfrastructure();
builder.Logging.AddConsole();

using IHost host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var agents = new AgentBundle(
    host.Services.GetRequiredService<ITicketGeneratorAgent>(),
    host.Services.GetRequiredService<IRootCauseAnalyzerAgent>(),
    host.Services.GetRequiredService<ITicketAwareTestRunner>(),
    host.Services.GetRequiredService<IIncidentRepository>()
);

// Assuming reading from API/Queue etc. 
FailureScenario[] myScenarios = [
    new(
        "System.NullReferenceException: Object reference not set... at OrderController.cs:line 47",
        FailureInputType.StackTrace,
        "Customer checkout flow interrupted."),
    new(
        "TimeoutException: Gateway timeout at https://payments.api... Database pool exhausted.",
        FailureInputType.CodeDiff,
        "Scale-out event occurring in Production."),
    new(
        "403 Forbidden: User 1042 attempted to access Admin dashboard.",
        FailureInputType.Manual,
        "Potential security regression in v2.4.1."),
    new(
        @"CRITICAL: SmtpException at 10.0.0.42. 
          Failed to notify user john.doe@external-vendor.com. 
          Stack trace contains sensitive source IP 172.16.254.1.",
        FailureInputType.StackTrace,
        "Email relay failure containing PII and internal network topology.")
];

await AgentHelper.RunDemoAsync(logger, agents, myScenarios);