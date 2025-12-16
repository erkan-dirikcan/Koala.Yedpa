using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class Module
    {
        public Module()
        {
            GeneratedIds = new HashSet<GeneratedIds>();
            Claims = new HashSet<Claims>();
        }

        public string Id { get; set; } = Tools.CreateGuidStr();
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public string Description { get; set; }

        public StatusEnum Status { get; set; } = StatusEnum.Active;

        public virtual ICollection<GeneratedIds> GeneratedIds { get; set; }
        public virtual ICollection<Claims> Claims { get; set; }
        public virtual ICollection<ExtendedProperties> ExtendedProperties { get; set; } = new HashSet<ExtendedProperties>();

    }
}
