namespace WebApplication1.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<TaksItem> Tasks { get; set; } = new List<TaksItem>();
    }

}
