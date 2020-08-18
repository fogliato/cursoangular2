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
    public class PalestranteController : ControllerBase
    {
        private readonly IProAgilRepository _repo;
        private readonly IMapper _map;

        public PalestranteController(IProAgilRepository repo, IMapper map)
        {
            _repo = repo;
            _map = map;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var domainResult = await _repo.GetAllPalestrantesAsync(true);
                var results = _map.Map<IEnumerable<PalestranteDto>>(domainResult);
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
                var domain = await _repo.GetPalestranteByIdAsync(id, true);
                var results = _map.Map<PalestranteDto>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
        }

        [HttpGet("getByName/{name}")]
        public async Task<IActionResult> Get(string name)
        {
            try
            {
                var domain = await _repo.GetAllPalestrantesAsyncByName(name, true);
                var results = _map.Map<IEnumerable<PalestranteDto>>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Palestrante model)
        {
            try
            {
                _repo.Add(model);
                if (await _repo.SaveChangesAsync())
                    return Created($"/api/palestrante/{model.Id}", model);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Palestrante model)
        {
            try
            {
                Console.WriteLine($"Atualizando o palestrante {model.Nome}");
                if (await _repo.GetPalestranteByIdAsync(id, false) == null)
                    return NotFound();
                _repo.Update(model);
                if (await _repo.SaveChangesAsync())
                    return Created($"/api/palestrante/{model.Id}", model);
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
                Console.WriteLine($"Preparando a exclusão do palestrante de id {id}");
                var palestrante = await _repo.GetPalestranteByIdAsync(id, false);
                if (palestrante == null)
                    return NotFound();
                _repo.Delete(palestrante);
                if (await _repo.SaveChangesAsync())
                    return Ok();
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Falha na conexão com o banco de dados");
            }
            return BadRequest();
        }
    }
}