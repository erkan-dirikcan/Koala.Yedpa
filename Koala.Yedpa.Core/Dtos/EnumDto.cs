namespace Koala.Yedpa.Core.Dtos
{
    public enum StatusEnum
    {
        Active = 0x01,
        Passive = 0x02,
        Deleted = 0x03,
        Locked = 0x04,
        Unlocked = 0x05,
        Pending = 0x06
    }
    public enum PriorityEnum
    {
        Lowest = 0x01,
        Low = 0x02,
        Normal = 0x03,
        High = 0x04,
        Highest = 0x05
    }

    [Flags]
    public enum BuggetRatioMounthEnum
    {
        None=0x0000,
        January = 0x0001,
        February = 0x0002,
        March = 0x0004,
        April = 0x0008,
        May = 0x0010,
        June = 0x0020,
        July = 0x0040,
        August = 0x0080,
        September = 0x0100,
        October = 0x0200,
        November = 0x0400,
        December = 0x0800
    }
    public enum TransferStatusEnum
    {
        Pending = 0x01,
        Completed = 0x02,
        Failed = 0x03,
        Canceled = 0x04,
        FromLogo= 0x10,
    }
    public enum InputTypeEnum
    {
        Text = 0x01,
        TextArea = 0x02,
        Select = 0x03,
        Radio = 0x04,
        CheckBox = 0x05,
        DateTime = 0x06,
        File = 0x07,
        Image = 0x08
    }

    [Flags]
    public enum CompanyTypeEnum
    {
        Seller = 0x01,
        Buyer = 0x02
    }
    public enum CompanyFormEnum
    {
        Group = 0x01,//Grup Şirketi/ Holding
        Company = 0x02,//Şirket
        Branch = 0x03,//Şube
        SoleProprietorship = 0x04,//Şahıs Şirketi
        Other = 0x05//Diğer
    }
    public enum SettingsTypeEnum
    {
        LogoSqlSettings = 0x01,
        EmailSettings = 0x02,
        SmsSettings = 0x03,
        PushNotificationSettings = 0x04,
        ApplicationSettings = 0x05,
        LogoRestServiceSettings = 0x06,
        LogoUserSettings = 0x07,
        HangfireSettings = 0x08
    }

    public enum SettingValueTypeEnum
    {
        String = 0x01,
        Int = 0x02,
        Bool = 0x03,
        Decimal = 0x04,
        DateTime = 0x05,
        Json = 0x06
    }
    public enum TicketPriorityEnum
    {
        Low = 0x01,
        Medium = 0x02,
        High = 0x03,
        Urgent = 0x04
    }

    public enum TicketContentTypeEnum
    {
        Answer = 0x01,
        Question = 0x02
    }
    [Flags]
    public enum ExtendedPropertyShowOnEnum
    {
        Insert = 0x01, //Insert Formunda Göster
        Update = 0x02, //Update Formunda Göster
        List = 0x04, //Listede Göster
        Detail = 0x08, //Detayda Göster
    }

    public enum BuggetTypeEnum
    {
        Budget=0x01,
        ExtraBudget= 0x02
    }

}
