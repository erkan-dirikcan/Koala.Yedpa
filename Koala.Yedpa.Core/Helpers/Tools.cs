using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;

namespace Koala.Yedpa.Core.Helpers
{
    public static class Tools
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        static extern int UuidCreateSequential(out Guid guid);
        public static string CreateGuidStr()
        {
            const int RPC_S_OK = 0;
            Guid g;

            return UuidCreateSequential(out g) != RPC_S_OK ? Guid.NewGuid().ToString() : g.ToString();
        }
        public static Guid CreateGuid()
        {
            const int RPC_S_OK = 0;
            Guid g;

            return UuidCreateSequential(out g) != RPC_S_OK ? Guid.NewGuid() : g;
        }
        public static string MessageReplace(this string metin)
        {
            metin = metin.Replace("Ç", "C");

            metin = metin.Replace("Ğ", "G");

            metin = metin.Replace("İ", "i");

            metin = metin.Replace("Ş", "S");

            metin = metin.Replace("Ö", "O");

            metin = metin.Replace("Ü", "U");

            metin = metin.Replace("ç", "c");

            metin = metin.Replace("ğ", "g");

            metin = metin.Replace("ı", "i");

            metin = metin.Replace("ş", "s");

            metin = metin.Replace("ö", "o");

            metin = metin.Replace("ü", "u");

            return metin;
        }
        public static bool TcCheck(this string identityNumber)
        {
            var returnvalue = false;
            if (identityNumber.Length == 11)
            {
                Int64 ATCNO, BTCNO, TcNo;
                long C1, C2, C3, C4, C5, C6, C7, C8, C9, Q1, Q2;

                TcNo = Int64.Parse(identityNumber);

                ATCNO = TcNo / 100;
                BTCNO = TcNo / 100;

                C1 = ATCNO % 10; ATCNO = ATCNO / 10;
                C2 = ATCNO % 10; ATCNO = ATCNO / 10;
                C3 = ATCNO % 10; ATCNO = ATCNO / 10;
                C4 = ATCNO % 10; ATCNO = ATCNO / 10;
                C5 = ATCNO % 10; ATCNO = ATCNO / 10;
                C6 = ATCNO % 10; ATCNO = ATCNO / 10;
                C7 = ATCNO % 10; ATCNO = ATCNO / 10;
                C8 = ATCNO % 10; ATCNO = ATCNO / 10;
                C9 = ATCNO % 10; ATCNO = ATCNO / 10;
                Q1 = ((10 - ((((C1 + C3 + C5 + C7 + C9) * 3) + (C2 + C4 + C6 + C8)) % 10)) % 10);
                Q2 = ((10 - (((((C2 + C4 + C6 + C8) + Q1) * 3) + (C1 + C3 + C5 + C7 + C9)) % 10)) % 10);

                returnvalue = ((BTCNO * 100) + (Q1 * 10) + Q2 == TcNo);
            }
            return returnvalue;
        }
        public static decimal DecimalParse(this decimal value, byte doubleLenght)
        {
            var sb = new StringBuilder("1");
            for (var i = 0; i < doubleLenght; i++)
            {
                sb.Append("0");
            }
            var flour = int.Parse(sb.ToString());
            return Math.Floor(flour * value) / flour;
        }
        public static string CreateConnectionString(string server, string dataBase, string userName, string password)
        {
            return "Data Source=" + server + ";Initial Catalog=" + dataBase + ";User ID=" + userName + ";Password=" + password + ";Encrypt=False; TrustServerCertificate=True;";
            //return "Data Source=" + server + ";Initial Catalog=" + dataBase + ";User ID=" + userName + ";Password=" + password;
        }
        public static string SqlQueryCreator(Dictionary<string, string> parametters, string query)
        {
            return parametters.Aggregate(query, (current, item) => current.Replace(item.Key, item.Value));

        }
        public static bool IsValidEmail(string email)
        {
            var valid = true;

            try
            {
                var emailAddress = new MailAddress(email);
            }
            catch
            {
                valid = false;
            }

            return valid;
        }
        public static int ConvertToLogoTime(this DateTime date)
        {
            return 0 + 256 * 2 + 65536 * date.Minute + 16777216 * date.Hour;
        }
        public static string ConvertToLogoTime(int minute, int hour)
        {
            return (0 + 256 * 2 + 65536 * minute + 16777216 * hour).ToString(); ;
        }
        public static bool InternetControl()
        {
            try
            {
                var clnt = new System.Net.Sockets.TcpClient("https://google.com", 80);
                clnt.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool ServerControl()
        {
            try
            {
                var clnt = new System.Net.Sockets.TcpClient("https://sistem-koala.com", 80);
                clnt.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
