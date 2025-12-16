using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class ExtendedProperties : CommonProperties
    {
        public ExtendedProperties()
        {
            Values = new HashSet<ExtendedPropertyValues>();
            RecordValues = new HashSet<ExtendedPropertyRecordValues>();
        }
        public string Id { get; set; } = Tools.CreateGuidStr();
        public string? ModuleId { get; set; }
        public required string? Name { get; set; }
        public required string? DisplayName { get; set; }
        public required string? Description { get; set; }
        public required ExtendedPropertyShowOnEnum ShowOn { get; set; } = ExtendedPropertyShowOnEnum.Detail | ExtendedPropertyShowOnEnum.Insert | ExtendedPropertyShowOnEnum.Update | ExtendedPropertyShowOnEnum.List;
        public required InputTypeEnum InputType { get; set; }= InputTypeEnum.Text;

        public virtual Module? Module { get; set; }
        public virtual ICollection<ExtendedPropertyValues> Values { get; set; }
        public virtual ICollection<ExtendedPropertyRecordValues> RecordValues { get; set; }
    }
}
