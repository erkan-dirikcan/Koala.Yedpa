// Koala.Yedpa.Service/HangfireDashboard/HangfireAuthorizationFilter.cs
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace Koala.Yedpa.Service.HangfireDashboard
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _requiredClaim; // örnek: "Hangfire.Access"

        public HangfireAuthorizationFilter(string requiredClaim = "Hangfire.Access")
        {
            _requiredClaim = requiredClaim;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // 1. Kullanıcı giriş yapmış mı?
            
            if (!httpContext.User.Identity?.IsAuthenticated ?? false)
                return false;

            // 2. Gerekli claim var mı?
            if (!httpContext.User.HasClaim(c => c.Value == _requiredClaim))
                return false;

            // Ekstra: Sadece "Hangfire.Trigger" claim’i olanlar job tetikleyebilsin (isteğe bağlı)
            var canTrigger = httpContext.User.HasClaim(c => c.Value == "Hangfire.Trigger");
            httpContext.Items["CanTriggerJobs"] = canTrigger;

            return true;
        }
    }
}