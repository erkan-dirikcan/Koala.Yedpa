using System.ComponentModel.DataAnnotations;
using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models.ViewModels
{
    public class AddSettingViewModel
    {
        public AddSettingViewModel()
        {
        }

        public AddSettingViewModel(string name, string desscription, string? value, SettingValueTypeEnum settingValueType, SettingsTypeEnum settingType)
        {
            Name = name;
            Desscription = desscription;
            Value = value;
            SettingValueType = settingValueType;
            SettingType = settingType;
        }
        [Required(ErrorMessage = "Ad Alanı Zorunludur")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Açıklama Zorunludur")]

        public string Desscription { get; set; }
        [Required(ErrorMessage = "Değer Zorunludur")]
        public string? Value { get; set; }
        [Required(ErrorMessage = "Değişken Tipi Alanı Zorunludur")]
        public SettingValueTypeEnum SettingValueType { get; set; }
        [Required(ErrorMessage = "Ayar Tipi Alanı Zorunludur")]
        public SettingsTypeEnum SettingType { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
    
    
    public class EmailSettingViewModel
    {
        public EmailSettingViewModel()
        {
        }
        public EmailSettingViewModel(int port, string senderEmail, string senderName, bool enableSsl, string smtpServer, string userName)
        {
            Port = port;
            SenderEmail = senderEmail;
            SenderName = senderName;
            EnableSsl = enableSsl;
            SmtpServer = smtpServer;
            UserName = userName;
        }
        [Required(ErrorMessage = "SMTP Sunucu Alanı Zorunludur")]
        [Display(Name = "SMTP Sunucusu")]
        public string SmtpServer { get; set; }//
        [Required(ErrorMessage = "Kullanıcı Adı Alanı Zorunludur")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }//
        [Required(ErrorMessage = "Parola Alanı Zorunludur")]
        [Display(Name = "Parola")]
        public string Password { get; set; }//
        [Required(ErrorMessage = "Port Alanı Zorunludur")]
        [Display(Name = "Port")]
        public int Port { get; set; }//
        [Required(ErrorMessage = "Gönderici Email Alanı Zorunludur")]
        [Display(Name = "Gönderici Email Adresi")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string SenderEmail { get; set; }
        [Display(Name = "Gönderen Adı")]
        public string SenderName { get; set; }
        [Display(Name = "SSL Etkinleştir")]
        public bool EnableSsl { get; set; }
    }
    
    public class LogoRestServiceSettingViewModel
    {
        public LogoRestServiceSettingViewModel()
        {
        }
        public LogoRestServiceSettingViewModel(string server, int port, string userName, string password, string firm, string perriod)
        {
            Server = server;
            Port = port;
            UserName = userName;
            Password = password;
            Firm = firm;
            Period = perriod;
        }

        public string Server { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Firm { get; set; }
        public string Period { get; set; }
    }

    public class LogoSqlSettingViewModel
    {
        public LogoSqlSettingViewModel()
        {
            
        }
        public LogoSqlSettingViewModel(string server, string userName, string password, string database)
        {
            Server = server;
            UserName = userName;
            Password = password;
            Database = database;
        }
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
    }

    public class LogoSettingViewModel
    {
        public LogoSettingViewModel()
        {
            
        }
        public LogoSettingViewModel(string userName, string password, string firm, string perriod)
        {
            UserName = userName;
            Password = password;
            Firm = firm;
            Period = perriod;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Firm { get; set; }
        public string Period { get; set; }
    }
}
