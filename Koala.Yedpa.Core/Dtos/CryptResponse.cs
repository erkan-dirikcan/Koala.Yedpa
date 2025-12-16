using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Koala.Yedpa.Core.Dtos
{
    public class DecryptResponse
    {
        public bool isValid { get; set; }
        public string customerCode { get; set; } = "";
        public string errorMessage { get; set; } = "";
    }

    public class CryptoApiResponse
    {
        [JsonPropertyName("encryptedText")]
        public string EncryptedText { get; set; } = string.Empty;

        [JsonPropertyName("plainText")]
        public string PlainText { get; set; } = string.Empty;

        // Decrypt için ekstra alanlar (gerekirse)
        public bool? isValid { get; set; }
        public string customerCode { get; set; } = "";
        public string errorMessage { get; set; } = "";
    }
    //public class CryptoResponse
    //{
    //    public string cipherText { get; set; } = "";
    //}

}
