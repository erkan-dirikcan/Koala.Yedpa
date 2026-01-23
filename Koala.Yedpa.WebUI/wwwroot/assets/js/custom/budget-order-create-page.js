// Budget Order Create Page JavaScript
document.addEventListener('DOMContentLoaded', function () {
    const budgetTypeRadios = document.querySelectorAll('input[name="budgetType"]');
    const sourceYearSelect = document.getElementById('sourceYear');
    const targetYearInput = document.getElementById('targetYear');
    const newRatioPercentInput = document.getElementById('newRatioPercent');
    const newBudgetInput = document.getElementById('newBudget');
    const sourceYearLoading = document.getElementById('sourceYearLoading');
    const monthsSelectionSection = document.getElementById('monthsSelectionSection');
    const calculateSection = document.getElementById('calculateSection');
    const duesStatisticSection = document.getElementById('duesStatisticSection');
    const oldBudgetSummary = document.getElementById('oldBudgetSummary');
    const newBudgetSummary = document.getElementById('newBudgetSummary');
    const calculateBtn = document.getElementById('calculateBtn');
    const saveBtn = document.getElementById('saveBtn');
    const form = document.getElementById('createBudgetForm');

    // Radio buttons for calculation type
    const ratioRadio = document.getElementById('ratioRadio');
    const amountRadio = document.getElementById('amountRadio');
    const ratioInputSection = document.getElementById('ratioInputSection');
    const amountInputSection = document.getElementById('amountInputSection');

    let dataTable = null;
    let currentBudgetType = 1; // Default: Bütçe
    let currentCalculationType = 'ratio'; // Default: Oran
    let sourceYearData = [];
    let hasCalculated = false;

    // Sayfa yüklendiğinde kaynak yılları getir
    loadSourceYears();

    // Kaydet butonunu başlangıçta pasif yap
    if (saveBtn) {
        saveBtn.disabled = true;
    }

    // Hesaplama türü değiştiğinde (Oran / Toplam Bütçe)
    if (ratioRadio && amountRadio) {
        ratioRadio.addEventListener('change', function () {
            if (this.checked) {
                currentCalculationType = 'ratio';
                ratioInputSection.style.display = 'block';
                amountInputSection.style.display = 'none';
                newBudgetInput.value = ''; // Toplam bütçe inputunu temizle
            }
        });

        amountRadio.addEventListener('change', function () {
            if (this.checked) {
                currentCalculationType = 'amount';
                ratioInputSection.style.display = 'none';
                amountInputSection.style.display = 'block';
                newRatioPercentInput.value = ''; // Oran inputunu temizle
            }
        });
    }

    // Bütçe türü değiştiğinde
    budgetTypeRadios.forEach(radio => {
        radio.addEventListener('change', function () {
            currentBudgetType = parseInt(this.value);

            if (currentBudgetType === 2) {
                // Ek Bütçe seçildi
                targetYearInput.disabled = true;
                targetYearInput.value = '';
                targetYearInput.removeAttribute('required');
            } else {
                // Bütçe seçildi
                targetYearInput.disabled = false;
                targetYearInput.setAttribute('required', 'required');
            }
        });
    });

    // Kaynak yıl değiştiğinde
    sourceYearSelect.addEventListener('change', function () {
        const selectedYear = this.value;
        if (selectedYear) {
            // Spinner göster
            sourceYearLoading.style.display = 'block';
            sourceYearSelect.disabled = true;

            // Veriyi yükle - Her zaman budgetType=1 (kaynaktaki mevcut kayıtları çek)
            loadDuesStatisticData(selectedYear, 1);
        } else {
            duesStatisticSection.style.display = 'none';
            monthsSelectionSection.style.display = 'none';
            calculateSection.style.display = 'none';
            oldBudgetSummary.style.display = 'none';
            newBudgetSummary.style.display = 'none';
            saveBtn.disabled = true;

            if (dataTable) {
                dataTable.clear().draw();
            }
        }
    });

    // Tüm ayları seç
    document.getElementById('selectAllMonths')?.addEventListener('click', function () {
        document.querySelectorAll('.month-checkbox').forEach(cb => cb.checked = true);
    });

    // Seçimi kaldır
    document.getElementById('deselectAllMonths')?.addEventListener('click', function () {
        document.querySelectorAll('.month-checkbox').forEach(cb => cb.checked = false);
    });

    // Hesapla ve Dağıt butonu
    calculateBtn.addEventListener('click', async function () {
        const ratioPercentValue = newRatioPercentInput.value.trim();
        const budgetValue = newBudgetInput.value.trim();
        const ratioPercent = ratioPercentValue !== '' ? parseFloat(ratioPercentValue) : null;
        const newBudget = budgetValue !== '' ? parseFloat(budgetValue) : null;

        // Checkbox flag değerlerini topla (1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048)
        const selectedMonthsFlag = Array.from(document.querySelectorAll('.month-checkbox:checked'))
            .reduce((sum, cb) => sum + parseInt(cb.value), 0);

        if (selectedMonthsFlag === 0) {
            showToast('error', 'Hata', 'En az bir ay seçmelisiniz!');
            return;
        }

        if (ratioPercent === null && !newBudget) {
            showToast('error', 'Hata', 'Lütfen geçerli bir Oran veya Toplam Bütçe giriniz (ikisinden biri yeterli)!');
            return;
        }

        // Yüzdeyi ratioya çevir (örn: 25 -> 1.25)
        const newRatio = ratioPercent !== null ? 1 + (ratioPercent / 100) : null;

        // Show loading
        calculateBtn.disabled = true;
        calculateBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Hesaplanıyor...';

        try {
            const request = {
                sourceYear: parseInt(sourceYearSelect.value),
                budgetType: currentBudgetType,
                selectedMonthsFlag: selectedMonthsFlag,
                ratio: newRatio,
                targetAmount: newBudget
            };

            const response = await fetch('/api/BudgetOrderApi/CalculateBudget', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(request)
            });

            const result = await response.json();

            if (response.ok && result.isSuccess && result.data) {
                const data = result.data;

                // Update source data with calculated values
                sourceYearData = data.calculatedDuesStatistics;

                // Show old budget total
                const oldBudgetTotalEl = document.getElementById('oldBudgetTotal');
                if (oldBudgetTotalEl) {
                    oldBudgetTotalEl.textContent = data.selectedMonthsTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2 });
                }
                oldBudgetSummary.style.display = 'block';

                // Tabloyu göster
                duesStatisticSection.style.display = 'block';

                // DataTable başlat veya güncelle
                if (!dataTable) {
                    initDataTable();
                }

                // Tabloyu doldur
                populateTable(sourceYearData);

                // Update footer totals
                updateTotals();

                // Show new budget total
                const newBudgetTotalEl = document.getElementById('newBudgetTotal');
                const appliedRatioEl = document.getElementById('appliedRatio');
                if (newBudgetTotalEl) {
                    newBudgetTotalEl.textContent = data.calculatedTotal.toLocaleString('tr-TR', { minimumFractionDigits: 2 });
                }
                if (appliedRatioEl) {
                    appliedRatioEl.textContent = data.appliedPercentage.toFixed(2);
                }
                newBudgetSummary.style.display = 'block';

                // Update ratio input (convert back to percent)
                newRatioPercentInput.value = ((data.appliedRatio - 1) * 100).toFixed(0);

                // Kaydet butonunu göster ve aktif et
                const saveSection = document.getElementById('saveSection');
                if (saveSection) {
                    saveSection.style.display = 'block';
                }
                saveBtn.disabled = false;
                hasCalculated = true;

                const method = newRatio ? 'Oran ile hesaplama' : 'Bütçe ile hesaplama (oran otomatik)';
                showToast('success', 'Başarılı', `${method} tamamlandı! Kaydetmek için butona tıklayın.`);
            } else {
                showToast('error', 'Hata', result.message || 'Hesaplama başarısız!');
            }
        } catch (error) {
            console.error('Error:', error);
            showToast('error', 'Hata', 'Bir hata oluştu: ' + error.message);
        } finally {
            calculateBtn.disabled = false;
            calculateBtn.innerHTML = '<i class="ki ki-calculator"></i>Hesapla ve Dağıt';
        }
    });

    // Kaydet butonu
    if (saveBtn) {
        saveBtn.addEventListener('click', async function (e) {
            e.preventDefault();

            const targetYear = parseInt(targetYearInput.value);
            // Hesaplanan oranı kullan (bütçe girilse bile oran hesaplanmıştı)
            const ratioPercentValue = newRatioPercentInput.value.trim();
            const ratioPercent = ratioPercentValue !== '' ? parseFloat(ratioPercentValue) : NaN;
            const finalRatio = !isNaN(ratioPercent) ? 1 + (ratioPercent / 100) : NaN;
            const selectedMonthsFlag = Array.from(document.querySelectorAll('.month-checkbox:checked'))
                .reduce((sum, cb) => sum + parseInt(cb.value), 0);

            if (!sourceYearSelect.value) {
                showToast('error', 'Hata', 'Kaynak yıl seçiniz!');
                return;
            }

            if (selectedMonthsFlag === 0) {
                showToast('error', 'Hata', 'En az bir ay seçmelisiniz!');
                return;
            }

            if (!hasCalculated) {
                showToast('error', 'Hata', 'Önce hesaplama yapınız!');
                return;
            }

            if (isNaN(finalRatio) || finalRatio <= 0) {
                showToast('error', 'Hata', 'Geçerli bir oran hesaplanmalı!');
                return;
            }

            if (currentBudgetType === 1 && !targetYear) {
                showToast('error', 'Hata', 'Hedef yıl giriniz!');
                return;
            }

            saveBtn.disabled = true;
            saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Kaydediliyor...';

            try {
                // Flag değerini ay numaralarına çevir
                const selectedMonths = ParseMonthsFlag(selectedMonthsFlag);

                // Kaydetme işlemi
                const saveData = {
                    sourceYear: parseInt(sourceYearSelect.value),
                    targetYear: currentBudgetType === 1 ? targetYear : null,
                    budgetType: currentBudgetType,
                    ratio: finalRatio,
                    selectedMonths: selectedMonths,
                    duesData: sourceYearData.map(item => ({
                        id: item.id,
                        code: item.code,
                        divCode: item.divCode,
                        divName: item.divName,
                        docTrackingNr: item.docTrackingNr,
                        clientCode: item.clientCode,
                        clientRef: item.clientRef,
                        budgetType: currentBudgetType,
                        january: item.january,
                        february: item.february,
                        march: item.march,
                        april: item.april,
                        may: item.may,
                        june: item.june,
                        july: item.july,
                        august: item.august,
                        september: item.september,
                        october: item.october,
                        november: item.november,
                        december: item.december,
                        total: item.total
                    }))
                };

                const response = await fetch('/api/BudgetOrderApi/SaveNewBudget', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(saveData)
                });

                const result = await response.json();

                if (response.ok && result.isSuccess) {
                    showToast('success', 'Başarılı', result.message || 'Bütçe başarıyla kaydedildi!');

                    // Index sayfasına yönlendir
                    setTimeout(() => {
                        window.location.href = '/BudgetOrder/Index';
                    }, 2000);
                } else {
                    showToast('error', 'Hata', result.message || 'Kayıt başarısız!');
                }
            } catch (error) {
                console.error('Error:', error);
                showToast('error', 'Hata', 'Bir hata oluştu: ' + error.message);
            } finally {
                saveBtn.disabled = false;
                saveBtn.innerHTML = '<span class="svg-icon svg-icon-md"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="24px" height="24px" viewBox="0 0 24 24" version="1.1"><g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd"><rect x="0" y="0" width="24" height="24" /><circle fill="#000000" cx="9" cy="15" r="6" /><path d="M8.8012943,7.00241953 C9.83837775,5.20768121 11.7781543,4 14,4 C17.3137085,4 20,6.6862915 20,10 C20,12.2218457 18.7923188,14.1616223 16.9975805,15.1987057 C16.9991904,15.1326658 17,15.0664274 17,15 C17,10.581722 13.418278,7 9,7 C8.93357256,7 8.86733422,7.00080962 8.8012943,7.00241953 Z" fill="#000000" opacity="0.3" /></g></svg></span>Kaydet';
            }
        });
    }

    // Form submit (backup)
    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        const targetYear = parseInt(targetYearInput.value);
        // Hesaplanan oranı kullan (bütçe girilse bile oran hesaplanmıştı)
        const ratioPercentValue = newRatioPercentInput.value.trim();
        const ratioPercent = ratioPercentValue !== '' ? parseFloat(ratioPercentValue) : NaN;
        const finalRatio = !isNaN(ratioPercent) ? 1 + (ratioPercent / 100) : NaN;
        const selectedMonthsFlag = Array.from(document.querySelectorAll('.month-checkbox:checked'))
            .reduce((sum, cb) => sum + parseInt(cb.value), 0);

        if (!sourceYearSelect.value) {
            showToast('error', 'Hata', 'Kaynak yıl seçiniz!');
            return;
        }

        if (selectedMonthsFlag === 0) {
            showToast('error', 'Hata', 'En az bir ay seçmelisiniz!');
            return;
        }

        if (!hasCalculated) {
            showToast('error', 'Hata', 'Önce hesaplama yapınız!');
            return;
        }

        if (isNaN(finalRatio) || finalRatio <= 0) {
            showToast('error', 'Hata', 'Geçerli bir oran hesaplanmalı!');
            return;
        }

        if (currentBudgetType === 1 && !targetYear) {
            showToast('error', 'Hata', 'Hedef yıl giriniz!');
            return;
        }

        saveBtn.disabled = true;
        saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Kaydediliyor...';

        try {
            // Flag değerini ay numaralarına çevir
            const selectedMonths = ParseMonthsFlag(selectedMonthsFlag);

            // Kaydetme işlemi
            const saveData = {
                sourceYear: parseInt(sourceYearSelect.value),
                targetYear: currentBudgetType === 1 ? targetYear : null,
                budgetType: currentBudgetType,
                ratio: finalRatio,
                selectedMonths: selectedMonths,
                duesData: sourceYearData.map(item => ({
                    id: item.id,
                    code: item.code,
                    divCode: item.divCode,
                    divName: item.divName,
                    docTrackingNr: item.docTrackingNr,
                    clientCode: item.clientCode,
                    clientRef: item.clientRef,
                    // Aylar (orantılı değerler)
                    january: item.january,
                    february: item.february,
                    march: item.march,
                    april: item.april,
                    may: item.may,
                    june: item.june,
                    july: item.july,
                    august: item.august,
                    september: item.september,
                    october: item.october,
                    november: item.november,
                    december: item.december,
                    total: item.total
                }))
            };

            const response = await fetch('/api/BudgetOrderApi/SaveNewBudget', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(saveData)
            });

            const result = await response.json();

            if (response.ok && result.isSuccess) {
                showToast('success', 'Başarılı', result.message || 'Bütçe başarıyla kaydedildi!');

                setTimeout(() => {
                    window.location.href = '/BudgetOrder/Index';
                }, 2000);
            } else {
                showToast('error', 'Hata', result.message || 'Kayıt başarısız!');
            }
        } catch (error) {
            console.error('Error:', error);
            showToast('error', 'Hata', 'Bir hata oluştu: ' + error.message);
        } finally {
            saveBtn.disabled = false;
            saveBtn.innerHTML = '<span class="svg-icon svg-icon-md"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="24px" height="24px" viewBox="0 0 24 24" version="1.1"><g stroke="none" stroke-width="1" fill="none" fill-rule="evenodd"><rect x="0" y="0" width="24" height="24" /><circle fill="#000000" cx="9" cy="15" r="6" /><path d="M8.8012943,7.00241953 C9.83837775,5.20768121 11.7781543,4 14,4 C17.3137085,4 20,6.6862915 20,10 C20,12.2218457 18.7923188,14.1616223 16.9975805,15.1987057 C16.9991904,15.1326658 17,15.0664274 17,15 C17,10.581722 13.418278,7 9,7 C8.93357256,7 8.86733422,7.00080962 8.8012943,7.00241953 Z" fill="#000000" opacity="0.3" /></g></svg></span>Kaydet';
        }
    });

    // Kaynak yılları yükle
    async function loadSourceYears() {
        try {
            const response = await fetch('/api/DuesStatisticApi/GetDistinctYears');
            const result = await response.json();

            if (response.ok && result.isSuccess && result.data) {
                sourceYearSelect.innerHTML = '<option value="">Seçiniz...</option>';
                result.data.forEach(year => {
                    const option = document.createElement('option');
                    option.value = year;
                    option.textContent = year;
                    sourceYearSelect.appendChild(option);
                });
            }
        } catch (error) {
            console.error('Error loading source years:', error);
        }
    }

    // DuesStatistic verilerini yükle
    async function loadDuesStatisticData(year, budgetType) {
        try {
            const response = await fetch(`/api/DuesStatisticApi/GetByYearAndType?year=${year}&budgetType=${budgetType}`);
            const result = await response.json();

            if (response.ok && result.isSuccess && result.data) {
                sourceYearData = result.data;

                // Tabloyu göster
                duesStatisticSection.style.display = 'block';

                // DataTable başlat (önce)
                initDataTable();

                // Tabloyu doldur (sonra)
                populateTable(sourceYearData);

                // Ay sütunlarının görünürlüğünü güncelle
                setTimeout(() => updateMonthColumnsVisibility(), 100);

                // Yeni hesaplama varsa gizle
                if (!hasCalculated) {
                    newBudgetSummary.style.display = 'none';
                    // Kaynak yıl toplamını da gizle (henüz aylar seçilmedi)
                    oldBudgetSummary.style.display = 'none';
                }

                // Aylar ve hesaplama bölümünü göster
                monthsSelectionSection.style.display = 'block';
                calculateSection.style.display = 'block';
            } else {
                showToast('error', 'Hata', result.message || 'Veriler yüklenemedi!');
            }
        } catch (error) {
            console.error('Error loading dues statistic data:', error);
            showToast('error', 'Hata', 'Veriler yüklenemedi!');
        } finally {
            // Spinner gizle ve select'i aktif et
            sourceYearLoading.style.display = 'none';
            sourceYearSelect.disabled = false;
        }
    }

    // DataTable başlat
    function initDataTable() {
        const tableElement = document.getElementById('duesStatisticTable');
        if (!tableElement) {
            console.error('Table element not found');
            return;
        }

        if (dataTable) {
            dataTable.destroy();
            dataTable = null;
        }

        dataTable = $('#duesStatisticTable').DataTable({
            autoWidth: false,
            data: sourceYearData,
            dom: '<"top"Bf>rt<"bottom"lip><"clear">',
            buttons: [
                {
                    extend: 'excel',
                    text: '<i class="fas fa-file-excel"></i> Excel\'e Aktar',
                    className: 'btn btn-success btn-sm mr-2',
                    filename: function () {
                        const date = new Date().toISOString().slice(0, 10);
                        const targetYear = document.getElementById('targetYear')?.value || sourceYearSelect.value;
                        return `Butce_Hesaplamasi_${ targetYear }_${ date }`;
                    },
                    title: function () {
                        const targetYear = document.getElementById('targetYear')?.value || sourceYearSelect.value;
                        const sourceYear = sourceYearSelect.value;
                        return `Bütçe Hesaplaması - Kaynak Yıl: ${ sourceYear }, Hedef Yıl: ${ targetYear }`;
                    },
                    sheetName: 'Bütçe Hesaplaması',
                    exportOptions: {
                        columns: ':visible',
                        format: {
                            body: function (data, row, column, node) {
                                // Para birimi sütunları için (Toplam sütunu - column 4)
                                if (column === 4) {
                                    var parsedValue = 0;

                                    if (typeof data === 'string') {
                                        // "1.234,56 TL" veya "1.234,56" formatını temizle
                                        var cleanData = data.replace(/\s*TL\s*/g, '').trim();
                                        if (cleanData && cleanData !== '-') {
                                            parsedValue = parseFloat(cleanData.replace(/\./g, '').replace(',', '.'));
                                        }
                                    } else if (typeof data === 'number') {
                                        parsedValue = data;
                                    }

                                    // Sayısal değer döndür
                                    return !isNaN(parsedValue) ? parsedValue : 0;
                                }
                                return data;
                            }
                        }
                    },
                    customize: function (xlsx) {
                        var sheet = xlsx.xl.worksheets[ 'sheet1.xml' ];
                        var styles = xlsx.xl[ 'styles.xml' ];

                        // numFmts kontrolü ve oluşturma
                        var numFmts = $('numFmts', styles);
                        if (numFmts.length === 0) {
                            var styleSheet = $('styleSheet', styles);
                            styleSheet.prepend('<numFmts count="1"><numFmt numFmtId="164" formatCode="#,##0.00"/></numFmts>');
                        } else {
                            var count = parseInt(numFmts.attr('count')) || 0;
                            numFmts.attr('count', count + 1);
                            numFmts.append('<numFmt numFmtId="164" formatCode="#,##0.00"/>');
                        }

                        // cellXfs'e yeni stil ekle
                        var cellXfs = $('cellXfs', styles);
                        var xfCount = parseInt(cellXfs.attr('count')) || 0;
                        var newStyleId = xfCount;
                        cellXfs.attr('count', xfCount + 1);
                        cellXfs.append('<xf numFmtId="164" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/>');

                        // Başlık satırı
                        $('row:first c', sheet).attr('s', '2');

                        // Para birimi sütunları (E: Toplam sütunu - 5. sütun)
                        var colLetters = ['E'];

                        colLetters.forEach(function (col) {
                            $('row c[r^="' + col + '"]', sheet).each(function (index) {
                                // Başlık satırını atla (index 0)
                                if (index > 0) {
                                    var cell = $(this);
                                    var value = cell.find('v').text();

                                    // Eğer değer varsa
                                    if (value && value !== '') {
                                        cell.attr('t', 'n'); // Sayı tipi
                                        cell.attr('s', newStyleId.toString()); // Yeni stil uygula
                                    }
                                }
                            });
                        });
                    }
                }
            ],
            columns: [
                {
                    className: 'details-control',
                    orderable: false,
                    data: null,
                    defaultContent: '',
                    width: '30px'
                },
                {
                    data: 'divName',
                    width: '200px'
                },
                { data: 'divCode' },
                { data: 'clientCode' },
                {
                    data: 'total',
                    render: function (data) {
                        return formatCurrency(data);
                    },
                    className: 'text-right total-cell font-weight-bold'
                }
            ],
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, 'Tümü']],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json'
            },
            order: [[1, 'asc']],
            drawCallback: function () {
                updateTotals();
            }
        });

        // Child row toggle - detay satırını aç/kapat
        $('#duesStatisticTable').on('click', 'td.details-control', function () {
            const tr = $(this).closest('tr');
            const row = dataTable.row(tr);

            if (row.child.isShown()) {
                row.child.hide();
                tr.removeClass('shown');
            } else {
                const data = row.data();

                const childRowContent = `
                    <table class="table table-sm table-borderless" style="width: 100%; margin: 0;">
                        <thead>
                            <tr style="background-color: #f8f9fa;">
                                <th style="width: 8%;">Ocak</th>
                                <th style="width: 8%;">Şubat</th>
                                <th style="width: 8%;">Mart</th>
                                <th style="width: 8%;">Nisan</th>
                                <th style="width: 8%;">Mayıs</th>
                                <th style="width: 8%;">Haziran</th>
                                <th style="width: 8%;">Temmuz</th>
                                <th style="width: 8%;">Ağustos</th>
                                <th style="width: 8%;">Eylül</th>
                                <th style="width: 8%;">Ekim</th>
                                <th style="width: 8%;">Kasım</th>
                                <th style="width: 8%;">Aralık</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td class="text-end">${data.january > 0 ? formatCurrency(data.january) : '-'}</td>
                                <td class="text-end">${data.february > 0 ? formatCurrency(data.february) : '-'}</td>
                                <td class="text-end">${data.march > 0 ? formatCurrency(data.march) : '-'}</td>
                                <td class="text-end">${data.april > 0 ? formatCurrency(data.april) : '-'}</td>
                                <td class="text-end">${data.may > 0 ? formatCurrency(data.may) : '-'}</td>
                                <td class="text-end">${data.june > 0 ? formatCurrency(data.june) : '-'}</td>
                                <td class="text-end">${data.july > 0 ? formatCurrency(data.july) : '-'}</td>
                                <td class="text-end">${data.august > 0 ? formatCurrency(data.august) : '-'}</td>
                                <td class="text-end">${data.september > 0 ? formatCurrency(data.september) : '-'}</td>
                                <td class="text-end">${data.october > 0 ? formatCurrency(data.october) : '-'}</td>
                                <td class="text-end">${data.november > 0 ? formatCurrency(data.november) : '-'}</td>
                                <td class="text-end">${data.december > 0 ? formatCurrency(data.december) : '-'}</td>
                            </tr>
                        </tbody>
                    </table>
                `;

                row.child(childRowContent).show();
                tr.addClass('shown');
            }
        });

        // Detay butonu için stil ekle
        const style = document.createElement('style');
        style.textContent = `
            td.details-control {
                text-align: center;
                cursor: pointer;
            }
            td.details-control:before {
                content: '▶';
                display: inline-block;
                font-size: 12px;
                color: #3498db;
                transition: transform 0.2s;
            }
            tr.shown td.details-control:before {
                content: '▼';
                transform: rotate(90deg);
            }
        `;
        document.head.appendChild(style);
    }

    // Tabloyu doldur (helper)
    function populateTable(data) {
        if (!dataTable) {
            // DataTable henüz init edilmedi, init edilecek
            return;
        }

        // DataTable'ı verilerle güncelle
        dataTable.clear();
        dataTable.rows.add(data);
        dataTable.draw();

        updateTotals();
    }

    // Toplamları güncelle
    function updateTotals() {
        const totals = {
            january: 0, february: 0, march: 0, april: 0, may: 0, june: 0,
            july: 0, august: 0, september: 0, october: 0, november: 0, december: 0, grandTotal: 0
        };

        sourceYearData.forEach(item => {
            totals.january += item.january || 0;
            totals.february += item.february || 0;
            totals.march += item.march || 0;
            totals.april += item.april || 0;
            totals.may += item.may || 0;
            totals.june += item.june || 0;
            totals.july += item.july || 0;
            totals.august += item.august || 0;
            totals.september += item.september || 0;
            totals.october += item.october || 0;
            totals.november += item.november || 0;
            totals.december += item.december || 0;
            totals.grandTotal += item.total || 0;
        });

        // Update footer with null checks
        const updateElement = (id, value) => {
            const el = document.getElementById(id);
            if (el) el.textContent = formatCurrency(value);
        };

        updateElement('sumJanuary', totals.january);
        updateElement('sumFebruary', totals.february);
        updateElement('sumMarch', totals.march);
        updateElement('sumApril', totals.april);
        updateElement('sumMay', totals.may);
        updateElement('sumJune', totals.june);
        updateElement('sumJuly', totals.july);
        updateElement('sumAugust', totals.august);
        updateElement('sumSeptember', totals.september);
        updateElement('sumOctober', totals.october);
        updateElement('sumNovember', totals.november);
        updateElement('sumDecember', totals.december);
        updateElement('sumGrandTotal', totals.grandTotal);
    }

    // Ay sütunlarının görünürlüğünü güncelle
    function updateMonthColumnsVisibility() {
        // Checkbox flag değerlerini topla ve ay numaralarına çevir
        const selectedMonthsFlag = Array.from(document.querySelectorAll('.month-checkbox:checked'))
            .reduce((sum, cb) => sum + parseInt(cb.value), 0);

        const selectedMonths = ParseMonthsFlag(selectedMonthsFlag);

        const monthColumns = {
            1: 3, 2: 4, 3: 5, 4: 6, 5: 7, 6: 8,
            7: 9, 8: 10, 9: 11, 10: 12, 11: 13, 12: 14
        };

        // DataTable column index (0-based, so month 1 = column 3)
        for (let month = 1; month <= 12; month++) {
            const columnIndex = monthColumns[month];
            const isVisible = selectedMonths.includes(month);

            if (dataTable) {
                const column = dataTable.column(columnIndex);
                if (isVisible) {
                    column.visible(true);
                } else {
                    column.visible(false);
                }
            }
        }

        // DataTable'ı yeniden çiz
        if (dataTable) {
            dataTable.draw();
        }
    }

    // Flag değerini ay numaralarına çevir
    function ParseMonthsFlag(flag) {
        const months = [];
        const monthFlags = [1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048];
        const monthNumbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

        for (let i = 0; i < monthFlags.length; i++) {
            if ((flag & monthFlags[i]) !== 0) {
                months.push(monthNumbers[i]);
            }
        }

        return months;
    }

    // Para birimi formatla
    function formatCurrency(value) {
        if (value === null || value === undefined) return '0,00';
        return parseFloat(value).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    // Toast mesajı göster
    function showToast(type, title, message) {
        if (typeof toastr !== 'undefined') {
            toastr[type](message, title);
        } else if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: type === 'success' ? 'success' : 'error',
                title: title,
                text: message
            });
        } else {
            alert(`${title}: ${message}`);
        }
    }
});
