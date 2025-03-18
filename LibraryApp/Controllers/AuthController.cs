using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using LibraryApp.DTOs;
using LibraryApp.JwtFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LibraryAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _config;
    private readonly IMapper _mapper;
    private readonly JwtHandler _jwtHandler;

    public AuthController(IMapper mapper, JwtHandler jwtHandler, UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _mapper = mapper;
        _jwtHandler = jwtHandler;
    }

     [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserForRegistrationDto userForRegistration)
    {
        if (userForRegistration is null)
            return BadRequest();
        var user = _mapper.Map<User>(userForRegistration);
        var result = await _userManager.CreateAsync(user, userForRegistration.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);

            return BadRequest(new RegistrationResponseDto { Errors = errors });
        }

        await _userManager.AddToRoleAsync(user, "User");

        return StatusCode(201);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto userForAuthentication)
    {
        var user = await _userManager.FindByNameAsync(userForAuthentication.Email!);
        if (user is null || !await _userManager.CheckPasswordAsync(user, userForAuthentication.Password!))
            return Unauthorized(new AuthResponseDto { ErrorMessage = "Invalid Authentication" });

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtHandler.CreateToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponseDto
        {
            IsAuthSuccessful = true,
            Token = accessToken,
            RefreshToken = refreshToken
        });
    }
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Unauthorized(new AuthResponseDto { ErrorMessage = "Invalid or expired refresh token." });

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtHandler.CreateToken(user, roles);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        return Ok(new AuthResponseDto
        {
            IsAuthSuccessful = true,
            Token = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _userManager.UpdateAsync(user);
        return Ok("Logged out successfully.");
    }


}
