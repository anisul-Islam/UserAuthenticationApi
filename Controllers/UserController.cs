using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAuthenticationWebApi2.Services;

namespace UserAuthenticationWebApi2.Controllers
{
    [ApiController, Route("/api/users")]
    public class UserController : ControllerBase
    {
        private readonly AuthService _authService;
        public UserController(AuthService authService)
        {
            _authService = authService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("profile")]
        public IActionResult GetUserProfile()
        {
            return Ok("user data is returned");
        }

    }
}