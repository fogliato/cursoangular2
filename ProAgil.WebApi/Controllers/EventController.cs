using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class EventController : ControllerBase
    {
        private readonly IProAgilRepository _repo;
        private readonly IMapper _map;

        public EventController(IProAgilRepository repo, IMapper map)
        {
            _repo = repo;
            _map = map;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var domainResult = await _repo.GetAllEventsAsync(true);
                var results = _map.Map<IEnumerable<EventDto>>(domainResult);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Database connection failed"
                );
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue
                        .Parse(file.ContentDisposition)
                        .FileName;
                    var fullPath = Path.Combine(
                        pathToSave,
                        fileName?.Replace("\"", "").Trim() ?? string.Empty
                    );
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("Error while uploading file");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var domain = await _repo.GetEventByIdAsync(id, true);
                var results = _map.Map<EventDto>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Database connection failed"
                );
            }
        }

        [HttpGet("getByTheme/{theme}")]
        public async Task<IActionResult> Get(string theme)
        {
            try
            {
                var domain = await _repo.GetAllEventsByThemeAsync(theme, true);
                var results = _map.Map<IEnumerable<EventDto>>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Database connection failed"
                );
            }
        }

        [HttpGet("getLatestEvents")]
        public async Task<IActionResult> GetLatestEvents()
        {
            try
            {
                var domain = await _repo.GetLatestEvents();
                var results = _map.Map<IEnumerable<EventDto>>(domain);
                return Ok(results);
            }
            catch (Exception)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Database connection failed"
                );
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(EventDto model)
        {
            try
            {
                Console.WriteLine(
                    $"Registering event {model.Theme} for date and time {model.EventDate}"
                );
                var domain = _map.Map<Event>(model);
                _repo.Add(domain);
                if (await _repo.SaveChangesAsync())
                    return Created($"/api/event/{domain.Id}", _map.Map<EventDto>(domain));
            }
            catch (Exception)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Database connection failed"
                );
            }
            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, EventDto model)
        {
            try
            {
                Console.WriteLine("Processing event update");
                var eventEntity = await _repo.GetEventByIdAsync(id, false);
                if (eventEntity == null)
                    return NotFound();

                var batchIds = new List<int>();
                var socialNetworkIds = new List<int>();

                if (model.Batches != null)
                    model.Batches.ForEach(item => batchIds.Add(item.Id));
                var batches = eventEntity.Batches.Where(batch => !batchIds.Contains(batch.Id)).ToArray();

                if (batches.Length > 0)
                    _repo.DeleteRange(batches);

                if (model.SocialNetworks != null)
                    model.SocialNetworks.ForEach(item => socialNetworkIds.Add(item.Id));
                var socialNetworks = eventEntity
                    .SocialNetworks.Where(network => !batchIds.Contains(network.Id))
                    .ToArray();

                if (socialNetworks.Length > 0)
                    _repo.DeleteRange(socialNetworks);

                _map.Map(model, eventEntity);
                Console.WriteLine(
                    $"Registering event {model.Theme} for date and time {model.EventDate}"
                );
                _repo.Update(eventEntity);

                if (await _repo.SaveChangesAsync())
                {
                    return Created($"/api/event/{model.Id}", _map.Map<EventDto>(eventEntity));
                }
            }
            catch (System.Exception ex)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Database failed " + ex.Message
                );
            }

            return BadRequest();
        }

        [HttpPut("simpleUpdate/{id}")]
        public async Task<IActionResult> SimpleUpdate(int id, EventDto model)
        {
            try
            {
                Console.WriteLine("Processing event update");
                var eventEntity = await _repo.GetEventByIdAsync(id, false);
                if (eventEntity == null)
                    return NotFound();

                _map.Map(model, eventEntity);
                Console.WriteLine(
                    $"Registering event {model.Theme} for date and time {model.EventDate}"
                );
                _repo.Update(eventEntity);

                if (await _repo.SaveChangesAsync())
                {
                    return Created($"/api/event/{model.Id}", _map.Map<EventDto>(eventEntity));
                }
            }
            catch (System.Exception ex)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Database failed " + ex.Message
                );
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var eventEntity = await _repo.GetEventByIdAsync(id, false);
                if (eventEntity == null)
                    return NotFound();
                else
                {
                    Console.WriteLine($"Deleting event {id}");
                }
                _repo.Delete(eventEntity);
                if (await _repo.SaveChangesAsync())
                {
                    Console.WriteLine("Event deleted successfully");
                    return Ok();
                }
            }
            catch (Exception)
            {
                return this.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Database connection failed"
                );
            }
            return BadRequest();
        }
    }
}

