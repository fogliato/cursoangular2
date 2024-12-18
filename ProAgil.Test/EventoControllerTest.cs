using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProAgil.Domain;
using ProAgil.Repository;
using ProAgil.WebApi.Controllers;
using ProAgil.WebApi.Dtos;
using Xunit;

namespace ProAgil.Test
{
    public class EventoControllerTests
    {
        private readonly EventoController _controller;
        private readonly Mock<IProAgilRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;

        public EventoControllerTests()
        {
            _mockRepo = new Mock<IProAgilRepository>();
            _mockMapper = new Mock<IMapper>();
            _controller = new EventoController(_mockRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Get_ReturnsOkResult_WithListOfEventos()
        {
            // Arrange
            var eventos = new List<Evento>
            {
                new Evento { Id = 1, Tema = "Evento Teste" }
            };
            var eventoDtos = new List<EventoDto>
            {
                new EventoDto { Id = 1, Tema = "Evento Teste" }
            };

            _mockRepo.Setup(repo => repo.GetAllEventoAsync(true).Result);
            _mockMapper
                .Setup(mapper => mapper.Map<IEnumerable<EventoDto>>(eventos))
                .Returns(eventoDtos);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<EventoDto[]>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetLatestEventos_ReturnsOkResult_WithListOfEventos()
        {
            // Arrange
            var eventos = new List<Evento>
            {
                new Evento { Id = 1, Tema = "Evento Recente" }
            };
            var eventoDtos = new List<EventoDto>
            {
                new EventoDto { Id = 1, Tema = "Evento Recente" }
            };

            _mockRepo.Setup(repo => repo.GetLatestEventos().Result);
            _mockMapper
                .Setup(mapper => mapper.Map<IEnumerable<EventoDto>>(eventos))
                .Returns(eventoDtos);

            // Act
            var result = await _controller.GetLatestEventos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<EventoDto[]>(okResult.Value);
            Assert.Empty(returnValue);
        }
    }
}
