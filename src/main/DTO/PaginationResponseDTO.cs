namespace iLib.src.main.DTO
{
    public class PaginationResponse<T>(List<T?> items, int pageNumber, int resultsPerPage, long totalResults, int totalPages)
    {
        public List<T?> Items { get; set; } = items;
        public int PageNumber { get; set; } = pageNumber;
        public int ResultsPerPage { get; set; } = resultsPerPage;
        public long TotalResults { get; set; } = totalResults;
        public int TotalPages { get; set; } = totalPages;
    }
}
