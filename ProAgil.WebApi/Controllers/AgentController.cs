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
        /// Processa uma mensagem do usuário usando o agente inteligente
        /// </summary>
        /// <param name="request">A mensagem a ser processada</param>
        /// <returns>O resultado do processamento</returns>
        [HttpPost("process")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessMessage([FromBody] AgentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Message))
                {
                    return BadRequest(
                        new { success = false, message = "A mensagem não pode estar vazia." }
                    );
                }

                Console.WriteLine($"[AgentController] Recebida mensagem: {request.Message}");
                var result = await _agentService.ProcessMessage(request.Message);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AgentController] Erro: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message = "Falha ao processar a mensagem",
                        error = ex.Message,
                    }
                );
            }
        }

        /// <summary>
        /// Endpoint de verificação de saúde do agente
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", service = "AgentService" });
        }
    }

    /// <summary>
    /// Request DTO para o endpoint do agente
    /// </summary>
    public class AgentRequest
    {
        /// <summary>
        /// A mensagem do usuário a ser processada pelo agente
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
