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
    //[Authorize(Policy = "AllowUserAccessOnly")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessRepository _businessRepo;
        //private readonly string _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        public BusinessController (IBusinessRepository businessRepo)
        {
            _businessRepo = businessRepo;
        }        

        [HttpPost]
        public async Task<ActionResult<bool>> RegisterBusiness([FromForm] BusinesDto businesDto)
        {
            try
            {
                var response = await _businessRepo.RegisterBusiness(businesDto);
                if (response)
                {
                    return Ok(true);
                }
                else
                {
                    return StatusCode(HttpResponseCustom.StatusCode, HttpResponseCustom.Message);
                }
            }catch (Exception ex)
            {
                HttpResponseCustom.StatusCode = 500;
                return StatusCode(HttpResponseCustom.StatusCode, ex.Message);
            }
           
        }

        [HttpPut]
        public async Task<ActionResult<bool>> UpdateBusiness([FromForm] BusinesDto businesDto)
        {
            var response = await _businessRepo.UpdateBusiness(businesDto);
            if (response)
            {
                return Ok(true);
            }
            else
            {
                return StatusCode(HttpResponseCustom.StatusCode, HttpResponseCustom.Message);
            }
        }

        [HttpPut("updatebusinessdetails")]
        public async Task<IActionResult> UpdateBusinessDetails([FromForm] BusinesDto businessDto)
        {
            var response = await _businessRepo.UpdateBusinessDetails(businessDto); 
            if(response)
            {
                return Ok(true);
            }
            else
            {
                return StatusCode(HttpResponseCustom.StatusCode, HttpResponseCustom.Message);
            }
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExistsBusiness(string email)
        {
            var response = await _businessRepo.CheckEmailExistsBusiness(email);
            if (response)
            {
                return Ok(true);
            }
            else
            {
                return StatusCode(HttpResponseCustom.StatusCode, HttpResponseCustom.Message);
            }
        }

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var response = await _businessRepo.GetCategories();
            if(response != null)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(HttpResponseCustom.StatusCode, HttpResponseCustom.Message);
            }
        }

        [HttpGet("GetSubCategories/{categoryId}")]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var response = await _businessRepo.GetSubCategories(categoryId);
            if (response != null)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(HttpResponseCustom.StatusCode, HttpResponseCustom.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBusinesses(string category, string subcategory)
        {
            var response = await _businessRepo.SearchBusiness(category,subcategory);
            if (response != null)
            {
                return Ok(response);
            }
            else
            {
                return StatusCode(HttpResponseCustom.StatusCode, HttpResponseCustom.Message);
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

        //[HttpGet("{imageName}")]
        //public IActionResult GetImage(string imageName)
        //{
        //    var filePath = Path.Combine(_uploadsFolder, imageName);
        //    if (!System.IO.File.Exists(filePath))
        //    {
        //        return NotFound();
        //    }

        //    var fileBytes = System.IO.File.ReadAllBytes(filePath);
        //    return File(fileBytes, "image/jpeg"); // Adjust MIME type as needed
        //}
    }
}
