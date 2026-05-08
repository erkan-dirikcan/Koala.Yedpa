SELECT
    -- Cari Bilgileri
    CLNTC.LOGICALREF        AS [CARI REFERANS],
    CLNTC.CODE              AS [MUSTERI KODU],
    CLNTC.DEFINITION_       AS [MUSTERI ADI],
    
    -- FATURA KÜNYESİ
    PTRNS.FICHEREF          AS [FATURA LOGICALREF],
    INVFC.FICHENO           AS [FATURA NO],
    PTRNS.PROCDATE          AS [FATURA TARIHI],
    CASE INVFC.TRCODE
        WHEN 1  THEN 'Mal Alım Faturası'
        WHEN 2  THEN 'Perakende Satış İade Faturası'
        WHEN 3  THEN 'Toptan Satış İade Faturası'
        WHEN 4  THEN 'Alınan Hizmet Faturası'
        WHEN 5  THEN 'Alınan Proforma Fatura'
        WHEN 6  THEN 'Alım İade Faturası'
        WHEN 7  THEN 'Alım Fiyat Farkı Faturası'
        WHEN 8  THEN 'Perakende Satış Faturası'
        WHEN 9  THEN 'Toptan Satış Faturası'
        WHEN 10 THEN 'Verilen Hizmet Faturası'
        WHEN 11 THEN 'Verilen Proforma Fatura'
        WHEN 12 THEN 'Verilen Vade Farkı Faturası'
        WHEN 13 THEN 'Satış Fiyat Farkı Faturası'
        WHEN 14 THEN 'Satınalma Fiyat Farkı Faturası'
        WHEN 26 THEN 'Müstahsil Makbuzu'
        WHEN 32 THEN 'Alınan Fiyat Farkı Faturası'
        WHEN 33 THEN 'Verilen Fiyat Farkı Faturası'
        ELSE 'Diğer'
    END                     AS [FATURA TURU],
    INVFC.GENEXP1           AS [FATURA ACIKLAMA1],
    INVFC.GENEXP2           AS [FATURA ACIKLAMA2],
    
    -- TUTAR BİLGİLERİ (Kalan Tutar Hesabı)
    PTRNS.TOTAL             AS [FATURA VADE TUTARI],
    ISNULL(KAPATILAN.KAPANAN_TUTAR, 0) AS [KAPATILAN TUTAR],
    (PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0)) AS [KALAN ODENECEK TUTAR],
    
    -- TARİH VE VADE BİLGİLERİ
    PTRNS.DATE_             AS [VADE TARIHI],
    DATEPART(mm, PTRNS.DATE_) AS AY,
    DATEPART(wk, PTRNS.DATE_) AS HAFTA,
    DATEDIFF(DAY, PTRNS.PROCDATE, PTRNS.DATE_) AS [VADE GUN],
    DATEDIFF(DAY, GETDATE(), PTRNS.DATE_)      AS [KALAN GUN],
    
    -- DÖVİZ
    CASE PTRNS.TRCURR
        WHEN 0  THEN 'TL'
        WHEN 1  THEN 'USD'
        WHEN 20 THEN 'EUR'
        ELSE ''
    END                     AS [DOVIZ TURU],
    
    -- DURUM
    CASE
        WHEN PTRNS.PAID = 0 THEN 'AÇIK'
        WHEN PTRNS.PAID = 1 AND (PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0)) > 0 THEN 'KISMİ ÖDEME'
        ELSE 'KAPALI'
    END                     AS [DURUM]

FROM LG_211_12_PAYTRANS AS PTRNS
    INNER JOIN LG_211_CLCARD AS CLNTC
        ON CLNTC.LOGICALREF = PTRNS.CARDREF
    LEFT OUTER JOIN LG_211_12_INVOICE AS INVFC
        ON INVFC.LOGICALREF = PTRNS.FICHEREF
    -- Bu vade satırını kapatan ödeme hareketlerinin toplamı
    LEFT OUTER JOIN (
        SELECT
            CROSSREF,
            SUM(PAID) AS KAPANAN_TUTAR
        FROM LG_211_12_PAYTRANS
        WHERE CROSSREF <> 0
          AND CANCELLED = 0
        GROUP BY CROSSREF
    ) AS KAPATILAN
        ON KAPATILAN.CROSSREF = PTRNS.LOGICALREF

WHERE
    PTRNS.CROSSREF = 0          -- Ana fatura/vade satırı
    AND PTRNS.CANCELLED = 0
    AND PTRNS.SIGN = 1          -- Borç (tahsil edilecek)
    AND ISNULL(INVFC.FROMKASA, 0) = 0
    AND (PTRNS.TOTAL - ISNULL(KAPATILAN.KAPANAN_TUTAR, 0)) > 0

ORDER BY PTRNS.DATE_ DESC