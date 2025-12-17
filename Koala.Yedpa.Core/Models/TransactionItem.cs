using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class TransactionItem : CommonProperties
    {
        public string Id { get; set; } = Tools.CreateGuidStr();
        public string? TransactionId { get; set; }
        public string? Description { get; set; }
        public bool IsSuccess { get; set; } = false;
        public DateTime? UpdateTime { get; set; }

        public virtual Transaction? Transaction { get; set; }
    }
}
