using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class TransactionType
    {
        public TransactionType()
        {
            Transactions = new HashSet<Transaction>();
        }
        public string Id { get; set; } = Tools.CreateGuidStr();
        public string? ColorClass { get; set; }
        public string? Icon { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }


        public StatusEnum Status { get; set; } = StatusEnum.Active;
        public virtual ICollection<Transaction> Transactions { get; set; }

    }
}
