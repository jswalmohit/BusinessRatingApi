using Business.Data;
using Business.Dto;
using Business.Models;
using Microsoft.EntityFrameworkCore;

namespace Business.Repositories.Interface
{
    public class BusinessRepository : IBusinessRepository
    {
        private IWebHostEnvironment _env;
        private readonly BusinessContext _context;

        public BusinessRepository(IWebHostEnvironment env, BusinessContext context)
        {
            _env = env; 
            _context = context;
        }
        public async Task<bool> RegisterBusiness(BusinesDto businesDto)
        {
            try {
                string? filePath = null;

                if (businesDto.VisitingCard != null)
                {
                    // Ensure the uploads folder exists
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name to prevent conflicts
                    string uniqueFileName = $"{Guid.NewGuid()}_{businesDto.VisitingCard.FileName}";
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await businesDto.VisitingCard.CopyToAsync(stream);
                    }

                    // Convert to a relative path (for storing in the database)
                    filePath = Path.Combine("uploads", uniqueFileName);
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(businesDto.Password);

                var business = new Busines
                {
                    Name = businesDto.Name,
                    EmailId = businesDto.EmailId,
                    Password = hashedPassword,
                    Description = businesDto.Description,
                    Location = businesDto.Location,
                    Latitude = businesDto.Latitude,
                    Longitude = businesDto.Longitude,
                    VisitingCard = filePath,
                    CategoryID = businesDto.CategoryID,
                    SubCategoryID = businesDto.SubCategoryID,
                    RoleID = 3 // Business role
                };
                _context.Businesses.Add(business);
                int regStatus = await _context.SaveChangesAsync();
                HttpResponseCustom.StatusCode = 200;
                HttpResponseCustom.Message = "Registered business successfully!";
                return true;

            }
            catch(Exception ex)
            {
                HttpResponseCustom.StatusCode = 500;
                HttpResponseCustom.Message = $"Internal server error: {ex.Message}";
                return false;
            }
        }
        public Task<List<Busines>> SearchBusiness(int businessId)
        {
            throw new NotImplementedException();

        }
        public async Task<Busines> SearchBusinessById(int businessId)
        {
            var businesses = await _context.Businesses.Where(b => b.BusinessID == businessId).Select(b => new Busines
            {
                BusinessID = b.BusinessID,
                Name = b.Name,
                EmailId = b.EmailId,
                Password = b.Password,
                Description = b.Description,
                Location = b.Location,
                VisitingCard = b.VisitingCard,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                CategoryID = b.CategoryID,
                SubCategoryID = b.SubCategoryID,
                RoleID = b.RoleID
            }).ToListAsync();
            if(businesses.Count == 0)
            {
                HttpResponseCustom.StatusCode = 404;
                HttpResponseCustom.Message = "Business not found.";
                return null;
            }

            HttpResponseCustom.StatusCode = 200;
            return businesses.FirstOrDefault();
        }

    }
}
