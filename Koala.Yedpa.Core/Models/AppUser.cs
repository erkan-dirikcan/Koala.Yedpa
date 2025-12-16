using Koala.Yedpa.Core.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Koala.Yedpa.Core.Models
{
    public class AppUser : IdentityUser<string>
    {
        public AppUser()
        {

        }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public StatusEnum? Status { get; set; }= StatusEnum.Active;
        public string? Avatar { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }

        public override string? ToString()
        {
            if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(MiddleName) && string.IsNullOrEmpty(LastName))
            {
                return $"{UserName}";
            }
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(MiddleName) && !string.IsNullOrEmpty(LastName))
            {
                return $"{FirstName} {MiddleName} {LastName}";
            }
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(MiddleName))
            {
                return $"{FirstName} {MiddleName}";
            }
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            {
                return $"{FirstName} {LastName}";
            }
            if (!string.IsNullOrEmpty(MiddleName) && !string.IsNullOrEmpty(LastName))
            {
                return $"{MiddleName} {LastName}";
            }
            return !string.IsNullOrEmpty(FirstName) ? $"{FirstName}" : Email;
        }
    }
}
