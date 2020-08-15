using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProAgil.WebApi.Dtos;

namespace ProAgil.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContatoController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(ContatoDto model)
        {
            try
            {
                Console.WriteLine("Realizando contato...");
                //TODO: fazer a implementação aqui do contato
                return Created($"/api/conato/{model.Email}", model);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na execução do do contato");
            }
        }
    }
}