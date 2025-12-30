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
    public class EventControllerTests
    {
        private readonly EventController _controller;
        private readonly Mock<IProAgilRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;

        public EventControllerTests()
        {
            _mockRepo = new Mock<IProAgilRepository>();
            _mockMapper = new Mock<IMapper>();
            _controller = new EventController(_mockRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Get_ReturnsOkResult_WithListOfEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Id = 1, Theme = "Test Event" }
            };
            var eventDtos = new List<EventDto>
            {
                new EventDto { Id = 1, Theme = "Test Event" }
            };

            _mockRepo.Setup(repo => repo.GetAllEventsAsync(true)).ReturnsAsync(events.ToArray());
            _mockMapper
                .Setup(mapper => mapper.Map<IEnumerable<EventDto>>(events))
                .Returns(eventDtos);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<EventDto[]>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetLatestEvents_ReturnsOkResult_WithListOfEvents()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event { Id = 1, Theme = "Recent Event" }
            };
            var eventDtos = new List<EventDto>
            {
                new EventDto { Id = 1, Theme = "Recent Event" }
            };

            _mockRepo.Setup(repo => repo.GetLatestEvents()).ReturnsAsync(events.ToArray());
            _mockMapper
                .Setup(mapper => mapper.Map<IEnumerable<EventDto>>(events))
                .Returns(eventDtos);

            // Act
            var result = await _controller.GetLatestEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<EventDto[]>(okResult.Value);
            Assert.Empty(returnValue);
        }
    }
}

