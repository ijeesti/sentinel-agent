using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Enums;
using Microsoft.Extensions.Logging;
using OpenAI.Realtime;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace SentinelAgent.Host;

internal static class AgentHelper
{
    internal static async Task RunDemoAsync(
        ILogger logger,
        AgentBundle agents,
        params FailureScenario[] scenarios)
    {
        Console.WriteLine("""
    ╔═══════════════════════════════════════════════════════╗
    ║   SentinelAgent — AI-Powered QA Automation Demo       ║
    ║               Powered by Semantic Kernel              ║
    ╚═══════════════════════════════════════════════════════╝
    """);

        var reportBuilder = new StringBuilder();
       // reportBuilder.Append("<html><head><style>body{font-family:sans-serif; padding:20px;} .card{border:1px solid #ccc; margin-bottom:20px; padding:15px; border-radius:8px;} .status{color:green; font-weight:bold;}</style></head><body><h1>SentinelAgent Analysis Report</h1>");
        ReportHelper.AddReportHeader(reportBuilder);
        foreach (var scenario in scenarios)
        {
            LogActions.LogCallingAgent(logger, $"Processing {scenario.Type}");

            // 1. Generate Ticket
            var location = ExtractSourceLocation(scenario.RawInput);
            var genRequest = new GenerateTicketRequest(
                RawFailureInput: scenario.RawInput,
                InputType: scenario.Type,
                SourceLocation: location,
                AdditionalContext: scenario.Context);

            var ticket = await agents.Generator.GenerateTicketAsync(genRequest);
            await agents.Repo.SaveAsync(ticket);

            // 2. Perform RCA
            var rcaRequest = new AnalyzeFailureRequest(
                RawFailureInput: scenario.RawInput,
                InputType: scenario.Type);

            var rca = await agents.Analyzer.AnalyzeAsync(rcaRequest);
            Console.WriteLine($"[PROCESSED] Ticket: {ticket.Id} | Title: {ticket.Title}");
            ReportHelper.AddBody(reportBuilder, rca: rca, ticket:ticket, location: location);
        }

        //Footer...
        reportBuilder.Append("""</div></body></html>""");
        const string reportFolder = "Reports";
        Directory.CreateDirectory(reportFolder);
        
        string fileName = Path.Combine(reportFolder, $"IncidentReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        await File.WriteAllTextAsync(fileName, reportBuilder.ToString());
        Console.WriteLine($"\nDemo complete. Full data saved to: {fileName}");
    }
    static string ExtractSourceLocation(string stackTrace)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            stackTrace,
            @"in\s+(?<file>.+):line\s+(?<line>\d+)");

        if (!match.Success)
            return "Unknown";

        var file = Path.GetFileName(match.Groups["file"].Value);
        var line = match.Groups["line"].Value;

        return $"{file}:{line}";
    }
}