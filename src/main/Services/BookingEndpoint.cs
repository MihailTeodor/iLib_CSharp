
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
    [Route("bookingsEndpoint")]
    [ApiController]
    public class BookingEndpoint(IBookingController bookingController) : ControllerBase
    {
        private readonly IBookingController _bookingController = bookingController;

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize]
        public IActionResult RegisterBooking([FromQuery] Guid userId, [FromQuery] Guid articleId)
        {
            if (userId == Guid.Empty)
                return BadRequest(new { error = "Cannot register Booking, User not specified!" });
            if (articleId == Guid.Empty)
                return BadRequest(new { error = "Cannot register Booking, Article not specified!" });

            var loggedUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (User.FindFirstValue(ClaimTypes.Role) != UserRole.ADMINISTRATOR.ToString() && loggedUserId != userId.ToString())
            {
                return Unauthorized();
            }

            try
            {
                var bookingId = _bookingController.RegisterBooking(userId, articleId);
                return Created("", new { bookingId });
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
                return StatusCode(500, new { error = "An error occurred while registering the booking." });
            }
        }

        [HttpGet("{bookingId}")]
        [Produces("application/json")]
        public IActionResult GetBookingInfo(Guid bookingId)
        {
            try
            {
                var bookingDTO = _bookingController.GetBookingInfo(bookingId);
                return Ok(bookingDTO);
            }
            catch (BookingDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving the booking information." });
            }
        }

        [HttpPatch("{bookingId}/cancel")]
        [Produces("application/json")]
        [Authorize]
        public IActionResult CancelBooking(Guid bookingId)
        {
            var loggedUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            try
            {
                var bookingUserId = _bookingController.GetBookingInfo(bookingId).BookingUserId;

                if (User.FindFirstValue(ClaimTypes.Role) != UserRole.ADMINISTRATOR.ToString() && loggedUserId != bookingUserId.ToString())
                {
                    return Unauthorized();
                }

                _bookingController.CancelBooking(bookingId);
                return Ok(new { message = "Booking cancelled successfully." });
            }
            catch (BookingDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exceptions.InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while attempting to cancel the booking." });
            }
        }

        [HttpGet("{userId}/bookings")]
        [Produces("application/json")]
        [Authorize]
        public IActionResult GetBookedArticlesByUser(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int resultsPerPage = 10)
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
                long totalResults = _bookingController.CountBookingsByUser(userId);
                int totalPages = (int)Math.Ceiling((double)totalResults / resultsPerPage);

                if (pageNumber > totalPages)
                {
                    pageNumber = totalPages == 0 ? 1 : totalPages;
                }

                int fromIndex = (pageNumber - 1) * resultsPerPage;
                if (fromIndex < 0)
                {
                    return BadRequest(new { error = "For strange reasons the fromIndex parameter is negative!" });
                }

                var bookingDTOs = _bookingController.GetBookingsByUser(userId, fromIndex, resultsPerPage)
                                                    .Select(booking => new BookingDTO(booking))
                                                    .ToList<BookingDTO?>();

                var response = new PaginationResponse<BookingDTO>(
                    bookingDTOs,
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
            catch (SearchHasGivenNoResultsException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving user bookings." });
            }
        }
    }
}
