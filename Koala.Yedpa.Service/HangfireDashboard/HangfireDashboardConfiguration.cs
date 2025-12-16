// Koala.Yedpa.Service/HangfireDashboard/HangfireDashboardConfiguration.cs
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;

namespace Koala.Yedpa.Service.HangfireDashboard
{
    public static class HangfireDashboardConfiguration
    {
        public static IApplicationBuilder UseKoalaHangfireDashboard(this IApplicationBuilder app)
        {
            var dashboardOptions = new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangfireAuthorizationFilter("Hangfire.Access") // burada istediğin claim’i yaz
                },
                DashboardTitle = "Logo Sync – Yedpa Koala",
                // İstersen sadece okuma izni varsa butonları gizle (CSS ile de yapılıyor ama gerekirse JS ekleriz)
            };

            app.UseHangfireDashboard("/hangfire", dashboardOptions);
            return app;
        }
    }
}