namespace Koala.Yedpa.Core.Helpers
{
    /// <summary>
    /// AIDAT sipariş satırlarının ay eşleşmesi Logo'da ORL.LINEEXP (büyük harf, ASCII)
    /// üzerinden yapılır — LINENO_ takvim ayıyla güvenilir değildir (canlı veride doğrulandı).
    /// </summary>
    public static class BulkInvoiceMonths
    {
        private static readonly string[] Names =
            { "OCAK", "SUBAT", "MART", "NISAN", "MAYIS", "HAZIRAN",
              "TEMMUZ", "AGUSTOS", "EYLUL", "EKIM", "KASIM", "ARALIK" };

        /// <summary>1..12 ay numarasını Logo LINEEXP ay adına çevirir (1=OCAK .. 12=ARALIK).</summary>
        public static string ToLogoName(int month) => Names[month - 1];
    }
}
