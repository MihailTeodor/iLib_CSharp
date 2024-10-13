using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.IControllers;
using iLib.src.main.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iLib.src.main.services
{
    [Route("usersEndpoint")]
    [ApiController]
    public class UserEndpoint(IUserController userController) : ControllerBase
    {
        private readonly IUserController _userController = userController;

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetUserInfo(Guid id)
        {
            var loggedUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var loggedUserRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (loggedUserRole != UserRole.ADMINISTRATOR.ToString() && loggedUserId != id.ToString())
            {
                return Forbid();
            }

            try
            {
                var userDto = _userController.GetUserInfoExtended(id);
                return Ok(userDto);
            }
            catch (UserDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving the user information." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult CreateUser([FromBody] UserDTO userDTO)
        {
            try
            {
                var id = _userController.AddUser(userDTO);
                return Created("", new { userId = id });
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { error = e.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while registering the user." });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult UpdateUser(Guid id, [FromBody] UserDTO userDTO)
        {
            var loggedUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var loggedUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (loggedUserRole != UserRole.ADMINISTRATOR.ToString() && loggedUserId != id.ToString())
            {
                return Forbid();
            }

            try
            {
                _userController.UpdateUser(id, userDTO);
                return Ok(new { message = "User updated successfully." });
            }
            catch (UserDoesNotExistException e)
            {
                return NotFound(new { error = e.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while updating the user." });
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult SearchUsers([FromQuery] string? email,
                                        [FromQuery] string? name,
                                        [FromQuery] string? surname,
                                        [FromQuery] string? telephoneNumber,
                                        [FromQuery] int pageNumber = 1,
                                        [FromQuery] int resultsPerPage = 10)
        {
            if (pageNumber < 1 || resultsPerPage < 0)
            {
                return BadRequest(new { error = "Pagination parameters incorrect!" });
            }

            try
            {
                long totalResults = _userController.CountUsers(email, name, surname, telephoneNumber);
                int totalPages = (int)Math.Ceiling((double)totalResults / resultsPerPage);

                if (pageNumber > totalPages)
                {
                    pageNumber = totalPages == 0 ? 1 : totalPages;
                }

                int fromIndex = (pageNumber - 1) * resultsPerPage;
                var userDTOs = _userController.SearchUsers(email, name, surname, telephoneNumber, fromIndex, resultsPerPage)
                                             .Select(user => new UserDTO(user))
                                             .ToList<UserDTO?>();

                var response = new PaginationResponse<UserDTO>(
                    userDTOs,
                    pageNumber,
                    resultsPerPage,
                    totalResults,
                    totalPages
                );

                return Ok(response);
            }
            catch (SearchHasGivenNoResultsException e)
            {
                return NotFound(new { error = e.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred during user search." });
            }
        }
    }
}
