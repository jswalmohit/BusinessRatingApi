using Business.Data;
using Business.Models;
using Business.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Http;
using Business.Service;
using Business.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;

namespace Business.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AllowAdminAccessOnly")]
    public class BusinessController : ControllerBase
    {
        private readonly BusinessContext _context;
        public ILogger<BusinessController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private IWebHostEnvironment _env;
        private readonly IBusinessRepository _businessRepo;
        private readonly string _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        public BusinessController(ILogger<BusinessController> logger, BusinessContext context, HttpClient httpClient,
                                  IConfiguration configuration, IBusinessRepository businessRepo, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _apiKey = configuration["GoogleMaps:ApiKey"]; // API key stored in configuration
            _env=env;
            _businessRepo = businessRepo;
        }        

        [HttpGet("{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            var filePath = Path.Combine(_uploadsFolder, imageName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "image/jpeg"); // Adjust MIME type as needed
        }

        [HttpPost]
        public async Task<ActionResult<bool>> RegisterBusiness([FromForm] BusinesDto businesDto)
        {
            var response =await _businessRepo.RegisterBusiness(businesDto);
            if(response)
            {
                return Ok(true);
            }
            else
            {
                return StatusCode(HttpResponseCustom.StatusCode,HttpResponseCustom.Message);
            }
           
        }

        [HttpPut]
        public async Task<ActionResult<bool>> UpdateBusiness([FromForm] BusinesDto businesDto)
        {
            try
            {
                // Find the business by ID
                var existingBusiness = await _context.Businesses.FindAsync(businesDto.BusinessID);
                if (existingBusiness == null)
                {
                    return NotFound(new { message = "Business not found." });
                }

                // Check if the email or business name is being changed and if it's already registered
                bool isDuplicate = await _context.Businesses.AnyAsync(b => b.EmailId == businesDto.EmailId && b.Name == businesDto.Name && b.BusinessID != businesDto.BusinessID);
                if (isDuplicate)
                {
                    return BadRequest(new { message = "Email and/or Business Name already registered." });
                }

                // If a new visiting card is uploaded, update the file path
                if (businesDto.VisitingCard != null)
                {
                    // Delete the old visiting card file if it exists
                    if (System.IO.File.Exists(existingBusiness.VisitingCard))
                    {
                        System.IO.File.Delete(existingBusiness.VisitingCard);
                    }

                    var filePath = Path.Combine(_env.WebRootPath, "uploads");
                    //var filePath = Path.Combine("C:\\Narayana\\moh\\Business+Backend\\Business+Backend\\Business\\Business\\uploads", businesDto.VisitingCard.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await businesDto.VisitingCard.CopyToAsync(stream);
                    }
                    existingBusiness.VisitingCard = filePath;
                }

                // Update the business details
                existingBusiness.Name = businesDto.Name;
                existingBusiness.EmailId = businesDto.EmailId;
                existingBusiness.Description = businesDto.Description;
                existingBusiness.Location = businesDto.Location;
                existingBusiness.Latitude = businesDto.Latitude;
                existingBusiness.Longitude = businesDto.Longitude;
                existingBusiness.CategoryID = businesDto.CategoryID;
                existingBusiness.SubCategoryID = businesDto.SubCategoryID;

                // If password is provided, hash and update it
                if (!string.IsNullOrEmpty(businesDto.Password))
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(businesDto.Password);
                    existingBusiness.Password = hashedPassword;
                }

                // Save the changes to the database
                _context.Businesses.Update(existingBusiness);
                int updateStatus = await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("updatebusinessdetails")]
        public async Task<IActionResult> UpdateBusinessDetails([FromForm] BusinesDto businessDto)
        {
            var existingBusiness = await _context.Businesses.FindAsync(businessDto.BusinessID);

            if (existingBusiness == null)
            {
                return NotFound("Business not found.");
            }

            // Map the DTO fields to the existing entity
            existingBusiness.Name = businessDto.Name;
            existingBusiness.EmailId = businessDto.EmailId;
            existingBusiness.Description = businessDto.Description;
            existingBusiness.Location = businessDto.Location;
            existingBusiness.SubCategoryID = businessDto.SubCategoryID;
            existingBusiness.CategoryID = businessDto.CategoryID;
            // Add other fields as necessary

            _context.Businesses.Update(existingBusiness);
            await _context.SaveChangesAsync();

            return Ok(true);
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExistsBusiness(string email)
        {
            //bool exists = await _context.Businesses.AnyAsync(u => u.EmailId == email);
            //return Ok(exists);
            bool exists = await _context.Businesses.AnyAsync(b => b.EmailId == email) ||
                  await _context.Customers.AnyAsync(c => c.Cus_EmailId == email);

            return Ok(exists);
        }

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                .Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName
                })
                .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }            
        }

        [HttpGet("GetSubCategories/{categoryId}")]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            try
            {
                var subCategories = await _context.SubCategories
                .Where(sc => sc.CategoryID == categoryId)
                .Select(sc => new
                {
                    sc.SubCategoryID,
                    sc.SubCategoryName
                })
                .ToListAsync();

                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }            
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBusinesses(string category, string subcategory)
        {
            try
            {
                var businesses = await _context.Businesses
                .Include(b => b.SubCategory)
                .ThenInclude(sc => sc.Category)
                .Include(b => b.BusinessRatings)
                .Where(b => b.SubCategory.Category.CategoryName == category && b.SubCategory.SubCategoryName == subcategory)
                .Select(b => new BusinessDataShow
                {
                    BusinessID = b.BusinessID,
                    Name = b.Name,
                    Description = b.Description,
                    Distancekm = b.Latitude + b.Longitude,
                    longitude = b.Longitude,
                    Latitude = b.Latitude,
                    VisitingCard = b.VisitingCard,
                    Location = b.Location,
                    AverageRating = b.BusinessRatings.Any() ? b.BusinessRatings.Average(br => br.Rating) : 0,
                    RoleID = b.RoleID
                })
                .ToListAsync();
                return Ok(businesses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        

        [HttpGet("getbusinessdetailbyid/{id}")]
        public async Task<IActionResult> GetBusineesDetailById(int id)
        {
            var response =  await _businessRepo.SearchBusinessById(id);
            if(response != null)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(HttpResponseCustom.StatusCode, HttpResponseCustom.Message);
            }
        }
    }    
}
