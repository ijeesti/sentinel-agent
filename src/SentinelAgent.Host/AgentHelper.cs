using SentinelAgent.Domain.Domains;
using Microsoft.Extensions.Logging;
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
            if (ticket is null)
            {
                var msg = $"Ticket is rejected by Agent. {scenario.Context}";
                Console.WriteLine(msg);
                logger.LogWarning("{message}", msg);
                continue;
            }

            await agents.Repo.SaveAsync(ticket);
            var rcaRequest = new AnalyzeFailureRequest(
                RawFailureInput: scenario.RawInput,
                InputType: scenario.Type);

            var rca = await agents.Analyzer.AnalyzeAsync(rcaRequest);
            Console.WriteLine($"[PROCESSED] Ticket: {ticket.Id} | Title: {ticket.Title}");
            ReportHelper.AddBody(reportBuilder, rca: rca, ticket: ticket, location: location);
        }

        //Footer...
        reportBuilder.Append("""</div></body></html>""");
        const string reportFolder = "Reports";
        Directory.CreateDirectory(reportFolder);

        string fileName = Path.Combine(reportFolder, $"IncidentReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        await File.WriteAllTextAsync(fileName, reportBuilder.ToString(), default);
        Console.WriteLine($"\nDemo complete. Full data saved to: {fileName}");
    }

    static string ExtractSourceLocation(string stackTrace)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            stackTrace,
            @"in\s+(?<file>.*?):?line\s+(?<line>\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return "Unknown";
        }

        var filePath = match.Groups["file"].Value.Trim();
        var file = Path.GetFileName(filePath);
        var line = match.Groups["line"].Value;

        return $"{file}:{line}";
    }
}