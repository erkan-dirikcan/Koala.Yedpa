using Koala.Yedpa.Core.Helpers;

namespace Koala.Yedpa.Core.Models;

public class ExtendedPropertyRecordValues
{
    public string Id { get; set; } = Tools.CreateGuidStr();
    public string? ExtendedPropertyId { get; set; }
    public string? Value { get; set; }
    public string? RecordId { get; set; }
    public virtual ExtendedProperties? ExtendedProperty { get; set; }
}