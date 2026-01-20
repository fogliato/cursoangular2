using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProAgil.Domain.Agent;

namespace ProAgil.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        /// <summary>
        /// Processes a user message using the intelligent agent
        /// </summary>
        /// <param name="request">The message to be processed</param>
        /// <returns>The processing result</returns>
        [HttpPost("process")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessMessage([FromBody] AgentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Message))
                {
                    return BadRequest(
                        new { success = false, message = "The message cannot be empty." }
                    );
                }

                Console.WriteLine($"[AgentController] Received message: {request.Message}");
                var result = await _agentService.ProcessMessage(request.Message);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AgentController] Error: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message = "Failed to process the message",
                        error = ex.Message,
                    }
                );
            }
        }

        /// <summary>
        /// Agent health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "AgentService" });
        }
    }

    /// <summary>
    /// Request DTO for the agent endpoint
    /// </summary>
    public class AgentRequest
    {
        /// <summary>
        /// The user message to be processed by the agent
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
