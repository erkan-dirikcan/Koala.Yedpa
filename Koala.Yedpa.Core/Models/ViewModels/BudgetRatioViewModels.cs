using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateBudgetRatioViewModel
{
    /// <summary>
    /// Bütçe / Ek Bütçe Kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// Bütçe / Ek Bütçe Açıklama
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Bütçe / Ek Bütçe Yılı
    /// </summary>
    public int Year { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Geçen Yıla Göre Oranı
    /// </summary>
    public decimal Ratio { get; set; }
    /// <summary>
    /// Toplam / Ek Bütçe Bütçe
    /// </summary>
    public decimal TotalBugget { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Ayları (Flag Enum - BuggetRatioMounthEnum'ü inceleyiniz.)
    /// </summary>
    public BuggetRatioMounthEnum BuggetRatioMounths { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Türü (Detaylar için BuggetTypeEnum'a bakınız)
    /// </summary>
    public BuggetTypeEnum BuggetType { get; set; }
}

public class UpdateBudgetRatioViewModel
{
    /// <summary>
    /// Kayıt Id
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Bütçe / Ek Bütçe Kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// Bütçe / Ek Bütçe Açıklama
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Bütçe / Ek Bütçe Yılı
    /// </summary>
    public int Year { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Geçen Yıla Göre Oranı
    /// </summary>
    public decimal Ratio { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Toplam Bütçe
    /// </summary>
    public decimal TotalBugget { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Ayları (Flag Enum - BuggetRatioMounthEnum'ü inceleyiniz.)
    /// </summary>
    public BuggetRatioMounthEnum BuggetRatioMounths { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Türü (Detaylar için BuggetTypeEnum'a bakınız)
    /// </summary>
    public BuggetTypeEnum BuggetType { get; set; }
}

public class BudgetRatioListViewModel
{
    /// <summary>
    /// Bütçe / Ek Bütçe Kayıt Id
    /// </summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>
    /// Bütçe / Ek Bütçe Kodu
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Bütçe / Ek Bütçe Yılı
    /// </summary>
    public int Year { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Geçen Yıla Göre Oranı
    /// </summary>
    public decimal Ratio { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Toplam Bütçe
    /// </summary>
    public decimal TotalBugget { get; set; }

    /// <summary>
    /// Bütçe / Ek Bütçe Türü (Detaylar için BuggetTypeEnum'a bakınız)
    /// </summary>
    public BuggetTypeEnum BuggetType { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Durumu
    /// </summary>
    public StatusEnum Status { get; set; }

}

public class BudgetRatioDetailViewModel : BudgetRatioListViewModel
{
    /// <summary>
    /// Bütçe / Ek Bütçe Açıklama
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Bütçe / Ek Bütçe Ayları (Flag Enum - BuggetRatioMounthEnum'ü inceleyiniz.)
    /// </summary>
    public BuggetRatioMounthEnum BuggetRatioMounths { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Oluşturulma Zamanı
    /// </summary>
    public DateTime CreateTime { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Son Güncellenme Zamanı
    /// </summary>
    public DateTime? LastUpdateTime { get; set; }
    // Additional details can be added here
}

public class BudgetRatioSearchViewModel
{
    /// <summary>
    /// Bütçe / Ek Bütçe Kodu
    /// </summary>
    public string? Code { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Yılı
    /// </summary>
    public int? Year { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Türü (Detaylar için BuggetTypeEnum'a bakınız)
    /// </summary>
    public BuggetTypeEnum? BuggetType { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Ayları (Flag Enum - BuggetRatioMounthEnum'ü inceleyiniz.)
    /// </summary>
    public BuggetRatioMounthEnum? BuggetRatioMounths { get; set; }
    /// <summary>
    /// Bütçe / Ek Bütçe Durumu
    /// </summary>
    public StatusEnum? Status { get; set; }
    /// <summary>
    /// Bulunacak Sayfa Numarası
    /// </summary>
    public int PageIndex { get; set; } = 1;
    /// <summary>
    /// Bulunacak Sayfa Başına Kayıt Sayısı
    /// </summary>
    public int PageSize { get; set; } = 10;
}
