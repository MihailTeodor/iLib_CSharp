using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.IControllers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using iLib.src.main.Model;

namespace iLib.src.main.services
{
    [Route("loansEndpoint")]
    [ApiController]
    public class LoanEndpoint(ILoanController loanController) : ControllerBase
    {
        private readonly ILoanController _loanController = loanController;

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult RegisterLoan([FromQuery] Guid userId, [FromQuery] Guid articleId)
        {
            if (userId == Guid.Empty)
                return BadRequest(new { error = "Cannot register Loan, User not specified!" });
            if (articleId == Guid.Empty)
                return BadRequest(new { error = "Cannot register Loan, Article not specified!" });

            try
            {
                var loanId = _loanController.RegisterLoan(userId, articleId);
                return Created("", new { loanId });
            }
            catch (UserDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArticleDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exceptions.InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while registering the loan." });
            }
        }

        [HttpPatch("{loanId}/return")]
        [Produces("application/json")]
        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult RegisterReturn(Guid loanId)
        {
            try
            {
                _loanController.RegisterReturn(loanId);
                return Ok(new { message = "Loan successfully returned." });
            }
            catch (LoanDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exceptions.InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while returning the loan." });
            }
        }

        [HttpGet("{loanId}")]
        [Produces("application/json")]
        public IActionResult GetLoanInfo(Guid loanId)
        {
            try
            {
                var loanDTO = _loanController.GetLoanInfo(loanId);
                return Ok(loanDTO);
            }
            catch (LoanDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving the loan information." });
            }
        }

        [HttpGet("{userId}/loans")]
        [Produces("application/json")]
        [Authorize]
        public IActionResult GetLoansByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int resultsPerPage = 10)
        {
            if (pageNumber < 1 || resultsPerPage < 0)
            {
                return BadRequest(new { error = "Pagination parameters incorrect!" });
            }

            var loggedUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (User.FindFirstValue(ClaimTypes.Role) != UserRole.ADMINISTRATOR.ToString() && loggedUserId != userId.ToString())
            {
                return Unauthorized();
            }

            try
            {
                long totalResults = _loanController.CountLoansByUser(userId);
                int totalPages = (int)Math.Ceiling((double)totalResults / resultsPerPage);

                if (pageNumber > totalPages)
                {
                    pageNumber = totalPages == 0 ? 1 : totalPages;
                }

                int fromIndex = (pageNumber - 1) * resultsPerPage;

                var loansDTOs = _loanController.GetLoansByUser(userId, fromIndex, resultsPerPage)
                                               .Select(loan => new LoanDTO(loan))
                                               .ToList<LoanDTO?>();

                var response = new PaginationResponse<LoanDTO>(
                    loansDTOs,
                    pageNumber,
                    resultsPerPage,
                    totalResults,
                    totalPages
                );

                return Ok(response);
            }
            catch (UserDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (LoanDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving user loans." });
            }
        }

        [HttpPatch("{loanId}/extend")]
        [Produces("application/json")]
        [Authorize]
        public IActionResult ExtendLoan(Guid loanId)
        {
            var loggedUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            try
            {
                var loaningUserId = _loanController.GetLoanInfo(loanId).LoaningUserId;
                if (User.FindFirstValue(ClaimTypes.Role) != UserRole.ADMINISTRATOR.ToString() && loggedUserId != loaningUserId.ToString())
                {
                    return Unauthorized();
                }
                _loanController.ExtendLoan(loanId);
                return Ok(new { message = "Loan extended successfully." });
            }
            catch (LoanDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exceptions.InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    }
}
