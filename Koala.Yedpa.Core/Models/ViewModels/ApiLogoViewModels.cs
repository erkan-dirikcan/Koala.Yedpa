using Swashbuckle.AspNetCore.Annotations;

namespace Koala.Yedpa.Core.Models.ViewModels
{

    // using Swashbuckle.AspNetCore.Annotations;

    namespace YourNamespace.ViewModels
    {

        public class ClCardInfoViewModel
        {
            /// <summary>
            /// Ana cari kart kodu
            /// </summary>
            [SwaggerSchema(Description = "Ana cari kart kodu")]
            public string CARI_KODU { get; set; } = string.Empty;

            /// <summary>
            /// Cari unvanı / firma adı
            /// </summary>
            [SwaggerSchema(Description = "Cari firma adı / unvanı")]
            public string CARI_UNVAN { get; set; } = string.Empty;

            /// <summary>
            /// Vergi dairesi adı
            /// </summary>
            [SwaggerSchema(Description = "Vergi dairesi")]
            public string VERGI_DAIRESI { get; set; } = string.Empty;

            /// <summary>
            /// Vergi kimlik numarası
            /// </summary>
            [SwaggerSchema(Description = "Vergi numarası")]
            public string VERGI_NUMARASI { get; set; } = string.Empty;

            /// <summary>
            /// T.C. Kimlik numarası (şahıs firmaları için)
            /// </summary>
            [SwaggerSchema(Description = "T.C. Kimlik numarası")]
            public string TCKN { get; set; } = string.Empty;

            /// <summary>
            /// İl adı
            /// </summary>
            [SwaggerSchema(Description = "İl")]
            public string IL { get; set; } = string.Empty;

            /// <summary>
            /// İlçe adı
            /// </summary>
            [SwaggerSchema(Description = "İlçe")]
            public string ILCE { get; set; } = string.Empty;

            /// <summary>
            /// Mahalle / semt
            /// </summary>
            [SwaggerSchema(Description = "Mahalle")]
            public string MAHALLE { get; set; } = string.Empty;

            /// <summary>
            /// Adres satırı 1
            /// </summary>
            [SwaggerSchema(Description = "Adres satırı 1")]
            public string ADRES_1 { get; set; } = string.Empty;

            /// <summary>
            /// Adres satırı 2
            /// </summary>
            [SwaggerSchema(Description = "Adres satırı 2")]
            public string ADRES_2 { get; set; } = string.Empty;

            /// <summary>
            /// 3. e-posta adresi (genelde muhasebe)
            /// </summary>
            [SwaggerSchema(Description = "Ek e-posta adresi")]
            public string EMAIL_3 { get; set; } = string.Empty;

            /// <summary>
            /// Ana telefon numarası
            /// </summary>
            [SwaggerSchema(Description = "Telefon 1")]
            public string TELEFON_1 { get; set; } = string.Empty;

            /// <summary>
            /// Alternatif telefon numarası
            /// </summary>
            [SwaggerSchema(Description = "Telefon 2")]
            public string TELEFON_2 { get; set; } = string.Empty;

