namespace WebApplication1.DTOs
{
    public class UpdateTaksRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
