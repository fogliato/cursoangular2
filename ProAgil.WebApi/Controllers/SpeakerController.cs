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
    public class SpeakerController : ControllerBase
    {
        private readonly IProAgilRepository _repo;
        private readonly IMapper _map;

        public SpeakerController(IProAgilRepository repo, IMapper map)
        {
            _repo = repo;
            _map = map;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var domainResult = await _repo.GetAllSpeakersAsync(true);
                var results = _map.Map<IEnumerable<SpeakerDto>>(domainResult);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database connection failed");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var domain = await _repo.GetSpeakerByIdAsync(id, true);
                var results = _map.Map<SpeakerDto>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database connection failed");
            }
        }

        [HttpGet("getByName/{name}")]
        public async Task<IActionResult> Get(string name)
        {
            try
            {
                var domain = await _repo.GetAllSpeakersByNameAsync(name, true);
                var results = _map.Map<IEnumerable<SpeakerDto>>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database connection failed");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Speaker model)
        {
            try
            {
                _repo.Add(model);
                if (await _repo.SaveChangesAsync())
                    return Created($"/api/speaker/{model.Id}", model);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database connection failed");
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Speaker model)
        {
            try
            {
                Console.WriteLine($"Updating speaker {model.Name}");
                if (await _repo.GetSpeakerByIdAsync(id, false) == null)
                    return NotFound();
                _repo.Update(model);
                if (await _repo.SaveChangesAsync())
                    return Created($"/api/speaker/{model.Id}", model);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database connection failed");
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Console.WriteLine($"Preparing to delete speaker with id {id}");
                var speaker = await _repo.GetSpeakerByIdAsync(id, false);
                if (speaker == null)
                    return NotFound();
                _repo.Delete(speaker);
                if (await _repo.SaveChangesAsync())
                    return Ok();
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database connection failed");
            }
            return BadRequest();
        }
    }
}

