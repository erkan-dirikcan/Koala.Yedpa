using Koala.Yedpa.Core.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Koala.Yedpa.WebUI.TagHelpers
{

    public class UserSelectTagHelper:TagHelper
    {
        private readonly IAppUserService _userService;

        public UserSelectTagHelper(IAppUserService userService)
        {
            _userService = userService;
        }

        public string SelectedId { get; set; }
        //public override void Process(TagHelperContext context, TagHelperOutput output)
        //{
        //    output.TagName = "select";
        //    output.AddClass("form-control", HtmlEncoder.Default);
        //    var sb = new StringBuilder();
        //    var selected = "selected=\"selected\"";
        //    var users = _userService.GetUserActiveList().Result;
        //    if (!users.IsSuccess)
        //    {
        //        output.Content.SetHtmlContent(sb.ToString());
        //        return;
        //    }
        //    foreach (var item in users.Data)
        //    {
        //        sb.Append($"<option value=\"{item.Id}\" {(string.Equals(item.Id, SelectedId, StringComparison.OrdinalIgnoreCase) ? selected : string.Empty)}>{item.FullName}</option>");
        //    }
        //    output.Content.SetHtmlContent(sb.ToString());
        //}
    }
}
