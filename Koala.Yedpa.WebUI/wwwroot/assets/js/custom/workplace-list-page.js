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
});
