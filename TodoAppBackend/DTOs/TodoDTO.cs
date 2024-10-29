namespace TodoAppBackend.DTOs
{
    public class TodoDTO
    {
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsCompleted { get; set; } = false;
    }
}


