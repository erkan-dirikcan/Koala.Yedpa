"use strict";

// Dashboard — Toplu Faturalandırma
//
// Akış:
//   1) Ayın 15'inden sonra ve gelecek ay için tarih seçilmemişse  → uyarı bandı çıkar.
//   2) Modalda SADECE tarih seçilir (satır seçimi yoktur; liste önizlemedir).
//   3) Tarih kaydedilince uyarı kalkar, yerine "Aktarım Yapılacak Firmaları Görüntüle" paneli gelir.
//
// Aktarım günü 00:01'de N8N → RabbitMQ tetiğiyle o ana kadar biriken TÜM bekleyen
// AIDAT satırları faturalandırılır.
var KLBulkInvoiceDashboard = function () {

    var pendingLinesTable = null;
    var pendingLinesData = [];

    // --- Yardımcılar ---
    var money = function (v) {
        return (parseFloat(v) || 0).toFixed(2) + ' ₺';
    };

    var pad = function (n) {
        return ('0' + n).slice(-2);
    };

    var formatDate = function (iso) {
        if (!iso) return '';
        var d = new Date(iso);
        if (isNaN(d)) return iso;
        return pad(d.getDate()) + '.' + pad(d.getMonth() + 1) + '.' + d.getFullYear();
    };

    // --- Durum kontrolü: uyarı mı, planlanan aktarım paneli mi? ---
    var loadStatus = function () {
        $.ajax({
            url: '/BulkInvoice/CheckAlert',
            method: 'GET',
            success: function (response) {
                if (!response || !response.isSuccess || !response.data) return;

                var d = response.data;

                // Tarih seçilmemiş → uyarı bandı
                $('#bulkInvoiceAlert').toggleClass('d-none', !d.showAlert);

                // Tarih seçilmiş → planlanan aktarım paneli + firmaları görüntüle butonu
                if (d.showPlannedPanel) {
                    $('#plannedTransferDate').text(formatDate(d.transferDate));
                    $('#btnViewPlannedFirms').attr('href', '/BulkInvoice/Manage?sessionId=' + d.sessionId);
                    $('#bulkInvoicePlanned').removeClass('d-none');
                } else {
                    $('#bulkInvoicePlanned').addClass('d-none');
                }
            },
            error: function () {
                console.error('Toplu faturalandırma durumu alınamadı');
            }
        });
    };

    // --- Önizleme listesi ---
    var loadPendingLines = function () {
        $.ajax({
            url: '/BulkInvoice/GetPendingLines',
            method: 'GET',
            success: function (response) {
                if (response && response.isSuccess) {
                    pendingLinesData = response.data || [];
                    renderPendingLines();
                } else {
                    toastr.error((response && response.message) || 'Veriler yüklenirken hata oluştu');
                }
            },
            error: function () {
                toastr.error('Veriler yüklenirken bir hata oluştu');
            }
        });
    };

    var renderPendingLines = function () {
        if (pendingLinesTable) {
            pendingLinesTable.destroy();
        }

        pendingLinesTable = $('#pendingLinesTable').DataTable({
            data: pendingLinesData,
            pageLength: 25,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json'
            },
            order: [[2, 'desc']], // Tutara göre
            columnDefs: [
                { className: 'text-right', targets: 2 },
                { className: 'text-center', targets: 4 }
            ],
            columns: [
                { data: 'clientCode' },
                {
                    data: 'clientName',
                    render: function (data) {
                        return data && data.length > 40 ? data.substring(0, 40) + '...' : (data || '');
                    }
                },
                { data: 'amount', render: money },
                { data: 'monthName' },
                {
                    data: 'closedStatus',
                    render: function (data) {
                        return (data === 1 || data === '1')
                            ? '<span class="label label-light-success label-inline">ÖDENMİŞ</span>'
                            : '<span class="label label-light-warning label-inline">ÖDENMEMİŞ</span>';
                    }
                }
            ]
        });

        // Aktarılacak toplam — listenin TAMAMI aktarılır, seçim yok.
        var total = pendingLinesData.reduce(function (sum, l) {
            return sum + (parseFloat(l.amount) || 0);
        }, 0);
        $('#pendingTotal').text(money(total));
        $('#pendingCount').text(pendingLinesData.length);
    };

    // --- Modal ---
    var openModal = function () {
        $('#bulkInvoiceModal').modal('show');
        loadPendingLines();
    };

    var initDatepicker = function () {
        $('#invoiceDate').datepicker({
            format: 'dd.mm.yyyy',
            language: 'tr',
            autoclose: true,
            todayHighlight: true,
            startDate: new Date()
        }).on('changeDate change', function () {
            toggleSaveButton();
        });
    };

    var toggleSaveButton = function () {
        var hasDate = $('#invoiceDate').val().trim() !== '';
        $('#btnCreateInvoices').prop('disabled', !hasDate);
    };

    // --- Tarihi kaydet ---
    var saveTransferDate = function () {
        var invoiceDate = $('#invoiceDate').val();
        if (!invoiceDate) {
            toastr.error('Lütfen aktarım tarihi seçin');
            return;
        }

        var parts = invoiceDate.split('.');
        if (parts.length !== 3) {
            toastr.error('Geçersiz tarih formatı. Lütfen dd.MM.yyyy formatında girin.');
            return;
        }

        var $btn = $('#btnCreateInvoices');
        $btn.prop('disabled', true);

        $.ajax({
            url: '/BulkInvoice/CreateSession',
            method: 'POST',
            contentType: 'application/json',
            // Yeni akış: sunucuya YALNIZCA tarih gider; aktarım günü o ayın tüm
            // bekleyen AIDAT satırları çekilir.
            data: JSON.stringify({ invoiceDate: parts[2] + '-' + parts[1] + '-' + parts[0] }),
            success: function (response) {
                if (response && response.isSuccess) {
                    toastr.success(response.message || 'Aktarım tarihi kaydedildi.', '', { timeOut: 8000 });
                    $('#bulkInvoiceModal').modal('hide');
                    loadStatus(); // uyarı kalkar, planlanan aktarım paneli gelir
                } else {
                    toastr.error((response && response.message) || 'Aktarım tarihi kaydedilemedi');
                    $btn.prop('disabled', false);
                }
            },
            error: function () {
                toastr.error('Aktarım tarihi kaydedilirken bir hata oluştu');
                $btn.prop('disabled', false);
            }
        });
    };

    var initEvents = function () {
        $('#openBulkInvoiceModal').on('click', function (e) {
            e.preventDefault();
            openModal();
        });

        $('#btnCreateInvoices').on('click', saveTransferDate);

        $('#invoiceDate').on('keyup', toggleSaveButton);

        $('#bulkInvoiceModal').on('hidden.bs.modal', function () {
            $('#invoiceDate').val('');
            toggleSaveButton();
            if (pendingLinesTable) {
                pendingLinesTable.destroy();
                pendingLinesTable = null;
            }
        });
    };

    return {
        init: function () {
            initDatepicker();
            initEvents();
            loadStatus();
        }
    };
}();

jQuery(document).ready(function () {
    KLBulkInvoiceDashboard.init();
});
