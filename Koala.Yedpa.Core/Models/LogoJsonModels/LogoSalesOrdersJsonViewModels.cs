namespace Koala.Yedpa.Core.Models.LogoJsonModels
{
    public class Item
    {
        public string TYPE { get; set; }
        public string MASTER_CODE { get; set; }
        public string QUANTITY { get; set; }
        public string PRICE { get; set; }
        public string VAT_RATE { get; set; }
        public string TRANS_DESCRIPTION { get; set; }
        public string UNIT_CODE { get; set; }
        public string UNIT_CONV1 { get; set; }
        public string UNIT_CONV2 { get; set; }
        public string VAT_INCLUDED { get; set; }
        public string ORDER_CLOSED { get; set; }
        public DateTime DUE_DATE { get; set; }
        public string DEFNFLDS { get; set; }
        public string MULTI_ADD_TAX { get; set; }
        public string EDT_CURR { get; set; }
    }

    public class TRANSACTIONS
    {
        public IList<Item> items { get; set; }
    }

    public class SalesOrdersJsomViewModel
    {
        public string NUMBER { get; set; } = "~";
        public string DOC_TRACK_NR { get; set; }
        public DateTime DATE { get; set; }
        public string DOC_NUMBER { get; set; }
        public string AUTH_CODE { get; set; }
        public string ARP_CODE { get; set; }
        public string NOTES1 { get; set; }
        public string NOTES2 { get; set; }
        public string ORDER_STATUS { get; set; }
        public DateTime DATE_CREATED { get; set; }
        public string CURRSEL_TOTAL { get; set; }
        public TRANSACTIONS TRANSACTIONS { get; set; }
        public string DOC_TRACKING_NR { get; set; }
        public string ORGLOGOID { get; set; }
        public string DEFNFLDSLIST { get; set; }
        public string AFFECT_RISK { get; set; }
        public string EINVOICE { get; set; }
        public string EINSTEAD_OF_DISPATCH { get; set; }
        public string LABEL_LIST { get; set; }
    }
}
