namespace TaskManager.DTOs
    {
        public class SearchFilter
        {
            public string? text { get; set; }
            public bool? completed { get; set; }
            public int? step { get; set; }
            public string? orderBy { get; set; }
        }
    }
}
