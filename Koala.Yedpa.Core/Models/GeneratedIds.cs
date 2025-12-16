using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class GeneratedIds : CommonProperties
    {
        public string Id { get; set; } = Tools.CreateGuidStr();
        public string ModuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Prefix { get; set; }//lgp
        public int StartNumber { get; set; }//0
        public int LastNumber { get; set; }//1
        public int Digit { get; set; }//6

        public virtual Module Module { get; set; }
    }
}
