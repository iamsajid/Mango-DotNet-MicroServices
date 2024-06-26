﻿using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDto loginRequestDto = new LoginRequestDto();
            return View(loginRequestDto);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            ResponseDto? response = await _authService.LoginAsync(model);

            if (response != null && response.IsSuccess)
            {
                LoginResponseDto responseDto =
                    JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(response.Result));

                await SignIn(responseDto);
                _tokenProvider.SetToken(responseDto.Token);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["error"] = response?.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new() { Text = SD.RoleAdmin, Value = SD.RoleAdmin },
                new() { Text = SD.RoleCustomer, Value = SD.RoleCustomer },
            };
            ViewBag.RoleList = roleList;    

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto model)
        {
            ResponseDto response = await _authService.RegisterAsync(model);
            ResponseDto assignRole;

            if (response != null && response.IsSuccess)
            {
                if (string.IsNullOrEmpty(model.Role))
                {
                    model.Role = SD.RoleCustomer;
                }
                assignRole = await _authService.AssignRoleAsync(model);
                if (assignRole != null && assignRole.IsSuccess)
                {
                    TempData["success"] = "Registration Successful";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = response.Message;
            }

            var roleList = new List<SelectListItem>()
            {
                new() { Text = SD.RoleAdmin, Value = SD.RoleAdmin },
                new() { Text = SD.RoleCustomer, Value = SD.RoleCustomer },
            };
            ViewBag.RoleList = roleList;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        private async Task SignIn(LoginResponseDto loginResponseDto)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwtToken = handler.ReadJwtToken(loginResponseDto.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email, 
                jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub).Value));

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,
                jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value));

            identity.AddClaim(new Claim(ClaimTypes.Role,
                jwtToken.Claims.FirstOrDefault(x => x.Type == "role").Value));

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
