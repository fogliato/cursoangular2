using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProAgil.Repository;

namespace ProAgil.WebApi.Controllers
{
    [ApiController]
    [Route ("api/[controller]")]
    public class ValuesController : ControllerBase
    {

        public ProAgilContext _context { get; }

        public ValuesController (ProAgilContext context)
        {
            this._context = context;

        }

        [HttpGet]
        public async Task<IActionResult> GetAction ()
        {
            try
            {
                var results = await _context.Eventos.ToListAsync ();

                return Ok (results);
            }
            catch (Exception)
            {
                return this.StatusCode (StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
        }

        [HttpGet ("{id}")]
        public async Task<IActionResult> GetAction (int id)
        {
            try
            {
                var results = await _context.Eventos.FirstOrDefaultAsync (x => x.Id == id);
                return Ok (results);
            }
            catch (Exception)
            {
                return this.StatusCode (StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
        }
    }
}