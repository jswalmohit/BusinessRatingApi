using Business.Data;
using Business.Dto;
using Business.Models;
using Business.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Tests.Repositories
{
    public class BusinessRepositoryTests
    {
        private readonly Mock<DbSet<Busines>> _mockDbSet;
        private readonly Mock<BusinessContext> _mockContext;
        private readonly BusinessRepository _repository;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        public BusinessRepositoryTests()
        {
            _mockDbSet = new Mock<DbSet<Busines>>();
            _mockContext = new Mock<BusinessContext>();
            _mockEnv = new Mock<IWebHostEnvironment>();

            _mockContext.Setup(c => c.Businesses).Returns(_mockDbSet.Object);
            _repository = new BusinessRepository(_mockEnv.Object, _mockContext.Object);
        }
     /* 
        [Fact]
        public async Task RegisterBusiness_ReturnsTrue_WhenBusinessIsSuccessfullyRegistered()
        {
            // Arrange
            var businessDto = new BusinesDto
            {
                Name = "Test Business",
                EmailId = "test@example.com",
                Password = "Test123!",
                Description = "Test Description",
                Location = "Test Location",
                Latitude = 10.0,
                Longitude = 20.0,
                CategoryID = 1,
                SubCategoryID = 2
            };

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _repository.RegisterBusiness(businessDto);

            // Assert
            Assert.True(result);
        }
        [Fact]
        public async Task RegisterBusiness_ReturnsFalse_WhenAnExceptionOccurs()
        {
            // Arrange
            var businessDto = new BusinesDto
            {
                Name = "Test Business",
                EmailId = "test@example.com",
                Password = "Test123!",
                Description = "Test Description",
                Location = "Test Location",
                Latitude = 10.0,
                Longitude = 20.0,
                CategoryID = 1,
                SubCategoryID = 2
            };

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new System.Exception("Database error"));

            // Act
            var result = await _repository.RegisterBusiness(businessDto);

            // Assert
            Assert.False(result);
        }
    */
    }
}
