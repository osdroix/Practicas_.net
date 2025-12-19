namespace WebApplication1.DTOs
{
    public class CreateTaskRequest
    {
        public string Title { get; set; }
        public bool IsCompleted { get; internal set; }
    }
}
