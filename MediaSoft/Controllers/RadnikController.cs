using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MediaSoft.Data;
using MediaSoft.Data.Extensions;
using MediaSoft.Data.InputObjects;
using MediaSoft.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation;

namespace MediaSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RadnikController : ControllerBase
    {
        public RoleManager<IdentityRole> RoleManager { get; }
        public UserManager<Radnik> UserManager { get; }
        public SignInManager<Radnik> SignInManager { get; }
        private readonly IConfiguration configuration;
        

        // GET api/values
        [HttpGet]
        [Authorize(Policy = "NivoPristupaPolicy")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { HttpContext.User.Identity.Name };
        }

        public RadnikController(IConfiguration conf, UserManager<Radnik> userManager, SignInManager<Radnik> signInManager)
        {
            configuration = conf;
            UserManager = userManager;
            SignInManager = signInManager;
        }

        [HttpPost("~/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginObject val)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(val.Username);
                if (user != null)
                {
                    var result = await SignInManager.PasswordSignInAsync(val.Username, val.PasswordHash, false, false);
                    if (result.Succeeded)
                    {
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var nowUtc = DateTime.Now.ToUniversalTime();
                        var expires = nowUtc.AddMinutes(double.Parse(configuration["Tokens:ExpiryMinutes"])).ToUniversalTime();
                       
                        var claims = new List<Claim>();
                        //claims.Add(new Claim("name", user.Username));
                        //claims.Add(new Claim("iss", configuration["Tokens:Issuer"]));
                        var token = new JwtSecurityToken(
                            configuration.GetConfigValue("Tokens:Issuer"),
                            configuration.GetConfigValue("Tokens:Audience"),
                            claims: claims,
                            expires: expires,
                            signingCredentials: creds
                        );

                        var response = new JwtSecurityTokenHandler().WriteToken(token);
                        return Ok(response);
                    }
                    ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                    return BadRequest();
                }
                return BadRequest("Login unsuccessful");
            }
            return BadRequest("Bad input data");
        }

        [HttpPost("~/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterObject val)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid input format");
            }
            else
            {
                var user = await UserManager.FindByIdAsync(val.Korisnicko_ime);
                if(user != null)
                {
                    return BadRequest("User already exists.");
                }
                else
                {
                    var radnik = new Radnik
                    {
                        Korisnicko_ime = val.Korisnicko_ime,
                        Prezime = val.Prezime,
                        Ime = val.Ime,
                        Pwd = val.PWD,
                        Lozinka = val.Lozinka
                    };
                    var result = await UserManager.CreateAsync(radnik);
                    if (result.Succeeded)
                    {
                        var claims = new List<Claim>();
                        claims.Add(new Claim(val.Claim, val.Claim));
                        var rezultat = await UserManager.AddClaimAsync(radnik, new Claim(val.Claim, val.Claim));
                        if (rezultat.Succeeded)
                        {
                            await SignInManager.SignInAsync(radnik, isPersistent: false);
                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Tokens:Key"]));
                            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                            var nowUtc = DateTime.Now.ToUniversalTime();
                            var expires = nowUtc.AddMinutes(double.Parse(configuration["Tokens:ExpiryMinutes"])).ToUniversalTime();

                            //claims.Add(new Claim("name", user.Username));
                            //claims.Add(new Claim("iss", configuration["Tokens:Issuer"]));
                            var token = new JwtSecurityToken(
                                configuration.GetConfigValue("Tokens:Issuer"),
                                configuration.GetConfigValue("Tokens:Audience"),
                                claims: claims,
                                expires: expires,
                                signingCredentials: creds
                            );

                            var response = new JwtSecurityTokenHandler().WriteToken(token);
                            return Ok(response);
                        }
                    }
                    return BadRequest("Something went wrong..");
                }
            }
        }
    }
}
