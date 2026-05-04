using SentinelAgent.Application.Plugins;
using Xunit;

namespace SentinelAgent.UnitTests.Application.Agents;

[Trait("Category", "Agents")]
[Trait("Module", "SecurityGuardrailPluginTests")]
public class SecurityGuardrailPluginTests
{
    private readonly SecurityGuardrailPlugin _plugin = new();

    [Fact]
    public void Sanitize_ContainsSensitiveData_RedactsCorrectly()
    {
        // Arrange
        string raw = "Error at 192.168.1.1. User: john.doe@company.com. Conn: Password=MySecret123;";

        // Act
        var result = _plugin.Sanitize(raw);

        // Assert
        Assert.Contains("[IP_REDACTED]", result);
        Assert.Contains("[EMAIL_REDACTED]", result);
        Assert.Contains("Password=********", result);
        Assert.DoesNotContain("192.168", result);
        Assert.DoesNotContain("john.doe", result);
        Assert.DoesNotContain("MySecret123", result);
    }

    [Fact]
    public void Sanitize_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _plugin.Sanitize(string.Empty));
        Assert.Null(_plugin.Sanitize(null!));
    }
}