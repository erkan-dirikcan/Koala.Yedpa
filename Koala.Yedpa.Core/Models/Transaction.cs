using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class Transaction : CommonProperties
    {
        public Transaction()
        {
            TransactionItems = new HashSet<TransactionItem>();
        }

        public string Id { get; set; } = Tools.CreateGuidStr();
        public string TransactionNumber { get; set; }
        public string? TransactionTypeId { get; set; }
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsComplated { get; set; } = false;


        public virtual TransactionType? TransactionType { get; set; }
        public virtual AppUser? AppUser { get; set; }
        public virtual ICollection<TransactionItem> TransactionItems { get; set; }
    }
}
