namespace Koala.Yedpa.Core.Models.ViewModels;

public class ExtendedPropertyRecordValuesListViewModel
{
    public string Id { get; set; }
    public string? RecordId { get; set; }
    public string? ExtendedPropertyId { get; set; }
    public string? ExtendedPropertyName { get; set; }
    public string? Value { get; set; }
}
public class CreateExtendedPropertyRecordValuesViewModel
{
    public string? RecordId { get; set; }
    public string? ExtendedPropertyId { get; set; }
    public string? ExtendedPropertyName { get; set; }
    public string? Value { get; set; }
}
public class UpdateExtendedPropertyRecordValuesViewModel : CreateExtendedPropertyRecordValuesViewModel
{
    public string Id { get; set; }
}