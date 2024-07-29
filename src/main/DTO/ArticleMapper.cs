using iLib.src.main.Model;

namespace iLib.src.main.DTO
{
    public static class ArticleMapper
    {
        public static ArticleDTO? ToDTO(Article article, DateTime? loanDueDate, DateTime? bookingEndDate)
        {
            if (article == null)
                return null;

            var dto = new ArticleDTO
            {
                Id = article.Id,
                Location = article.Location,
                Title = article.Title,
                YearEdition = article.YearEdition,
                Publisher = article.Publisher,
                Genre = article.Genre,
                Description = article.Description,
                State = article.State,
                LoanDueDate = loanDueDate,
                BookingEndDate = bookingEndDate
            };

            switch (article)
            {
                case Book book:
                    dto.Type = ArticleType.BOOK;
                    dto.Author = book.Author;
                    dto.Isbn = book.Isbn;
                    break;
                case Magazine magazine:
                    dto.Type = ArticleType.MAGAZINE;
                    dto.IssueNumber = magazine.IssueNumber;
                    dto.Issn = magazine.Issn;
                    break;
                case MovieDVD movieDVD:
                    dto.Type = ArticleType.MOVIEDVD;
                    dto.Director = movieDVD.Director;
                    dto.Isan = movieDVD.Isan;
                    break;
            }

            return dto;
        }

        public static Article ToEntity(ArticleDTO dto)
        {
            return dto.Type switch
            {
                ArticleType.BOOK => CreateBook(dto),
                ArticleType.MAGAZINE => CreateMagazine(dto),
                ArticleType.MOVIEDVD => CreateMovieDVD(dto),
                _ => throw new ArgumentException("Unknown article type: " + dto.Type),
            };
        }

        private static Book CreateBook(ArticleDTO dto)
        {
            if (dto.Isbn == null)
                throw new ArgumentException("Article identifier is required");

            if (dto.Author == null)
                throw new ArgumentException("Author is required");

            var book = ModelFactory.CreateBook();
            book.Title = dto.Title;
            book.Location = dto.Location;
            book.YearEdition = dto.YearEdition ?? default(DateTime);
            book.Publisher = dto.Publisher;
            book.Genre = dto.Genre;
            book.Description = dto.Description;
            book.Author = dto.Author;
            book.Isbn = dto.Isbn;
            return book;
        }

        private static Magazine CreateMagazine(ArticleDTO dto)
        {
            if (dto.Issn == null)
                throw new ArgumentException("Article identifier is required");

            if (dto.IssueNumber == null)
                throw new ArgumentException("Issue Number is required");

            var magazine = ModelFactory.CreateMagazine();
            magazine.Title = dto.Title;
            magazine.Location = dto.Location;
            magazine.YearEdition = dto.YearEdition ?? default(DateTime);
            magazine.Publisher = dto.Publisher;
            magazine.Genre = dto.Genre;
            magazine.Description = dto.Description;
            magazine.IssueNumber = dto.IssueNumber.Value;
            magazine.Issn = dto.Issn;
            return magazine;
        }

        private static MovieDVD CreateMovieDVD(ArticleDTO dto)
        {
            if (dto.Isan == null)
                throw new ArgumentException("Article identifier is required");

            if (dto.Director == null)
                throw new ArgumentException("Director is required");

            var movieDVD = ModelFactory.CreateMovieDVD();
            movieDVD.Title = dto.Title;
            movieDVD.Location = dto.Location;
            movieDVD.YearEdition = dto.YearEdition ?? default(DateTime);
            movieDVD.Publisher = dto.Publisher;
            movieDVD.Genre = dto.Genre;
            movieDVD.Description = dto.Description;
            movieDVD.Director = dto.Director;
            movieDVD.Isan = dto.Isan;
            return movieDVD;
        }
    }
}
