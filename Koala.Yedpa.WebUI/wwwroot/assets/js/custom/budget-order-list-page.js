// Budget Order List Page JavaScript
document.addEventListener('DOMContentLoaded', function () {
    const table = document.getElementById('BudgetOrderTable');

    if (table) {
        // DataTable başlat
        const dataTable = $(table).DataTable({
            ajax: {
                url: '/api/BudgetRatioApi',
                type: 'GET',
                dataSrc: function (json) {
                    // API'den gelen veriyi DataTable formatına çevir
                    console.log('API Response:', json);
                    if (json.isSuccess && json.data) {
                        return json.data.map(item => ({
                            year: item.year,
                            budgetType: item.buggetType === 1 ? 'Bütçe' : 'Ek Bütçe',
                            ratio: (item.ratio * 100).toFixed(2) + '%',
                            totalBudget: item.totalBugget.toLocaleString('tr-TR', { minimumFractionDigits: 2 }) + ' TL',
                            status: item.status === 6 ? 'Aktarım Bekleniyor' : 'Kilitli',
                            statusClass: item.status === 6 ? 'badge-warning' : 'badge-success',
                            statusCode: item.status,
                            id: item.id,
                            code: item.code
                        }));
                    }
                    console.log('No data found');
                    return [];
                }
            },
            columns: [
                { data: 'year' },
                { data: 'budgetType' },
                { data: 'ratio' },
                { data: 'totalBudget' },
                {
                    data: 'status',
                    render: function (data, type, row) {
                        return `<span class="badge ${row.statusClass}">${data}</span>`;
                    }
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        // Pending (6) durumu: Güncelle ve Aktar butonları
                        if (row.statusCode === 6) {
                            return `
                                <a href="/BudgetOrder/Update/${row.id}" class="btn btn-sm btn-light-primary" title="Güncelle">
                                    <i class="ki ki-pencil"></i> Güncelle
                                </a>
                                <a href="/BudgetOrder/Transfer/${row.id}" class="btn btn-sm btn-light-success" title="Aktar">
                                    <i class="ki ki-arrow-next"></i> Aktar
                                </a>
                            `;
                        }
                        // Kilitli (1) durumu: İncele ve Görüntüle butonları
                        else if (row.statusCode === 1) {
                            return `
                                <a href="/BudgetOrder/Review/${row.id}" class="btn btn-sm btn-light-warning" title="İncele">
                                    <i class="ki ki-search"></i> İncele
                                </a>
                                <a href="/BudgetOrder/Details/${row.id}" class="btn btn-sm btn-light-info" title="Görüntüle">
                                    <i class="ki ki-eye"></i> Görüntüle
                                </a>
                            `;
                        }
                        // Diğer durumlar: Sadece Görüntüle
                        else {
                            return `
                                <a href="/BudgetOrder/Details/${row.id}" class="btn btn-sm btn-light-info" title="Görüntüle">
                                    <i class="ki ki-eye"></i> Görüntüle
                                </a>
                            `;
                        }
                    }
                }
            ],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json'
            },
            order: [[0, 'desc']]
        });
    }

    // DataTable yenileme fonksiyonu
    function refreshTable() {
        if (typeof dataTable !== 'undefined') {
            dataTable.ajax.reload(null, false);
        }
    }

    // Sayfa yüklendiğinde verileri yükle
    refreshTable();
});
