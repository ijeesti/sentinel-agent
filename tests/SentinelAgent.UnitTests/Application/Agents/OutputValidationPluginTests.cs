using SentinelAgent.Application.Plugins;
using Xunit;

namespace SentinelAgent.UnitTests.Application.Agents;

[Trait("Category", "Agents")]
[Trait("Module", "OutputValidationPluginTests")]
public class OutputValidationPluginTests
{
    private readonly OutputValidationPlugin _plugin = new();

    [Theory]
    [InlineData("Everything looks good.")]
    [InlineData("The system is stable.")]
    public void ValidateIncidentReport_ProfessionalInput_ReturnsOriginalWithDisclaimer(string input)
    {
        var result = _plugin.ValidateIncidentReport(input);
        Assert.Contains("AI-Generated Analysis", result);
        Assert.Contains(input, result);
    }

    [Theory]
    [InlineData("I think the server is down.")]
    [InlineData("Maybe we should restart.")]
    [InlineData("Oops, something went wrong.")]
    public void ValidateIncidentReport_UnprofessionalInput_ReturnsFlaggedMessage(string input)
    {
        var result = _plugin.ValidateIncidentReport(input);
        Assert.Equal("AI output flagged for manual review: Low confidence language detected.", result);
    }
}

