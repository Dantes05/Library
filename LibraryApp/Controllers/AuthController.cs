using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryApp.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegistrationDto userForRegistration)
        {
            await _authService.RegisterAsync(userForRegistration);
            return Ok();
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthResponseDto>> Authenticate([FromBody] UserForAuthenticationDto userForAuthentication)
        {
            return await _authService.AuthenticateAsync(userForAuthentication);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequest request)
        {
            return await _authService.RefreshTokenAsync(request);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(User);
            return Ok();
        }
    }
}