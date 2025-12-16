using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models.ViewModels
{
    public class ExtendedPropertiesListViewModel
    {
        public string Id { get; set; }
        public string ModuleId { get; set; }
        public string ModeleName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ExtendedPropertyShowOnEnum ShowOn { get; set; }
        public InputTypeEnum InputType { get; set; }
    }
    public class ExtendedPropertiesDetailViewModel : ExtendedPropertiesListViewModel
    {
        public List<ExtendedPropertyValuesListViewModel> Values { get; set; } = new List<ExtendedPropertyValuesListViewModel>();
        public List<ExtendedPropertyRecordValuesListViewModel> RecordValues { get; set; } = new List<ExtendedPropertyRecordValuesListViewModel>();
    }

    public class ExtendedPropertiesGetValueViewModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
    }

    public class CreateExtendedPropertiesViewModel
    {
        public string? ModuleId { get; set; }
        public required string? Name { get; set; }
        public required string? DisplayName { get; set; }
        public required string? Description { get; set; }
        public required ExtendedPropertyShowOnEnum ShowOn { get; set; } = ExtendedPropertyShowOnEnum.Detail | ExtendedPropertyShowOnEnum.Insert | ExtendedPropertyShowOnEnum.Update | ExtendedPropertyShowOnEnum.List;
        public required InputTypeEnum InputType { get; set; } = InputTypeEnum.Text;

    }

    public class UpdateExtendedPropertiesViewModel : CreateExtendedPropertiesViewModel
    {
        public string Id { get; set; }
    }

    public class ExtendedPropertyValuesViewModel
    {
        public ExtendedPropertyValuesViewModel()
        {
            ValuesList = new List<ExtendedPropertyValuesListViewModel>();
        }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public required InputTypeEnum InputType { get; set; } = InputTypeEnum.Text;
        public string ModuleName { get; set; }
        public List<ExtendedPropertyValuesListViewModel> ValuesList { get; set; }
    }
}






