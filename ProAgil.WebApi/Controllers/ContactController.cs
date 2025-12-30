using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProAgil.WebApi.Dtos;

namespace ProAgil.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(ContactDto model)
        {
            try
            {
                Console.WriteLine("Processing contact...");
                //TODO: implement contact handling here
                return Created($"/api/contact/{model.Email}", model);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Contact execution failed");
            }
        }
    }
}

