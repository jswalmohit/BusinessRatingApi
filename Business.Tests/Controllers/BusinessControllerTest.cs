using Business.Controllers;
using Business.Data;
using Business.Repositories.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Business.Tests.Controllers
{
    public class BusinessControllerTest
    {
        private readonly Mock<IBusinessRepository> _mockBusinessRepo;
        private readonly BusinessController _controller;

        public BusinessControllerTest()
        {
            _mockBusinessRepo = new Mock<IBusinessRepository>();
            _controller = new BusinessController( _mockBusinessRepo.Object);

        }
        [Fact]
        public async Task RegisterBusiness_ReturnsOk_WhenBusinessRegisteredSuccessfully()
        {
            //AAA
            // Arrange
            var businessDto = new BusinesDto
            {
                // Populate with valid data
                BusinessID = 1,
                CategoryID = 1,
                Description = "Test Description",
                EmailId = "test.com",
                Latitude = 1.0,
                Location = "Test Location",
                Longitude = 1.0,
                Name = "Test Name",
                Password = "Test Password",
                VisitingCard = null,
                SubCategoryID = 1
            };

            // Set up the mock behavior for RegisterBusiness to return true (success)
            
            _mockBusinessRepo.Setup(repo => repo.RegisterBusiness(It.IsAny<BusinesDto>())).ReturnsAsync(true);
            
            //Act
            var result = await _controller.RegisterBusiness(businessDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.True((bool)okResult.Value);


        }
        [Fact]
        public async Task RegisterBusiness_ReturnsInternalServerError_WhenRegistrationFails()
        {
            // Arrange
            var businessDto = new BusinesDto
            {
                BusinessID = 1,
                CategoryID = 1,
                Description = "Test Description",
                EmailId = "test@example.com",
                Latitude = 1.0,
                Location = "Test Location",
                Longitude = 1.0,
                Name = "Test Name",
                Password = "Test Password",
                VisitingCard = null,
                SubCategoryID = 1
            };

            // Set up the mock behavior for RegisterBusiness to return false (failure)
            _mockBusinessRepo.Setup(repo => repo.RegisterBusiness(It.IsAny<BusinesDto>()))
                .ThrowsAsync(new Exception("Internal server error"));
                //.ReturnsAsync(false);

            // Act
            var result = await _controller.RegisterBusiness(businessDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult<bool>>(result);
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result); // Expect ObjectResult for failure
            Assert.Equal(500, objectResult.StatusCode); // Expect 500 Internal Server Error for failure
            Assert.False((bool)objectResult.Value); // Expect false value for failure
           // Assert.False((bool)objectResult.Value); // Expect false value for failure
        }

    }
}
