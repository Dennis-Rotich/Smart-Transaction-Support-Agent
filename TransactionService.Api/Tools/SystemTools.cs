using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace TransactionService.Api.Tools;

[McpServerToolType]
public class SystemTools
{
    [McpServerTool]
    [Description("Pings the backend to verify AI connectivity and server health.")]
    public async Task<string> PingMcp()
    {
        return "MCP Server is active, attached, and ready for tool registrations.";
    }
}