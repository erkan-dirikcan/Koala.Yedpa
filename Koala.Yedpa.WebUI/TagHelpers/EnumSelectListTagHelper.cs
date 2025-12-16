using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;
using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.WebUI.TagHelpers
{
    public class PriorityEnumSelectTagHelper : TagHelper
    {
        public PriorityEnum SelectedValue { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            output.AddClass("form-control", HtmlEncoder.Default);
            var sb = new StringBuilder();
            var selected = "selected=\"selected\"";
            sb.Append($"<option value=\"1\" {(SelectedValue == PriorityEnum.Lowest ? selected : string.Empty)}>Çok Düşük</option>");
            sb.Append($"<option value=\"2\" {(SelectedValue == PriorityEnum.Low ? selected : string.Empty)}>Düşük</option>");
            sb.Append($"<option value=\"3\" {(SelectedValue == PriorityEnum.Normal ? selected : string.Empty)}>Normal</option>");
            sb.Append($"<option value=\"4\" {(SelectedValue == PriorityEnum.High ? selected : string.Empty)}>Yüksek</option>");
            sb.Append($"<option value=\"5\" {(SelectedValue == PriorityEnum.Highest ? selected : string.Empty)}>Çok Yüksek</option>");
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
/*
  public class CrmCategorySelectTagHelper : TagHelper
    {
        private readonly ICrmCategoryService _categoryService;
        public string SelectedOid { get; set; } = string.Empty;

        public CrmCategorySelectTagHelper(ICrmCategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            output.AddClass("form-control", HtmlEncoder.Default);

            var categories = _categoryService.Where(x => x.IsActive == true && x.GCRecord == null);
            if (!categories.IsSuccess)
            {
                output.Content.SetHtmlContent(string.Empty);
            }
            else
            {
                var sb = new StringBuilder();
                foreach(var item in categories.Data)
                {
                    if (SelectedOid.ToUpper()== item.Oid.ToUpper())
                    {
                         sb.Append($"<option value=\"{item.Oid}\" selected=\"selected\">{item.ActivityCategoryDescription} </option>");
                    }
                    else
                    {
                         sb.Append($"<option value=\"{item.Oid}\" selected=\"selected\">{item.ActivityCategoryDescription} </option>");

                    }
                }
                output.Content.SetHtmlContent(sb.ToString());
            }
        }

    }
}
 */