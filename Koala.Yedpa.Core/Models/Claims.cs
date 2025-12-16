using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class Claims
    {
        public string Id { get; set; } = Tools.CreateGuidStr();
        public string ModuleId { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public virtual Module Module { get; set; }
    }
}
