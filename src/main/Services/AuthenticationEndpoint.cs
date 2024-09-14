using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.IControllers;
using iLib.src.main.rest;
using Microsoft.AspNetCore.Mvc;

namespace iLib.src.main.Services
{
    [Route("auth")]
    [ApiController]
    public class AuthenticationEndpoint(IUserController userController, ILogger<AuthenticationEndpoint> logger) : ControllerBase
    {
        private readonly IUserController _userController = userController;
        private readonly ILogger<AuthenticationEndpoint> _logger = logger;

        [HttpPost("login")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public IActionResult LoginUser([FromBody] LoginDTO credentials)
        {
            try
            {
                var user = _userController.SearchUsers(credentials.Email, null, null, null, 0, 1).FirstOrDefault();
                if (user == null || user.Email == null || !BCrypt.Net.BCrypt.Verify(credentials.Password, user.Password))
                {
                    return Unauthorized(new { error = "Credentials are invalid." });
                }

                var token = JwtHelper.GenerateToken(user.Id, user.Email, user.Role.ToString());
                return Ok(new { token, userId = user.Id, role = user.Role.ToString() });
            }
            catch (SearchHasGivenNoResultsException)
            {
                return Unauthorized(new { error = "Credentials are invalid." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user search.");
                return StatusCode(500, new { error = "An error occurred during user search." });
            }
        }
    }
}
