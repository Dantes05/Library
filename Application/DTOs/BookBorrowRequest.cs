namespace Application.DTOs
{
    public class BookBorrowRequest
    {
        public int BookId { get; set; }
        public DateTime ReturnAt { get; set; }
    }
}
