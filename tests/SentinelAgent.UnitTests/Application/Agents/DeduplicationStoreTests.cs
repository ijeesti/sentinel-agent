using SentinelAgent.Application.Agents;
using SentinelAgent.UnitTests.Application.Agents.Helpers;

using Xunit;

namespace SentinelAgent.UnitTests.Application.Agents;

[Trait("Category", "Agents")]
[Trait("Module", "DeduplicationStoreTests")]
public class DeduplicationStoreTests
{
    [Fact]
    public async Task GenerateTicketAsync_DuplicateInput_ReturnsNull()
    {

        var ticket = AgentHelper.ValidTicketJson(severity: "Critical");
        var (kernel, _) = AgentHelper.BuildKernel(ticket);
        var dedup = new DeduplicationStore();
        var agent = AgentHelper.CreateTicketGeneratorAgent(kernel, dedup);
        var request = AgentHelper.BuildRequest("same error every time");

        // First call registers it
        await agent.GenerateTicketAsync(request);

        // Second call is a duplicate → null
        var duplicate = await agent.GenerateTicketAsync(request);

        Assert.Null(duplicate);
    }

    [Fact]
    public async Task GenerateTicketAsync_UniqueInputs_BothReturnTickets()
    {
        var dedup = new DeduplicationStore();

        var (k1, _) = AgentHelper.BuildKernel(AgentHelper.ValidTicketJson(title: "Error A"));
        var agent1 = AgentHelper.CreateTicketGeneratorAgent(k1, dedup);
        var t1 = await agent1.GenerateTicketAsync(AgentHelper.BuildRequest("error-alpha"));

        var (k2, _) = AgentHelper.BuildKernel(AgentHelper.ValidTicketJson(title: "Error B"));
        var agent2 = AgentHelper.CreateTicketGeneratorAgent(k2, dedup);
        var t2 = await agent2.GenerateTicketAsync(AgentHelper.BuildRequest("error-beta"));

        Assert.NotNull(t1);
        Assert.NotNull(t2);
    }
}
