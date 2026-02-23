namespace TaskManager.DTOs.TaskDto
{
    public class TaskImportResultDto
    {
        public int Guardados { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
    }
}
