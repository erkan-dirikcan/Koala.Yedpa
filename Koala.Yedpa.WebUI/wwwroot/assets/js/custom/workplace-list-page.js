// Workplace List Page JavaScript
document.addEventListener('DOMContentLoaded', function () {
    const table = document.getElementById('workplaceTable');

    if (table) {
        // DataTable başlat (Client-side)
        const dataTable = $(table).DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json'
            },
            pageLength: 25,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Tümü"]],
            order: [[1, 'asc']], // Order by 2nd column (İşyeri Kodu) since first is for expand/collapse
            columnDefs: [
                {
                    targets: 0,
                    orderable: false,
                    className: 'details-control'
                },
                {
                    targets: -1, // Son sütun (İşlemler) sıralanamaz
                    orderable: false
                },
                {
                    targets: [5, 8, 9],
                    type: 'numeric'
                }
            ]
        });

        // Child row toggle - detay satırını aç/kapat
        $(table).on('click', 'td.details-control', function () {
            const tr = $(this).closest('tr');
            const row = dataTable.row(tr);

            if (row.child.isShown()) {
                // Child row açıksa kapat
                row.child.hide();
                tr.removeClass('shown');
            } else {
                // Child row kapalıysa aç
                // Current accounts verisini data attribute'tan al
                const currentAccountsJson = tr.attr('data-currentaccounts');
                console.log('CurrentAccounts JSON:', currentAccountsJson); // Debug

                let accounts = [];

                // JSON parse et
                if (currentAccountsJson && currentAccountsJson !== '[]' && currentAccountsJson !== '') {
                    try {
                        accounts = JSON.parse(currentAccountsJson);
                        console.log('Parsed accounts:', accounts); // Debug
                    } catch (e) {
                        console.error('JSON parse error:', e, 'JSON was:', currentAccountsJson);
                        accounts = [];
                    }
                }

                console.log('Final accounts count:', accounts.length); // Debug

                // Child row içeriğini oluştur
                let childRowContent = '';
                if (accounts && accounts.length > 0) {
                    childRowContent = `
                        <table class="table table-sm table-bordered" style="width: 100%; margin: 0;">
                            <thead>
                                <tr style="background-color: #f8f9fa;">
                                    <th>Cari Kodu</th>
                                    <th>Cari Adı</th>
                                    <th>E-posta</th>
                                </tr>
                            </thead>
                            <tbody>
                    `;

                    accounts.forEach(function(account) {
                        childRowContent += `
                            <tr>
                                <td>${account.Code || ''}</td>
                                <td>${account.Definition || ''}</td>
                                <td>${account.EmailAddress || ''}</td>
                            </tr>
                        `;
                    });

                    childRowContent += `
                            </tbody>
                        </table>
                    `;
                } else {
                    childRowContent = `
                        <div style="padding: 10px; color: #6c757d;">
                            Kayıtlı cari hesap bulunamadı.
                        </div>
                    `;
                }

                console.log('Child row content length:', childRowContent.length); // Debug
                console.log('Child row HTML:', childRowContent); // Debug

                // Child row'u göster
                row.child(childRowContent);
                row.child.show();
                tr.addClass('shown');

                console.log('Child row shown'); // Debug
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
            content: '▶';
            transform: rotate(90deg);
        }
    `;
    document.head.appendChild(style);

    console.log('Workplace List Page initialized');

    // ============================================================
    // Bütçe Excel oluştur butonu
    // ============================================================
    const generateBudgetExcelBtn = document.getElementById('generateBudgetExcelBtn');
    console.log('generateBudgetExcelBtn:', generateBudgetExcelBtn);
    if (generateBudgetExcelBtn) {
        console.log('Adding event listener to generateBudgetExcelBtn');
        generateBudgetExcelBtn.addEventListener('click', function() {
            console.log('generateBudgetExcelBtn clicked');
            // Yıl seçim modalı göster
            Swal.fire({
                title: 'Bütçe Excel Oluştur',
                html: `
                    <div class="form-group">
                        <label for="budgetYearExcel">Yıl Seçin:</label>
                        <select id="budgetYearExcel" class="swal2-input" style="width: 200px;">
                            <option value="2025">2025</option>
                            <option value="2026">2026</option>
                            <option value="2027">2027</option>
                        </select>
                    </div>
                    <p style="color: #17a2b8; margin-top: 15px;">
                        <i class="fas fa-info-circle"></i>
                        Bu işlem tüm işyerlerinin cari hesapları için bütçe ödeme planı Excel dosyası oluşturacaktır.
                    </p>
                `,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Oluştur',
                cancelButtonText: 'İptal',
                confirmButtonColor: '#17a2b8',
                cancelButtonColor: '#dc3545',
                showLoaderOnConfirm: true,
                preConfirm: () => {
                    const year = document.getElementById('budgetYearExcel').value;
                    if (!year) {
                        Swal.showValidationMessage('Lütfen bir yıl seçin');
                        return false;
                    }
                    return year;
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    const year = result.value;
                    generateBudgetExcel(year);
                }
            });
        });
    } else {
        console.error('generateBudgetExcelBtn NOT FOUND!');
    }

    // ============================================================
    // Toplu mail gönderme butonu
    // ============================================================
    const sendBulkEmailsBtn = document.getElementById('sendBulkEmailsBtn');
    if (sendBulkEmailsBtn) {
        sendBulkEmailsBtn.addEventListener('click', function() {
            // Yıl seçim modalı göster
            Swal.fire({
                title: 'Bütçe Maili Gönder',
                html: `
                    <div class="form-group">
                        <label for="budgetYear">Yıl Seçin:</label>
                        <select id="budgetYear" class="swal2-input" style="width: 200px;">
                            <option value="2025">2025</option>
                            <option value="2026">2026</option>
                            <option value="2027">2027</option>
                        </select>
                    </div>
                    <p style="color: #f56954; margin-top: 15px;">
                        <i class="fas fa-exclamation-triangle"></i>
                        Bu işlem tüm işyerlerinin cari hesaplarına bütçe ödeme planı maili gönderecektir.
                    </p>
                `,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Gönder',
                cancelButtonText: 'İptal',
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#dc3545',
                showLoaderOnConfirm: true,
                preConfirm: () => {
                    const year = document.getElementById('budgetYear').value;
                    if (!year) {
                        Swal.showValidationMessage('Lütfen bir yıl seçin');
                        return false;
                    }
                    return year;
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    const year = result.value;
                    sendBulkBudgetEmails(year);
                }
            });
        });
    }

    // ============================================================
    // Toplu mail gönderme fonksiyonu
    // ============================================================
    function sendBulkBudgetEmails(year) {
        // Anti-forgery token'ı al
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        if (!token) {
            Swal.fire({
                icon: 'error',
                title: 'Hata',
                text: 'Security token bulunamadı. Sayfayı yenileyip tekrar deneyin.',
                confirmButtonText: 'Tamam'
            });
            return;
        }

        $.ajax({
            url: '/Workplace/SendBulkBudgetEmails',
            type: 'POST',
            data: {
                __RequestVerificationToken: token,
                year: year
            },
            success: function(response) {
                if (response.success) {
                    let detailsHtml = '<div style="text-align: left;">';

                    if (response.data) {
                        detailsHtml += `
                            <p><strong>Toplam İşyeri:</strong> ${response.data.totalWorkplaces || 0}</p>
                            <p><strong>Başarılı:</strong> <span style="color: #28a745;">${response.data.totalEmailsSent || 0}</span></p>
                            <p><strong>Başarısız:</strong> <span style="color: #dc3545;">${response.data.totalEmailsFailed || 0}</span></p>
                        `;

                        if (response.data.failedEmails && response.data.failedEmails.length > 0) {
                            detailsHtml += '<div style="margin-top: 15px;"><strong>Başarısız Olanlar:</strong><ul style="max-height: 200px; overflow-y: auto;">';
                            response.data.failedEmails.forEach(function(fail) {
                                detailsHtml += `<li>${fail.workplaceName} - ${fail.currentAccountName} (${fail.emailAddress}): ${fail.errorMessage || 'Bilinmeyen hata'}</li>`;
                            });
                            detailsHtml += '</ul></div>';
                        }
                    }

                    detailsHtml += '</div>';

                    Swal.fire({
                        icon: 'success',
                        title: 'Başarılı!',
                        html: detailsHtml,
                        confirmButtonText: 'Tamam',
                        width: '600px'
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Hata',
                        text: response.message || 'Mail gönderimi başarısız oldu.',
                        confirmButtonText: 'Tamam'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('Error sending bulk emails:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'Hata',
                    text: 'Bir hata oluştu: ' + (xhr.responseJSON?.message || error),
                    confirmButtonText: 'Tamam'
                });
            }
        });
    }

    // ============================================================
    // Bütçe Excel oluştur fonksiyonu
    // ============================================================
    function generateBudgetExcel(year) {
        console.log('generateBudgetExcel called with year:', year);
        // Anti-forgery token'ı al
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        if (!token) {
            Swal.fire({
                icon: 'error',
                title: 'Hata',
                text: 'Security token bulunamadı. Sayfayı yenileyip tekrar deneyin.',
                confirmButtonText: 'Tamam'
            });
            return;
        }

        console.log('Creating form and submitting...');

        // Form oluştur ve submit et
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Workplace/GenerateBudgetExcel';

        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = token;
        form.appendChild(tokenInput);

        const yearInput = document.createElement('input');
        yearInput.type = 'hidden';
        yearInput.name = 'year';
        yearInput.value = year;
        form.appendChild(yearInput);

        document.body.appendChild(form);
        form.submit();

        // Loading mesajı göster
        Swal.fire({
            title: 'Excel Oluşturuluyor',
            html: 'Lütfen bekleyin...',
            timer: 2000,
            timerProgressBar: true,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    }
});
