using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Identity.Dtos;
using Identity.Reposi;
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

        public UserController(  IConfiguration config,
                                UserManager<User> userManager,
                                SignInManager<User> singInManager,
                                IMapper mapper)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = singInManager;
            _mapper = mapper;
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
            var result = await _signInManager.CheckPasswordSignInAsync(user, userDto.Password, false);
            if (result.Succeeded)
            {
                var appUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.NormalizedUserName == userDto.Username.ToUpper());
                var s = _userManager.IsLockedOutAsync(appUser);
                var userToReturn = _mapper.Map<UserLoginDto>(appUser);
                var token = GenerationJwtToken(appUser).Result;
                return Ok(new {
                    bearer = token,
                    user = userToReturn
                });
                
            }
            return Unauthorized();
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
                    var x = await _userManager.AddClaimAsync(user, new Claim("teste", "userJr"));
                    if (x.Succeeded)
                    {
                    var userToResult = _mapper.Map<UserDto>(user);
                        return Created("Login", userToResult);  
                    }                    
                }
                return BadRequest(result.Errors);
            }
            catch (System.Exception ex)
            {                
               return this.StatusCode(StatusCodes.Status500InternalServerError, $"Banco falhou {ex.Message}");
            }
        }
        
        //[Authorize(Roles="userJr")]
        
        [HttpGet] 
        [ClaimsAuthorize("teste", "userJr")]       
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
