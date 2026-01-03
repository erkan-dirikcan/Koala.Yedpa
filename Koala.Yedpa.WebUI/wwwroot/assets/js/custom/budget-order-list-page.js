// Budget Order List Page JavaScript
document.addEventListener('DOMContentLoaded', function () {
    const table = document.getElementById('BudgetOrderTable');

    if (table) {
        // DataTable başlat
        const dataTable = $(table).DataTable({
            ajax: {
                url: '/api/DuesStatisticApi/GetPagedList',
                type: 'POST',
                contentType: 'application/json',
                data: function (d) {
                    return JSON.stringify(d);
                },
                dataSrc: function (json) {
                    // API'den gelen veriyi DataTable formatına çevir
                    if (json.isSuccess && json.data) {
                        return json.data.map(item => ({
                            year: item.year,
                            budgetType: item.budgetType === 1 ? 'Bütçe' : 'Ek Bütçe',
                            total: item.total.toLocaleString('tr-TR', { minimumFractionDigits: 2 }) + ' TL',
                            createdDate: new Date().toLocaleDateString('tr-TR'),
                            status: item.transferStatus === 0 ? 'Beklemede' : item.transferStatus === 1 ? 'Başarılı' : 'Başarısız',
                            statusClass: item.transferStatus === 0 ? 'badge-warning' : item.transferStatus === 1 ? 'badge-success' : 'badge-danger',
                            id: item.id,
                            clientCode: item.clientCode
                        }));
                    }
                    return [];
                }
            },
            columns: [
                { data: 'year' },
                { data: 'budgetType' },
                { data: 'total' },
                { data: 'createdDate' },
                {
                    data: 'status',
                    render: function (data, type, row) {
                        return `<span class="badge ${row.statusClass}">${data}</span>`;
                    }
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        return `
                            <a href="/DuesStatistic/Details/${row.id}" class="btn btn-sm btn-light-primary" title="Detay">
                                <i class="ki ki-eye"></i>
                            </a>
                        `;
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
