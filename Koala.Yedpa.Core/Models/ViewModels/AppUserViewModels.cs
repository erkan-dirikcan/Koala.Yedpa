using Koala.Yedpa.Core.Dtos;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Koala.Yedpa.Core.Models.ViewModels
{
    public class UpdateUserStatusViewModel
    {
        public string? Id { get; set; }
        public StatusEnum Status { get; set; }
    }
    public class UpdateUserStatusByEmailViewModel
    {
        [Required(ErrorMessage = "E-Posta Alanı Boş Bırakılamaz")]
        [Display(Name = "E-Posta")]
        [EmailAddress]
        public string? Email { get; set; }
        public StatusEnum Status { get; set; }
    }
   
    public class UpdateUserStatusByUserNameViewModel
    {
        [Required(ErrorMessage = "E-Posta Alanı Boş Bırakılamaz")]
        [Display(Name = "E-Posta")]
        [EmailAddress]
        public string? UserName { get; set; }
        public StatusEnum Status { get; set; }
    }
    public class AppUserInfoViewModels
    {
        public string? Id { get; set; }

        [Display(Name = "E-Posta")]
        [EmailAddress]
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
        [Display(Name = "E-Posta")]
        [EmailAddress]
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{FirstName} {MiddleName} {LastName}";
        public string? PhoneNumber { get; set; }
        public List<AppRoleListViewModel> Roles { get; private set; }
    }
    public class CreateAppUserViewModel
    {
        public IFormFile? Avatar { get; set; }
        [Required(ErrorMessage = "E-Posta Alanı Boş Bırakılamaz")]
        [Display(Name = "E-Posta")]
        [EmailAddress]
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
        public string? Role { get; private set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(MiddleName) && string.IsNullOrEmpty(LastName))
            {
                return $"{Email}";
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
    public class UpdateAppUserViewModel
    {
        public string? Id { get; set; }
        public IFormFile? Avatar { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Removed { get; set; }
    }
    public class RemoveLockout
    {
        public string? Id { get; set; }
    }
   
    public class ForgetPasswordViewModel
    {
        [Required(ErrorMessage = "E-Posta Alanı Boş Bırakılamaz")]
        [Display(Name = "E-Posta")]
        [EmailAddress]
        public string? Email { get; set; }
    }
    public class ResetPasswordViewModel
    {
        [email: EmailAddress]
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? Password { get; set; }
    }
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-Posta Alanı Boş Bırakılamaz")]
        [Display(Name = "E-Posta")]
        [EmailAddress]
        public string? Email { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "şifre Alanı Boş Bırakılamaz")]
        public string? Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
    public class ChangePasswordViewModel
    {
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
    public class UserProfileViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
        [email: EmailAddress]
        public string? Email { get; set; }

        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public List<AppRoleListViewModel> Roles { get; private set; }
    }
    public class UserProfileUpdateViewModel
    {
        public string? Id { get; set; }
        public IFormFile? Avatar { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        [email: EmailAddress]
        public string? Email { get; set; }
        public string Removed { get; set; }
    }
    public class UserListViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
        [email: EmailAddress]
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => GetFullName();
        public string? PhoneNumber { get; set; }
        public StatusEnum? Status { get; set; }
        public List<AppRoleListViewModel> Roles { get; private set; }

        public override string ToString()
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

        private string? GetFullName()
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

