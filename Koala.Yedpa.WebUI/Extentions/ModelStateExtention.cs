using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Koala.Yedpa.WebUI.Extentions
{
    public static class ModelStateExtention
    {
        public static void AddModelErrorList(this ModelStateDictionary modelState, List<string> errors)
        {
            foreach (var item in errors)
            {
                modelState.AddModelError(string.Empty, item);

            }
        }
    }
}
