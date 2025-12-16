using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class EmailTemplate : CommonProperties
    {
        public string Id { get; set; } = Tools.CreateGuidStr();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }
}
