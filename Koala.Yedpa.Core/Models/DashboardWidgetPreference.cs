namespace Koala.Yedpa.Core.Models
{
    public class DashboardWidgetPreference
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string WidgetId { get; set; } = string.Empty;
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Visible { get; set; } = true;
    }
}
