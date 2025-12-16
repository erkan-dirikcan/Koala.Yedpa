using Koala.Yedpa.Core.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Koala.Yedpa.Core.Models
{
    public class AppRole:IdentityRole<string>
    {
        public string? Description { get; set; }
        public string? DisplayName { get; set; }
        public StatusEnum StatusEnum { get; set; }
    }
}
