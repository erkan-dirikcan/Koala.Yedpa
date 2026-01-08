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
            order: [[0, 'asc']],
            columnDefs: [
                {
                    targets: [4, 7, 8],
                    type: 'numeric'
                }
            ]
        });
    }

    console.log('Workplace List Page initialized');
});
