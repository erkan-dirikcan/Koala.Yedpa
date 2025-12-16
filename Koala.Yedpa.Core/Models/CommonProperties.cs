using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models
{
    public class CommonProperties
    {
        public string? CreateUserId { get; set; }
        public DateTime CreateTime { get; set; }=DateTime.UtcNow;
        public string? LastUpdateUserId { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public StatusEnum Status { get; set; }
    }
}
