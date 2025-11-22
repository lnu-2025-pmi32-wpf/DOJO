namespace DAL.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public ToDoTask? Task { get; set; }
    }
}
