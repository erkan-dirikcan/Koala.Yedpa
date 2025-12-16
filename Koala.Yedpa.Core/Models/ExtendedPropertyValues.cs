using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models
{
    public class ExtendedPropertyValues : CommonProperties
    {
        public string Id { get; set; }=Tools.CreateGuidStr();
        public string? ExtendedPropertyId { get; set; }
        public required string DisplayText { get; set; }
        public required string Value { get; set; }
        public virtual ExtendedProperties? ExtendedProperty { get; set; }
    }
}
