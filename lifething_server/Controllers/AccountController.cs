using Microsoft.AspNetCore.Identity;
using lifething_server.Models;
using Microsoft.AspNetCore.Mvc;
using lifething_server.DTO.Requests;
using lifething_server.Helper;
using lifething_server.DTO.Responses;

namespace lifething_server.Controllers
{
    [ApiController] 
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private SignInManager<AppUser> _signInManager{get; set;}
        private UserManager<AppUser> _userManager{get; set;}


        public AccountController(SignInManager<AppUser> signInManager, 
            UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterRequest registerRequest)
        {
            if(!ModelState.IsValid)
            {
                var badRequestResponse = ResponseFormatter.BadRequestResponse<string>(ModelState.Values.ToString());
            }


            var result = await _userManager.CreateAsync(new AppUser{
                Email = registerRequest.Email, 
                EmailConfirmed = true, 
                UserName = ResponseFormatter.GetUsernameFromEmail(registerRequest.Email)
            }, registerRequest.Password);


            if(result.Succeeded)
            {
                var createdResponse = new GenericResponse<string>
                {
                    HttpStatusCode = 201, 
                    ResponseMessage = "User Successfully Created"
                };

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


    }
}