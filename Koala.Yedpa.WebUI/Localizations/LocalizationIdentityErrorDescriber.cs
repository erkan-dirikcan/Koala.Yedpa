using Microsoft.AspNetCore.Identity;

namespace Koala.Yedpa.WebUI.Localizations
{
    public class LocalizationIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = "DuplicateEmail",
                Description = "Bu E-Posta Adresi Zaten Kullanılıyor"
            }; //base.DuplicateEmail(email);
        }

        public override IdentityError InvalidToken()
        {
            return new IdentityError
            {
                Code = "Hatalı Token",
                Description = "Süresi geçmiş veya daha önce kullanılmış bir token kullandınız."
            };
        }
        public override IdentityError PasswordMismatch()
        {
            return new IdentityError
            {
                Code = "Hatalı Şifre",
                Description = "Mevcut şifrenizi kontrol ederek tekrar deneyiniz"
            };
        }
        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = "Kullanıcı Adı Mevcut",
                Description = "Bu issimle daha önce kaydedilmiş bir kullanıcı bulunmaktadır."
            };
        }
        public override IdentityError UserNotInRole(string role)
        {
            return new IdentityError
            {
                Code = "Yetkiniz Bulunmuyor",
                Description = "Bu alana erişim sağlamanız için gerekli yetkilere sahip değilsiniz."
            };
        }
        public override IdentityError DuplicateRoleName(string role)
        {
            return new IdentityError
            {
                Code = "Kayıtlı Rol Adı",
                Description = "Bu rol ismi daha önce kaydedilmiş."
            };
        }

        public override IdentityError InvalidEmail(string? email)
        {
            return new IdentityError
            {
                Code = "Geçersiz E-Posta Adresi",
                Description = "Geçerli bir e-posta adresi giriniz."
            };
        }
        public override IdentityError InvalidRoleName(string? role)
        {
            return new IdentityError
            {
                Code = "Geçersiz Rol Adı",
                Description = "Geçerli bir rol adı giriniz."
            };
        }
        public override IdentityError InvalidUserName(string? userName)
        {
            return new IdentityError
            {
                Code = "Geçersiz Kullanıcı Adı",
                Description = "Geçerli bir kullanıcı adı giriniz."
            };
        }
        public override IdentityError UserAlreadyHasPassword()
        {
            return new IdentityError
            {
                Code = "Kullanıcı Zaten Şifreli",
                Description = "Bu kullanıcı için zaten bir şifre mevcut."
            };
        }
        public override IdentityError UserAlreadyInRole(string role)
        {
            return new IdentityError
            {
                Code = "Kullanıcı Zaten Rolde",
                Description = "Bu kullanıcı zaten bu rolde."
            };
        }
        public override IdentityError UserLockoutNotEnabled()
        {
            return new IdentityError
            {
                Code = "Kullanıcı Kilidi Yok",
                Description = "Bu kullanıcı için kilit açma işlemi yok."
            };
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            var error = new IdentityError
            {
                Code = "Kullanıcı Zaten Kayıtlı",
                Description = "Bu e-posta adresi ile daha önce kayıt yapılmış."
            };
            return error;

        }
        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = "Şifre Alfanumerik Olmalı",
                Description = "Şifreniz en az bir alfanumerik karakter içermelidir."
            };
        }
        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = "Şifre Rakam İçermeli",
                Description = "Şifreniz en az bir rakam içermelidir."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = "Şifre Küçük Harf İçermeli",
                Description = "Şifreniz en az bir küçük harf içermelidir."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = "Şifre Büyük Harf İçermeli",
                Description = "Şifreniz en az bir büyük harf içermelidir."
            };
        }

        public override IdentityError RecoveryCodeRedemptionFailed()
        {
            return new IdentityError
            {
                Code = "Hatalı Kod",
                Description = "Bu kod geçersiz veya süresi dolmuş."
            };
        }

        public override IdentityError DefaultError()
        {
            return new IdentityError
            {
                Code = "Hata",
                Description = "Bir hata oluştu. Lütfen tekrar deneyiniz."
            };
        }

        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError
            {
                Code = "Çoklu Giriş",
                Description = "Bu kayıt, başka bir kullanıcı tarafından değiştirilmiş."
            };
        }

        

    }
}
