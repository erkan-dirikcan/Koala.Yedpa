namespace Koala.Yedpa.Core.Dtos
{
    public class ResetPasswordEmailDto
    {
        public ResetPasswordEmailDto()
        {

        }
        public ResetPasswordEmailDto(string? name, string? lastname, string email, string resetLink)
        {
            Name = name;
            Lastname = lastname;
            Email = email;
            ResetLink = resetLink;
        }

        public string? Name { get; set; }
        public string? Lastname { get; set; }
        public string Email { get; set; }
        public string ResetLink { get; set; }
    }
    public class CustomEmailDto:EmailDto
    {
        public CustomEmailDto()
        {

        }
        public CustomEmailDto(string? name, string? lastname, string email)
        {
            Name = name;
            Lastname = lastname;
            //Email = email;
        }
        
        public string? Name { get; set; }
        public string? Lastname { get; set; }
        public string Email { get; set; }
    }
    public class EmailDto
    {
        public EmailDto()
        {

        }
        public EmailDto(string? email, string content, string title)
        {
            Email = email;
            Content = content;
            Title = title;
        }

        public string Email { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
    }
}
