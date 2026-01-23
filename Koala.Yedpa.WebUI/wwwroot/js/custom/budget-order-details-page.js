// Budget Order Details Page JavaScript
document.addEventListener('DOMContentLoaded', function () {
    const table = document.getElementById('duesStatisticsTable');

    if (table) {
        // DataTable başlat - 10'arlı sayfalama
        const dataTable = $(table).DataTable({
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json'
            },
            order: [[1, 'asc']], // İşyeri Koduna göre sırala (1. sütun)
            columnDefs: [
                { orderable: false, targets: 0 }, // Detay butonu sıralanamaz
                { orderable: false, targets: -1 } // Son sütun (İşlem) sıralanamaz
            ],
            drawCallback: function () {
                // Sayfa değiştiğinde butonları yeniden bağla
                bindRetryButtons();
            }
        });

        // DataTable yüklendiğinde butonları bağla
        bindRetryButtons();

        // Child row toggle - detay satırını aç/kapat
        $(table).on('click', 'td.details-control', function () {
            const tr = $(this).closest('tr');
            const row = dataTable.row(tr);

            if (row.child.isShown()) {
                // Child row kapalıysa aç
                row.child.hide();
                tr.removeClass('shown');
            } else {
                // Child row açıksa kapat
                // Aylık verileri data attribute'lardan al
                const january = tr.data('january');
                const february = tr.data('february');
                const march = tr.data('march');
                const april = tr.data('april');
                const may = tr.data('may');
                const june = tr.data('june');
                const july = tr.data('july');
                const august = tr.data('august');
                const september = tr.data('september');
                const october = tr.data('october');
                const november = tr.data('november');
                const december = tr.data('december');

                // Child row içeriğini oluştur
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
                                <td class="text-end">${january}</td>
                                <td class="text-end">${february}</td>
                                <td class="text-end">${march}</td>
                                <td class="text-end">${april}</td>
                                <td class="text-end">${may}</td>
                                <td class="text-end">${june}</td>
                                <td class="text-end">${july}</td>
                                <td class="text-end">${august}</td>
                                <td class="text-end">${september}</td>
                                <td class="text-end">${october}</td>
                                <td class="text-end">${november}</td>
                                <td class="text-end">${december}</td>
                            </tr>
                        </tbody>
                    </table>
                `;

                row.child(childRowContent).show();
                tr.addClass('shown');
            }
        });
    }

    // Detay butonu için stil ekle
    const style = document.createElement('style');
    style.textContent = `
        td.details-control {
            text-align: center;
            cursor: pointer;
            width: 30px;
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

    // Tekrar aktarım butonlarını bağla
    function bindRetryButtons() {
        document.querySelectorAll('.btn-retry-single').forEach(button => {
            // Zaten event listener varsa kaldır
            button.removeEventListener('click', handleRetryClick);
            // Yeni event listener ekle
            button.addEventListener('click', handleRetryClick);
        });
    }

    // Tekrar aktarım butonu click handler
    async function handleRetryClick() {
        const button = this;
        const id = button.getAttribute('data-id');
        const code = button.getAttribute('data-code');
        const budgetRatioId = button.getAttribute('data-budgetratio-id');

        if (!confirm(`${code} kodlu kaydı yeniden aktarmak istediğinize emin misiniz?`)) {
            return;
        }

        button.disabled = true;
        button.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Aktarılıyor...';

        try {
            const transferResponse = await fetch('/api/BudgetOrderApi/Transfer', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    duesStatisticIds: [id],
                    userId: null,
                    isDebugMode: false,
                    budgetRatioId: budgetRatioId
                })
            });

            const transferData = await transferResponse.json();

            if (!transferData.isSuccess) {
                alert('Aktarım başlatılırken hata: ' + transferData.message);
                button.disabled = false;
                button.innerHTML = '<i class="ki ki-arrow-next"></i> Aktar';
                return;
            }

            alert('Aktarım başlatıldı! Tamamlandığında e-posta ile bilgilendirileceksiniz.');
            location.reload();

        } catch (error) {
            console.error('Aktarım hatası:', error);
            alert('Aktarım başlatılırken bir hata oluştu: ' + error.message);
            button.disabled = false;
            button.innerHTML = '<i class="ki ki-arrow-next"></i> Aktar';
        }
    }

    // Tümünü yeniden aktar butonu
    const retryAllBtn = document.getElementById('retryAllBtn');
    if (retryAllBtn) {
        retryAllBtn.addEventListener('click', async function () {
            if (!confirm('Tüm aktarılmamış kayıtları yeniden aktarmak istediğinize emin misiniz?')) {
                return;
            }

            this.disabled = true;
            this.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Aktarılıyor...';

            try {
                const budgetRatioId = this.getAttribute('data-budgetratio-id');

                // Önce DuesStatistic ID'lerini al
                const response = await fetch(`/api/BudgetOrderApi/GetDuesStatisticIds?budgetRatioId=${budgetRatioId}`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });

                const data = await response.json();

                if (!data.isSuccess || !data.data || data.data.length === 0) {
                    alert('Aktarılacak kayıt bulunamadı!');
                    this.disabled = false;
                    this.innerHTML = '<i class="fas fa-redo"></i> Tüm Aktarılmamış Kayıtları Yeniden Aktar';
                    return;
                }

                // Tümünü aktar
                const transferResponse = await fetch('/api/BudgetOrderApi/Transfer', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        duesStatisticIds: data.data,
                        userId: null,
                        isDebugMode: false,
                        budgetRatioId: budgetRatioId
                    })
                });

                const transferData = await transferResponse.json();

                if (!transferData.isSuccess) {
                    alert('Aktarım başlatılırken hata: ' + transferData.message);
                    this.disabled = false;
                    this.innerHTML = '<i class="fas fa-redo"></i> Tüm Aktarılmamış Kayıtları Yeniden Aktar';
                    return;
                }

                alert('Aktarım başlatıldı! Tamamlandığında e-posta ile bilgilendirileceksiniz.');
                location.reload();

            } catch (error) {
                console.error('Aktarım hatası:', error);
                alert('Aktarım başlatılırken bir hata oluştu: ' + error.message);
                this.disabled = false;
                this.innerHTML = '<i class="fas fa-redo"></i> Tüm Aktarılmamış Kayıtları Yeniden Aktar';
            }
        });
    }
});
