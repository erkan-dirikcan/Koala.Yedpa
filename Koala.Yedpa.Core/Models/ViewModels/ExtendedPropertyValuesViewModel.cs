namespace Koala.Yedpa.Core.Models.ViewModels;

public class ExtendedPropertyValuesDetailViewModel
{
    public string Id { get; set; }
    public string ExtendedPropertyId { get; set; }
    public string ExtendedPropertyName { get; set; }
    public string DisplayText { get; set; }
    public string Value { get; set; }
}
public class CreateExtendedPropertyValuesViewModel
{
    public string ExtendedPropertyId { get; set; }
    public string DisplayText { get; set; }
    public string Value { get; set; }
}
public class UpdateExtendedPropertyValuesViewModel : CreateExtendedPropertyValuesViewModel
{
    public string Id { get; set; }
}
public class ExtendedPropertyValuesListViewModel
{
    public string Id { get; set; }
    public string ExtendedPropertyId { get; set; }
    public string ExtendedPropertyName { get; set; }
    public string DisplayText { get; set; }
    public string Value { get; set; }
}