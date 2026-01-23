using Microsoft.AspNetCore.Mvc.Rendering;

namespace Koala.Yedpa.WebUI
{
    public class ManageNavPages
    {
       
        private static string Dashboard => "Dashboard";
        private static string UserList => "UserList";
        private static string CreateUser => "CreateUser";
        private static string RoleList => "RoleList";
        private static string CreateRole => "CreateRole";
        private static string ModuleList => "ModuleList";
        private static string CreateModule => "CreateModule";
        private static string UpdateModule => "UpdateModule";
        private static string SmtpSettings => "SmtpSettings";
        private static string LogoUserSettings => "LogoUserSettings";
        private static string LogoRestServiceSettings => "LogoRestServiceSettings";
        private static string LogoSqlSettings => "LogoSqlSettings";



        private static string BudgetOrderList => "BudgetOrderList";
        private static string WorkplaceList => "WorkplaceList";



        //###############################################################################################################


        public static string DashboardNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, Dashboard);
        public static string UserListNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, UserList);
        public static string RoleListNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, RoleList);
        public static string CreateRoleNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, CreateRole);
        public static string CreateUserNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, CreateUser);
        public static string ModuleListNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, ModuleList);
        public static string CreateModuleNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, CreateModule);
        public static string UpdateModuleNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, UpdateModule);
        public static string SmtpSettingsNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, SmtpSettings);
        public static string LogoUserSettingsNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, LogoUserSettings);
        public static string LogoSqlSettingsNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, LogoSqlSettings);
        public static string LogoRestServiceSettingsNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, LogoRestServiceSettings);




        public static string BudgetOrderNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, BudgetOrderList);
        public static string WorkplaceNavClass(ViewContext viewContext) => PageMainNavClass(viewContext, WorkplaceList);



        //===============================================================================================================

        private static string User => "User";
        private static string Module => "Module";
        private static string Settings => "Settings";
        private static string Site => "Site";



        //###############################################################################################################

        public static string UserNavClass(ViewContext viewContext) => PageMainToogleNavClass(viewContext, User);
        public static string ModuleNavClass(ViewContext viewContext) => PageMainToogleNavClass(viewContext, Module);
        public static string SettingsNavClass(ViewContext viewContext) => PageMainToogleNavClass(viewContext, Settings);
        public static string SiteNavClass(ViewContext viewContext) => PageMainToogleNavClass(viewContext, Site);


        private static string PageMainNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                             ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            var retVal= string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "menu-item-active" : null;
            return retVal;
        }
        private static string PageMainToogleNavClass(ViewContext viewContext, string page)
        {
            var menuToogle = viewContext.ViewData["MenuToggle"] as string
                             ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            var retVal=string.Equals(menuToogle, page, StringComparison.OrdinalIgnoreCase) ? "menu-item-open" : "";
            return retVal;
        }
        //private static string PageUserProfileNavClass(ViewContext viewContext, string page)
        //{
        //    var activePage = viewContext.ViewData["ActivePage"] as string
        //                     ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        //    return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : "";
        //}
    }

}
