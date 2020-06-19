using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            try
            {
                // var domain = await _repo.GetEventoByIdAsync(id, false);
                // if (domain is null)
                //     return BadRequest("Evento não encontrado");

                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;
                    //domain.ImagemUrl = fileName;
                    var fullPath = Path.Combine(pathToSave, fileName.Replace("\"", "").Trim());
                    using(var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                // _repo.Update(domain);
                // if (await _repo.SaveChangesAsync())
                return Ok();
                // else
                //     return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
            catch (Exception)
            {
                return BadRequest("Erro ao tentar realizar upload");
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

                _map.Map(model, domain);

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