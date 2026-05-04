using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using SentinelAgent.Application.Agents;
using SentinelAgent.Domain.Aggregates;
using SentinelAgent.Domain.Domains;
using SentinelAgent.Domain.Enums;
using SentinelAgent.Domain.Interfaces;

using System.Text.Json;

namespace SentinelAgent.UnitTests.Application.Agents.Helpers;


public sealed class AgentHelper
{
    public static string ValidTicketJson(
        string title = "NullReferenceException in PaymentService",
        string severity = "Critical",
        double confidence = 0.9) => JsonSerializer.Serialize(new
        {
            title,
            description = "Service throws NRE when body is null.",
            rootCause = new
            {
                summary = "Null body not guarded",
                technicalDetail = "PaymentService.Process() dereferences request without null check.",
                confidenceScore = confidence,
                suggestedFix = "Add null guard at entry point."
            },
            acceptanceCriteria = new[] { "Returns 400 on null input", "No unhandled exception" },
            reproductionSteps = new[] { "POST /payments with empty body" },
            severity
        });

    public static (Kernel kernel, FakeChatCompletion fake) BuildKernel(string llmResponse)
    {
        var fake = new FakeChatCompletion(llmResponse);
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>(fake);
        return (builder.Build(), fake);
    }

    public static TicketGeneratorAgent CreateTicketGeneratorAgent(
        Kernel kernel,
        DeduplicationStore? dedup = null,
        IAgentDecisionEngine? engine = null,
        IReportGenerator? reporter = null)
    {
        dedup ??= new DeduplicationStore();
        engine ??= new RuleBasedDecisionEngine();

        if (reporter is null)
        {
            var reporterMock = new Mock<IReportGenerator>();
            reporterMock
                .Setup(r => r.GenerateIncidentReport(It.IsAny<IncidentTicket>()))
                .Returns("<html>report</html>");
            reporter = reporterMock.Object;
        }

        var logger = Mock.Of<ILogger<TicketGeneratorAgent>>();
        return new TicketGeneratorAgent(kernel, logger, dedup, engine, reporter);
    }

    public static GenerateTicketRequest BuildRequest(
        string raw = "System.NullReferenceException: Object reference not set",
        string? context = null) => new(
            RawFailureInput: raw,
            InputType: FailureInputType.StackTrace,
            AdditionalContext: context,
            SourceLocation: "PaymentService.cs:42");
}