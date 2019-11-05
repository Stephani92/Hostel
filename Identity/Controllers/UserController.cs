using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Models;
using Identity.Dtos;
using Identity.Reposi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    
    public class UserController : Controller
    {   
        private readonly IConfiguration _config;        
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly IUserClaimsPrincipalFactory<User> _factory;

        public UserController(  IConfiguration config,
                                UserManager<User> userManager,
                                SignInManager<User> singInManager,
                                IMapper mapper,
                                IUserClaimsPrincipalFactory<User> factory)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = singInManager;
            _mapper = mapper;
            _factory = factory;
        }

        // GET api/values/5
        [HttpGet("Get")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUser(UserDto userDto)
        {
            return Ok(new UserDto());
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var user = await _userManager.FindByNameAsync(userDto.Username);
            if (user != null && !await _userManager.IsLockedOutAsync(user))
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, userDto.Password, false);
                if (result.Succeeded)
                {   
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        return Unauthorized("Email not confirm.");
                    }
                    var appUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.NormalizedUserName == userDto.Username.ToUpper());
                    await _userManager.ResetAccessFailedCountAsync(appUser);
                    if (await _userManager.GetTwoFactorEnabledAsync(appUser))
                    {   
                        
                        var validor = await _userManager.GetValidTwoFactorProvidersAsync(appUser);
                        if (validor.Contains("Email"))
                        {
                            var token_ = await _userManager.GenerateTwoFactorTokenAsync(appUser, "Email");
                            await System.IO.File.WriteAllTextAsync("token2sv.txt", token_);
                            await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, Store2FA(appUser.Id, "Email"));
                        }
                        var Principal = await _factory.CreateAsync(appUser);
                        await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, Principal);
                    }
                    
                    var userToReturn = _mapper.Map<UserLoginDto>(appUser);
                    var token = GenerationJwtToken(appUser).Result;
                    return Ok(new {
                        bearer = token,
                        user = userToReturn
                    });
                    
                }
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Unauthorized("User is blocked.");
                }
                return Unauthorized("Password not match");
            }
            
            return NotFound("User not found");
        }

        private ClaimsPrincipal Store2FA(string id, string provider)
        {   
            var identity_ = new ClaimsIdentity( new List<Claim>
            {
                new Claim("sub", id),
                new Claim("arm", provider)
            }, IdentityConstants.TwoFactorUserIdScheme);
            return new ClaimsPrincipal(identity_);
        }
        [HttpPost("test")]
        [AllowAnonymous]
        public async Task<IActionResult> test(stringDto id)
        {
            var user = await _userManager.FindByNameAsync(id.id);
            
            return Ok(await _userManager.SetTwoFactorEnabledAsync(user, true));

        }
        [HttpPost("TwoFactor")]
        [AllowAnonymous]
        public async Task<IActionResult> TwoFactor(TwoFactorModel model)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
            if (!result.Succeeded)
            {
                return Unauthorized("Token expired. ");
            } else
            {
                var user = await _userManager.FindByIdAsync(result.Principal.FindFirstValue(@"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"));
                if (user != null)
                {
                    var isvalid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", model.Token);
                    if (isvalid)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
                        var claimsPrincipal = await _factory.CreateAsync(user);
                        await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, claimsPrincipal);
                        return Ok("Confirmed token");
                    }
                    return Unauthorized("Invalid token");
                }
                return NotFound("User not found");
            }
        }

        private async Task<string> GenerationJwtToken(User user)
        {
            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var roles = await _userManager.GetClaimsAsync(user);

            foreach (var role in roles)
            {
                claims.Add(role);
            }
            
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:Token").Value));
            

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
        

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserDto userDto)
        {

            try
            {
                var user = _mapper.Map<User>(userDto);
                var result = await _userManager.CreateAsync(user, userDto.Password);
                
                if (result.Succeeded)
                {   
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);                    
                    var confirmEmail = Url.Action("ConfirmEmailAddress", "User", new { token = token, email = user.Email }, Request.Scheme);
                    await System.IO.File.WriteAllTextAsync("ConfirEmail.txt", confirmEmail);
                    
                    var userToResult = _mapper.Map<UserDto>(user);
                    return Created("Login", userToResult);                
                }
                return BadRequest(result.Errors);
            }
            catch (System.Exception ex)
            {                
               return this.StatusCode(StatusCodes.Status500InternalServerError, $"Banco falhou {ex.Message}");
            }
        }

        [HttpGet("ConfirmEmailAddress")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailAddress(string token, string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var result = await _userManager.ConfirmEmailAsync(user, token);
                    if (result.Succeeded)
                    {
                        return Ok("E-mail confirmado");
                    }
                }

                return NotFound("User not found");
            }
            catch (System.Exception ex )
            {                
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Banco falhou {ex.Message}");
            }
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPassword forgot) { 
            
            try
            {
                var user = await _userManager.FindByEmailAsync(forgot.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetUrl = Url.Action("ResetPassword", "User", new {token = token, Email = forgot.Email}, Request.Scheme);
                    await System.IO.File.WriteAllTextAsync("resetLink.txt", resetUrl);
                    
                }
                return Ok("Success");
            }
            catch (System.Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Banco falhou {ex.Message}");
            }
        }
        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email){

            var reset = new ResetPassword{
                Token = token,
                Email = email
            };
            return Ok(reset);
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPassword reset) { 
            
            try
            {
                var user = await _userManager.FindByEmailAsync(reset.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, reset.Token, reset.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError("", item.Description);
                        }

                        return NotFound(ModelState);
                       
                    }
                    return Ok("Password changed");
                }
                return NotFound("User not found");
            }
            catch (System.Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Banco falhou {ex.Message}");
            }
        }
        //[Authorize(Roles="userJr")]
        
        [HttpGet]  
        public async Task<string> GetUser()
        {
            return "Sucesso";
        }
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
