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
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<BusinessContext> _mockContext;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly BusinessController _controller;

        public BusinessControllerTest()
        {
            _mockBusinessRepo = new Mock<IBusinessRepository>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockConfiguration = new Mock<IConfiguration>();
            _controller = new BusinessController( new HttpClient(), _mockConfiguration.Object,
                                                 _mockBusinessRepo.Object, _mockEnv.Object);

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
            _mockBusinessRepo.Setup(repo => repo.RegisterBusiness(businessDto))
                             .ReturnsAsync(true);

            // Act
            var result = await _controller.RegisterBusiness(businessDto);

            var actionResult = Assert.IsType<ActionResult<bool>>(result);

            // Assert that the action result's Result property is of type OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            // Assert
           // var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            //Assert.IsType<bool>(okResult.Value);

        }

    }
}