            /// <summary>
            /// Faks numarası
            /// </summary>
            [SwaggerSchema(Description = "Faks")]
            public string FAX { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 1
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 1")]
            public string OZEL_KOD { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 2
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 2")]
            public string OZEL_KOD2 { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 3
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 3")]
            public string OZEL_KOD3 { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 4
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 4")]
            public string OZEL_KOD4 { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 5
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 5")]
            public string OZEL_KOD5 { get; set; } = string.Empty;

            /// <summary>
            /// Dükkan cari kodu (.IS ile biten)
            /// </summary>
            [SwaggerSchema(Description = "Dükkan cari kodu (örn: 120.001.IS)")]
            public string DUKKAN_CARI_KODU { get; set; } = string.Empty;

            /// <summary>
            /// Dükkan adresi (Logo'dan ham hali)
            /// </summary>
            [SwaggerSchema(Description = "Logo'daki ham dükkan adres tanımı")]
            public string DUKKAN_ADRES_ORJINAL { get; set; } = string.Empty;

            /// <summary>
            /// Cadde / sokak adı
            /// </summary>
            [SwaggerSchema(Description = "Cadde adı")]
            public string CADDE { get; set; } = string.Empty;

            /// <summary>
            /// Kapı / dükkan numarası (eski no)
            /// </summary>
            [SwaggerSchema(Description = "Eski kapı numarası")]
            public string NO { get; set; } = string.Empty;

            /// <summary>
            /// Pasaj numarası
            /// </summary>
            [SwaggerSchema(Description = "Pasaj numarası")]
            public string PASAJ_NO { get; set; } = string.Empty;

            /// <summary>
            /// Yeni belediye numarası
            /// </summary>
            [SwaggerSchema(Description = "Yeni belediye kapı numarası")]
            public string YENI_NO { get; set; } = string.Empty;

            /// <summary>
            /// Kat bilgisi (ASMAKAT, ZEMİN KAT, DEPOLAR vb.)
            /// </summary>
            [SwaggerSchema(Description = "Kat bilgisi")]
            public string KAT { get; set; } = string.Empty;

            /// <summary>
            /// 1. yetkili ad soyad
            /// </summary>
            [SwaggerSchema(Description = "1. Yetkili ad soyad")]
            public string YETKILI1_AD_SOYAD { get; set; } = string.Empty;

            /// <summary>
            /// 1. yetkili e-posta adresi
            /// </summary>
            [SwaggerSchema(Description = "1. Yetkili e-posta")]
            public string YETKILI1_EMAIL { get; set; } = string.Empty;

            /// <summary>
            /// 1. yetkili telefon numarası
            /// </summary>
            [SwaggerSchema(Description = "1. Yetkili telefon")]
            public string YETKILI1_TELEFON { get; set; } = string.Empty;

            /// <summary>
            /// 2. yetkili ad soyad
            /// </summary>
            [SwaggerSchema(Description = "2. Yetkili ad soyad")]
            public string YETKILI2_AD_SOYAD { get; set; } = string.Empty;

            /// <summary>
            /// 2. yetkili e-posta adresi
            /// </summary>
            [SwaggerSchema(Description = "2. Yetkili e-posta")]
            public string YETKILI2_EMAIL { get; set; } = string.Empty;

            /// <summary>
            /// 2. yetkili telefon numarası
            /// </summary>
            [SwaggerSchema(Description = "2. Yetkili telefon")]
            public string YETKILI2_TELEFON { get; set; } = string.Empty;
        }

        public class ClCardInfoSearchViewModel
        {
            /// <summary>
            /// Cari kart kodu (Ana cari kodu)
            /// </summary>
            [SwaggerSchema(Description = "Ana cari kart kodu")]
            public string? CariKodu { get; set; } = string.Empty;

            /// <summary>
            /// Cari unvanı / firma adı
            /// </summary>
            [SwaggerSchema(Description = "Cari firma adı / unvanı")]
            public string? CariUnvan { get; set; } = string.Empty;

            /// <summary>
            /// Vergi dairesi adı
            /// </summary>
            [SwaggerSchema(Description = "Vergi dairesi")]
            public string? VergiDairesi { get; set; } = string.Empty;

            /// <summary>
            /// Vergi kimlik numarası
            /// </summary>
            [SwaggerSchema(Description = "Vergi numarası")]
            public string? VergiNumarasi { get; set; } = string.Empty;

            /// <summary>
            /// T.C. Kimlik numarası (şahıs firmaları için)
            /// </summary>
            [SwaggerSchema(Description = "T.C. Kimlik numarası")]
            public string? Tckn { get; set; } = string.Empty;

            /// <summary>
            /// İl adı
            /// </summary>
            [SwaggerSchema(Description = "İl")]
            public string? Il { get; set; } = string.Empty;

            /// <summary>
            /// İlçe adı
            /// </summary>
            [SwaggerSchema(Description = "İlçe")]
            public string? Ilce { get; set; } = string.Empty;

            /// <summary>
            /// Mahalle / semt
            /// </summary>
            [SwaggerSchema(Description = "Mahalle")]
            public string? Mahalle { get; set; } = string.Empty;

            /// <summary>
            /// Adres satırı 1
            /// </summary>
            [SwaggerSchema(Description = "Adres satırı 1")]
            public string? Adres1 { get; set; } = string.Empty;

            /// <summary>
            /// Adres satırı 2
            /// </summary>
            [SwaggerSchema(Description = "Adres satırı 2")]
            public string? Adres2 { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 1
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 1")]
            public string? OzelKod { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 2
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 2")]
            public string? OzelKod2 { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 3
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 3")]
            public string? OzelKod3 { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 4
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 4")]
            public string? OzelKod4 { get; set; } = string.Empty;

            /// <summary>
            /// Özel kod 5
            /// </summary>
            [SwaggerSchema(Description = "Özel kod 5")]
            public string? OzelKod5 { get; set; } = string.Empty;

            // Dükkan bilgileri
            /// <summary>
            /// Dükkan cari kodu (.IS ile biten)
            /// </summary>
            [SwaggerSchema(Description = "Dükkan cari kodu (örn: 120.001.IS)")]
            public string? DukkanCariKodu { get; set; } = string.Empty;

            /// <summary>
            /// Dükkan adresi (Logo'dan ham hali)
            /// </summary>
            [SwaggerSchema(Description = "Logo'daki ham dükkan adres tanımı")]
            public string? DukkanAdresOrjinal { get; set; } = string.Empty;

            // Parçalanmış adres
            /// <summary>
            /// Cadde / sokak adı (örn: A, H1, D)
            /// </summary>
            [SwaggerSchema(Description = "Cadde adı")]
            public string? Cadde { get; set; } = string.Empty;

            /// <summary>
            /// Kapı / dükkan numarası (eski no)
            /// </summary>
            [SwaggerSchema(Description = "Eski kapı numarası")]
            public string? No { get; set; } = string.Empty;

            /// <summary>
            /// Pasaj numarası (1.PASAJ ise 1 döner)
            /// </summary>
            [SwaggerSchema(Description = "Pasaj numarası")]
            public string? PasajNo { get; set; } = string.Empty;

            /// <summary>
            /// Yeni belediye numarası
            /// </summary>
            [SwaggerSchema(Description = "Yeni kapı numarası")]
            public string? YeniNo { get; set; } = string.Empty;

            /// <summary>
            /// Kat bilgisi (ASMAKAT, ZEMİN KAT, DEPOLAR, BODRUM KAT vb.)
            /// </summary>
            [SwaggerSchema(Description = "Kat bilgisi")]
            public string? Kat { get; set; } = string.Empty;

            // Yetkili 1
            /// <summary>
            /// 1. yetkili ad soyad
            /// </summary>
            [SwaggerSchema(Description = "1. Yetkili ad soyad")]
            public string? Yetkili1AdSoyad { get; set; } = string.Empty;

            // Yetkili 2
            /// <summary>
            /// 2. yetkili ad soyad
            /// </summary>
            [SwaggerSchema(Description = "2. Yetkili ad soyad")]
            public string? Yetkili2AdSoyad { get; set; } = string.Empty;

        }
    }
}
