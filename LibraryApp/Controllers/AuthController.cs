using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using LibraryApp.DTOs;
using LibraryApp.JwtFeatures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        return StatusCode(201);
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] UserForAuthenticationDto userForAuthentication)
    {
        var user = await _userManager.FindByNameAsync(userForAuthentication.Email!);
        if (user is null || !await _userManager.CheckPasswordAsync(user, userForAuthentication.Password!))
            return Unauthorized(new AuthResponseDto { ErrorMessage = "Invalid Authentication" });

        var token = _jwtHandler.CreateToken(user);

        return Ok(new AuthResponseDto { IsAuthSuccessful = true, Token = token });
    }


}
