namespace Koala.Yedpa.Core.Dtos
{
    public class SmtpMailSettingsDto
    {
        public required string MailServer { get; set; }
        public required int Port { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required bool EnableSsl { get; set; } = false;

    }
}
