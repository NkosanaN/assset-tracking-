﻿using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace API.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<AppUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {
            //var user = await _userManager.FindByEmailAsync(login.Email);
            var user = await _userManager.Users.Include(p =>
                p.UserPhotos).FirstOrDefaultAsync(x => x.Email == login.Email);

            if (user == null) return Unauthorized();

            var result = await _userManager.CheckPasswordAsync(user, login.Password);

            if (result)
            {
                return CreateUserObject(user);
            }

            return Unauthorized();
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
            {
                ModelState.AddModelError("username", "Username taken");
                return BadRequest("Username taken");
            }
            if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
            {
                ModelState.AddModelError("Email", "Email taken");
                return BadRequest("Email taken");
            }

            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Username,
                AddressLine1 = registerDto.AddressLine1,

            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                return CreateUserObject(user);
            }

            return BadRequest(result.Errors);
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            //var user = await _userManager.Users    
            //    .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

            var user = await _userManager.Users.Include(p => p.UserPhotos)
                .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

            return CreateUserObject(user!);
        }

        [Authorize]
        [HttpGet("Users")]
        public async Task<ActionResult<List<AppUser>>> GetAllUsers()
        {
            var user = await _userManager.Users.ToListAsync();

            return Ok(user);
        }

        private UserDto CreateUserObject(AppUser user)
        {
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Img = user?.UserPhotos.FirstOrDefault(x => x.IsMain)?.Url,
                Token = _tokenService.CreateToken(user!),
                Username = user!.UserName!
            };
        }

    }
}