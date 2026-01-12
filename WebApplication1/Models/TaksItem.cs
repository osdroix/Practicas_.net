namespace WebApplication1.Models
{
    public class TaksItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public int Step { get; set; }
        //Este campo guardar los datos de auditoria.
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CategoryId { get; set; } = 0;
        public Category? Category { get; set; }
    }

}
