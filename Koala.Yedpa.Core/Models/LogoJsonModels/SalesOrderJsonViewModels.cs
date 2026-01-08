namespace Koala.Yedpa.Core.Models.LogoJsonModels
{
    /// <summary>
    /// Logo SalesOrder Transaction Item
    /// </summary>
    public class SalesOrderTransactionItem
    {
        public string TYPE { get; set; } = "4"; // 4 = Alış Fatura / Satış siparişi
        public string MASTER_CODE { get; set; } = "600.11.0001"; // Hesap kodu
        public string QUANTITY { get; set; } = "1";
        public string PRICE { get; set; }
        public string VAT_RATE { get; set; } = "20"; // KDV oranı
        public string TRANS_DESCRIPTION { get; set; } // Ay adı (OCAK, ŞUBAT vb.)
        public string UNIT_CODE { get; set; } = "ADET";
        public string UNIT_CONV1 { get; set; } = "1";
        public string UNIT_CONV2 { get; set; } = "2";
        public string VAT_INCLUDED { get; set; } = "1"; // KDV dahil
        public string ORDER_CLOSED { get; set; } = "0";
        public DateTime DUE_DATE { get; set; }
        public string DEFNFLDS { get; set; } = "";
        public string MULTI_ADD_TAX { get; set; } = "0";
        public string EDT_CURR { get; set; } = "1";
    }

    /// <summary>
    /// Logo SalesOrder Transactions
    /// </summary>
    public class SalesOrderTransactions
    {
        public List<SalesOrderTransactionItem> Items { get; set; } = new List<SalesOrderTransactionItem>();
    }

    /// <summary>
    /// Logo SalesOrder JSON Model
    /// POST /api/v1/salesOrders
    /// </summary>
    public class SalesOrderJsonViewModel
    {
        public string NUMBER { get; set; } = "~"; // Otomatik numara
        public string DOC_TRACK_NR { get; set; } // Belge takip numarası
        public DateTime DATE { get; set; } // Belge tarihi
        public string DOC_NUMBER { get; set; } = "AIDAT"; // Belge numarası
        public string AUTH_CODE { get; set; } = "AIDAT"; // Yetki kodu
        public string ARP_CODE { get; set; } // Cari kodu (DukkanNo)
        public string NOTES1 { get; set; } // Not 1
        public string NOTES2 { get; set; } // Not 2
        public string ORDER_STATUS { get; set; } = "4"; // Sipariş durumu
        public DateTime DATE_CREATED { get; set; } = DateTime.Now; // Oluşturma tarihi
        public string CURRSEL_TOTAL { get; set; } = "1"; // Para birimi
        public SalesOrderTransactions TRANSACTIONS { get; set; } = new SalesOrderTransactions();
        public string DOC_TRACKING_NR { get; set; } // Doküman takip numarası
        public string ORGLOGOID { get; set; } = ""; // Orijinal Logo ID
        public string DEFNFLDSLIST { get; set; } = ""; // Özel alanlar
        public string AFFECT_RISK { get; set; } = "0"; // Risk etkilemesi
        public string EINVOICE { get; set; } = "1"; // E-Fatura
        public string EINSTEAD_OF_DISPATCH { get; set; } = "1"; // E-İrsaliye yerine
        public string LABEL_LIST { get; set; } = ""; // Etiket listesi
    }
}
