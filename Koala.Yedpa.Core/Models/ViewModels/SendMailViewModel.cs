using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models.ViewModels
{
    public class SendMailViewModel
    {
        public string Subject { get; set; }
        public string DestinationMail { get; set; }
        public string DestinationName { get; set; }
        public string PlainTextContent { get; set; }
        public string HtmlContent { get; set; }
        public List<string>? Attachement { get; set; }


    }



}
