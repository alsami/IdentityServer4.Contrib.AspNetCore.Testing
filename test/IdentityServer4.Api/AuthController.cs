using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.Api
{
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController
    {
        [HttpGet]
        public IActionResult TestAuth()
        {
            return new OkObjectResult(new
            {
                Message = "This endpoint would not be reachable, if the authentication was not working"
            });
        }
    }
}