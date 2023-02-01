using AspAuthDemoApp.Core.Controllers;
using AspAuthDemoApp.Feature.Authentication.Config;
using AspAuthDemoApp.Feature.Authentication.Models;
using AspAuthDemoApp.Feature.Authentication.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AspAuthDemoApp.Feature.Authentication.Controllers
{
    [Route("api/auth")]
    public class AuthenticationController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<AuthenticationController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get an access token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [Produces(typeof(TokenResponse))]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var jwtConfig = _configuration.GetJwtConfig();

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret));

                var token = new JwtSecurityToken(
                    issuer: jwtConfig.ValidIssuer,
                    audience: jwtConfig.ValidAudience,
                    //notBefore: DateTime.UtcNow.AddHours(3),
                    expires: DateTime.UtcNow.AddHours(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                _logger.LogInformation("{Username} logged in succesfully", model.Username);
                return Ok(new TokenResponse(
                    new JwtSecurityTokenHandler().WriteToken(token),
                    token.ValidTo, token.ValidFrom));
                    //token.ValidTo, token.ValidTo.AddHours(-1)));
            }

            _logger.LogError("{Username} failed to login", model.Username);
            return Unauthorized();
        }

        /// <summary>
        /// Create a normal user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("register")]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                _logger.LogError("{Username} aleready exists", model.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServerResponse { Status = "Error", Message = "User already exists!" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                LockoutEnabled = false
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("{Username} aleready exists", model.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServerResponse { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            _logger.LogInformation("{Username} created succesfully", model.Username);

            return Ok(new ServerResponse { Status = "Success", Message = "User created successfully!" });
        }

        /// <summary>
        /// Create a demo admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                _logger.LogError("{Username} aleready exists", model.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServerResponse { Status = "Error", Message = "User already exists!" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                LockoutEnabled = false
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("{Username} aleready exists", model.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new ServerResponse { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            return Ok(new ServerResponse { Status = "Success", Message = "User created successfully!" });
        }
    }
}
