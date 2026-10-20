namespace SmartTaskEngine.DTOs.Dashboard
{
    public class DashboardDto
    {
        public int TotalTasks { get; set; }
        public int ToDo { get; set; }
        public int InProgress { get; set; }
        public int Done { get; set; }
        public int Overdue { get; set; }
        public int AssignedToMe { get; set; }
        public int Projects { get; set; }
        public int OpenChecklistItems { get; set; }
        public int UnreadNotifications { get; set; }
        public int Bookmarks { get; set; }
    }
}
