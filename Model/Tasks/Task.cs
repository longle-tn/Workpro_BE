namespace Container_App.Model.Tasks
{
    public class Task
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public int AssignedTo { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
