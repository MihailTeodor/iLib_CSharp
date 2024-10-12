using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iLib.src.main.DTO;
using iLib.src.main.Exceptions;
using iLib.src.main.IServices;

namespace iLib.src.main.services
{
    [Route("articlesEndpoint")]
    [ApiController]
    public class ArticleController(IArticleService articleService) : ControllerBase
    {
        private readonly IArticleService _articleService = articleService;

        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult CreateArticle([FromBody] ArticleDTO articleDTO)
        {
            try
            {
                Guid id = _articleService.AddArticle(articleDTO);
                return Created("", new { articleId = id });
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { error = e.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while registering the article." });
            }
        }

        [HttpPut("{articleId}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult UpdateArticle(Guid articleId, [FromBody] ArticleDTO articleDTO)
        {
            try
            {
                _articleService.UpdateArticle(articleId, articleDTO);
                return Ok(new { message = "Article updated successfully." });
            }
            catch (ArticleDoesNotExistException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidStateTransitionException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while updating the article." });
            }
        }

        [HttpGet("{articleId}")]
        [Produces("application/json")]
        public IActionResult GetArticleInfo(Guid articleId)
        {
            try
            {
                var articleDTO = _articleService.GetArticleInfoExtended(articleId);
                return Ok(articleDTO);
            }
            catch (ArticleDoesNotExistException e)
            {
                return NotFound(new { error = e.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving the article information." });
            }
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult SearchArticles([FromQuery] string? isbn, [FromQuery] string? issn, [FromQuery] string? isan, [FromQuery] string? title, [FromQuery] string? genre, [FromQuery] string? publisher, [FromQuery] string? yearEdition, [FromQuery] string? author, [FromQuery] int? issueNumber, [FromQuery] string? director, [FromQuery] int pageNumber = 1, [FromQuery] int resultsPerPage = 10)
        {
            if (pageNumber < 1 || resultsPerPage < 0)
            {
                return BadRequest(new { error = "Pagination parameters incorrect!" });
            }

            DateTime? _yearEdition = null;
            if (yearEdition != null)
            {
                if (!DateTime.TryParse(yearEdition, out DateTime parsedDate))
                {
                    return BadRequest(new { error = "Invalid date format for 'yearEdition', expected format YYYY-MM-DD." });
                }
                _yearEdition = parsedDate;
            }

            try
            {
                long totalResults = _articleService.CountArticles(isbn, issn, isan, title, genre, publisher, _yearEdition, author, issueNumber, director);
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

                var articleDTOs = _articleService.SearchArticles(isbn, issn, isan, title, genre, publisher, _yearEdition, author, issueNumber, director, fromIndex, resultsPerPage)
                    .Select(article => ArticleMapper.ToDTO(article, null, null))
                    .ToList();

                var response = new PaginationResponse<ArticleDTO>(
                    articleDTOs,
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
            catch (Exception e)
            {
                return StatusCode(500, new { error = "An error occurred during article search. " + e.Message });
            }
        }

        [HttpDelete("{articleId}")]
        [Produces("application/json")]
        [Authorize(Roles = "ADMINISTRATOR")]
        public IActionResult DeleteArticle(Guid articleId)
        {
            try
            {
                _articleService.RemoveArticle(articleId);
                return Ok(new { message = "Article deleted successfully." });
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
                return StatusCode(500, new { error = "An error occurred during article deletion." });
            }
        }
    }
}
