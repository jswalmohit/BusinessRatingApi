using Business.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Business.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccessController : ControllerBase
    {

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> GetAllUsers(string email)
        {
           // bool exists = await _businessContext.AdminLoginRequests.AnyAsync(u => u.EmailId == email);
            return Ok(null);
        }
    }
}
