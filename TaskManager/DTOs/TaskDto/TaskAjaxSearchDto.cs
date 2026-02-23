namespace TaskManager.DTOs.TaskDto
{
    public class TaskAjaxSearchDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public int Step { get; set; }
    }
}
