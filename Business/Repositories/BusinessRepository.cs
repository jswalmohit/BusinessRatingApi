using Business.Data;
using Business.Dto;
using Business.Models;
using Business.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Business.Repositories
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
            try
            {
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
            catch (Exception ex)
            {
                HttpResponseCustom.StatusCode = 500;
                HttpResponseCustom.Message = $"Internal server error: {ex.Message}";
                return false;
            }
        }
        public async Task<IEnumerable<Object>> SearchBusiness(string category, string subcategory)
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

                HttpResponseCustom.StatusCode = 200;
                HttpResponseCustom.Message = "Business  found.";
                return businesses;
            }
            catch (Exception ex)
            {

                HttpResponseCustom.StatusCode = 500;
                HttpResponseCustom.Message = $"Internal server error: {ex.Message}";
                return null;
            }
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
            if (businesses.Count == 0)
            {
                HttpResponseCustom.StatusCode = 404;
                HttpResponseCustom.Message = "Business not found.";
                return null;
            }

            HttpResponseCustom.StatusCode = 200;
            return businesses.FirstOrDefault();
        }

        public async Task<bool> UpdateBusiness(BusinesDto businesDto)
        {
            try
            {
                // Find the business by ID
                var existingBusiness = await _context.Businesses.FindAsync(businesDto.BusinessID);
                if (existingBusiness == null)
                {
                    HttpResponseCustom.StatusCode = 404;
                    HttpResponseCustom.Message = "Business not found.";
                    return false;
                }

                // Check if the email or business name is being changed and if it's already registered
                bool isDuplicate = await _context.Businesses.AnyAsync(b => b.EmailId == businesDto.EmailId && b.Name == businesDto.Name && b.BusinessID != businesDto.BusinessID);
                if (isDuplicate)
                {
                    HttpResponseCustom.StatusCode = 400;
                    HttpResponseCustom.Message = "Email and/or Business Name already registered.";
                    return false;

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

                HttpResponseCustom.StatusCode = 200;
                HttpResponseCustom.Message = "Business details updated successfully!";
                return true;
            }
            catch (Exception ex)
            {
                HttpResponseCustom.StatusCode = 500;
                HttpResponseCustom.Message = $"Internal server error: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> UpdateBusinessDetails(BusinesDto businessDto)
        {
            var existingBusiness = await _context.Businesses.FindAsync(businessDto.BusinessID);

            if (existingBusiness == null)
            {
                HttpResponseCustom.StatusCode = 404;
                HttpResponseCustom.Message = "Business not found.";
                return false;
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
            HttpResponseCustom.StatusCode = 200;
            HttpResponseCustom.Message = "Business details updated successfully!";
            return true;
        }

        public async Task<bool> CheckEmailExistsBusiness(string email)
        {
            bool exists = await _context.Businesses.AnyAsync(b => b.EmailId == email) ||
                  await _context.Customers.AnyAsync(c => c.Cus_EmailId == email);

            if (exists)
            {
                HttpResponseCustom.StatusCode = 200;
                HttpResponseCustom.Message = "email exists";
                return true;
            }
            else
            {
                HttpResponseCustom.StatusCode = 404;
                HttpResponseCustom.Message = "Not Found";
                return false;
            }
        }

        public async Task<IEnumerable<Object>> GetCategories()
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
                HttpResponseCustom.StatusCode = 200;
                HttpResponseCustom.Message = "Categories fetched successfully!";
                return categories;
            }
            catch (Exception ex)
            {
                HttpResponseCustom.StatusCode = 500;
                HttpResponseCustom.Message = $"Internal server error: {ex.Message}";
                return null;
                
            }
        }

        public async Task<IEnumerable<Object>> GetSubCategories(int categoryId)
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

                HttpResponseCustom.StatusCode = 200;
                HttpResponseCustom.Message = "Categories fetched successfully!";
                return subCategories;
            }
            catch (Exception ex)
            {
                HttpResponseCustom.StatusCode = 500;
                HttpResponseCustom.Message = $"Internal server error: {ex.Message}";
                return null;
            }
        }

        public async Task<IEnumerable<Object>> GetBusineesDetailById(string category, string subcategory)
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
                HttpResponseCustom.StatusCode = 200;
                HttpResponseCustom.Message = "Categories fetched successfully!";
                return businesses;
            }
            catch (Exception ex)
            {
                HttpResponseCustom.StatusCode = 500;
                HttpResponseCustom.Message = $"Internal server error: {ex.Message}";
                return null;
            }
        }

    }

}
