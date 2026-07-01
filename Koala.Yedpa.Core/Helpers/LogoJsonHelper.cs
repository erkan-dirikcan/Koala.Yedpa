using Newtonsoft.Json.Linq;

namespace Koala.Yedpa.Core.Helpers
{
    public static class LogoJsonHelper
    {
        /// <summary>
        /// JSON string'e DataObjectParameter ekler
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>DataObjectParameter eklenmiş JSON string</returns>
        public static string InjectDataObjectParameter(string json)
        {
            // Boş veya null kontrolü
            if (string.IsNullOrWhiteSpace(json))
            {
                return json;
            }

            try
            {
                // JSON'ı parse et
                var jObject = JObject.Parse(json);

                // DataObjectParameter ekle
                jObject["DataObjectParameter"] = new JObject
                {
                    ["FillAccCodesOnPreSave"] = true
                };

                return jObject.ToString();
            }
            catch
            {
                // Parse hatası durumunda orijinal JSON'ı geri döndür
                return json;
            }
        }
    }
}