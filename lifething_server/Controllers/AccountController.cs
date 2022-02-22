using lifething_server.DTO.Requests;
using lifething_server.DTO.Responses;
using lifething_server.Helper;
using lifething_server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace lifething_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private SignInManager<AppUser> _signInManager { get; set; }
        private UserManager<AppUser> _userManager { get; set; }
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration; 


        public AccountController(SignInManager<AppUser> signInManager, ILogger<AccountController> logger,
            IConfiguration configuration,
            UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                var badRequestResponse = ResponseFormatter.BadRequestResponse<string>(ModelState.Values.ToString());
                return BadRequest(badRequestResponse);
            }


            var result = await _userManager.CreateAsync(new AppUser
            {
                Email = registerRequest.Email,
                EmailConfirmed = true,
                UserName = ResponseFormatter.GetUsernameFromEmail(registerRequest.Email)
            }, registerRequest.Password);


            if (result.Succeeded)
            {
                var createdResponse = new GenericResponse<string>
                {
                    HttpStatusCode = 201,
                    ResponseMessage = "User Successfully Created"
                };

                _logger.LogInformation("User Joined");

                return StatusCode(201, createdResponse);
            }

            List<string> registrationErrors = new List<string>();

            foreach (var error in result.Errors)
            {
                registrationErrors.Add(error.Description);
            }

            var errorResponse = new GenericResponse<List<string>>
            {
                Data = registrationErrors,
                HttpStatusCode = 409
            };

            return StatusCode(409, errorResponse);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
            {
                var badRequestResponse = ResponseFormatter.BadRequestResponse(ModelState.Values.ToString());
                return StatusCode(badRequestResponse.HttpStatusCode.Value, badRequestResponse);
            }


            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                var badRequest = new GenericResponse<string>
                {
                    HttpStatusCode = (int)HttpStatusCode.BadRequest,
                    ResponseMessage = "User with Email does not exist",
                    Data = null
                };
                return BadRequest(badRequest);
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
            if (!isPasswordCorrect)
            {
                var badRequest = new GenericResponse<string>
                {
                    HttpStatusCode = (int)HttpStatusCode.Unauthorized,
                    ResponseMessage = "Incorrect Password",
                    Data = null
                };
                return Unauthorized(badRequest);
            }

            var bearerToken = SecurityHelper.GenerateBearerToken(loginRequest.Email, 12, _configuration.GetSection("LtJwt:Key").Value.ToString());

            var loginResult = new GenericResponse<string>
            {
                Data = bearerToken,
                HttpStatusCode = (int)HttpStatusCode.OK,
                ResponseMessage = "Login Successful"
            };

            return Ok(loginResult);

        }

        
        [HttpGet("Logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            var logOut = new GenericResponse<string>
            {
                HttpStatusCode = (int)HttpStatusCode.OK,
                ResponseMessage = "User logged out"
            };

            return Ok(logOut);
        }


    }
}