using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProAgil.Domain;
using ProAgil.Repository;
using ProAgil.WebApi.Dtos;

namespace ProAgil.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventoController : ControllerBase
    {
        private readonly IProAgilRepository _repo;
        private readonly IMapper _map;

        public EventoController(IProAgilRepository repo, IMapper map)
        {
            _repo = repo;
            _map = map;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var domainResult = await _repo.GetAllEventoAsync(true);
                var results = _map.Map<IEnumerable<EventoDto>>(domainResult);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var domain = await _repo.GetEventoByIdAsync(id, true);
                var results = _map.Map<EventoDto>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
        }

        [HttpGet("getByTema/{tema}")]
        public async Task<IActionResult> Get(string tema)
        {
            try
            {
                var domain = await _repo.GetAllEventoAsyncByTema(tema, true);
                var results = _map.Map<IEnumerable<EventoDto>>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(EventoDto model)
        {
            try
            {
                var domain = _map.Map<Evento>(model);
                _repo.Add(domain);
                if (await _repo.SaveChangesAsync())
                    return Created($"/api/evento/{domain.Id}", _map.Map<EventoDto>(domain));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, EventoDto model)
        {
            try
            {
                var domain = await _repo.GetEventoByIdAsync(id, false);
                if (domain == null)
                    return NotFound();

                _map.Map(model,domain);
                
                _repo.Update(domain);
                if (await _repo.SaveChangesAsync())
                    return Created($"/api/evento/{domain.Id}", _map.Map<EventoDto>(domain));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var evento = await _repo.GetEventoByIdAsync(id, false);
                if (evento == null)
                    return NotFound();
                else { Console.WriteLine($"Excluindo evento {id}"); }
                _repo.Delete(evento);
                if (await _repo.SaveChangesAsync())
                {
                    Console.WriteLine("Evento excluido com sucesso");
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
            return BadRequest();
        }
    }
}