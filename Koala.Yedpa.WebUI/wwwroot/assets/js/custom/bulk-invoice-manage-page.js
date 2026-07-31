"use strict";

// Toplu Faturalandırma Yönetimi sayfası
//
// - Oturum listesi (aktarım tarihleri ve sonuç sayıları)
// - Seçilen oturumun aktarım satırları (aktarılacak / aktarılan / başarısız firmalar)
// - "Aktarılacak Verileri Oluştur" → o anki bekleyen AIDAT satırlarını senkronlar (gün içi önizleme)
// - "Eksik Kalanları Yeniden Aktar" → başarısız + hiç denenmemiş satırları tekrar dener
//
// Dashboard'dan ?sessionId=N ile gelindiğinde o oturum otomatik açılır ve liste boşsa
// bir kez senkronlanır (kullanıcı butona basmadan firmaları görebilsin).
var KLBulkInvoiceManage = function () {

    var sessionsTable = null;
    var itemsTable = null;
    var selectedSessionId = null;
    var deepLinkHandled = false;

    // --- Yardımcılar ---
    var pad = function (n) { return ('0' + n).slice(-2); };

    var fmtDate = function (s) {
        if (!s) return '';
        var d = new Date(s);
        if (isNaN(d)) return s;
        return pad(d.getDate()) + '.' + pad(d.getMonth() + 1) + '.' + d.getFullYear();
    };

    var fmtDateTime = function (s) {
        if (!s) return '';
        var d = new Date(s);
        if (isNaN(d)) return s;
        return fmtDate(s) + ' ' + pad(d.getHours()) + ':' + pad(d.getMinutes());
    };

    var money = function (v) { return (parseFloat(v) || 0).toFixed(2) + ' ₺'; };

    var sessionStatusBadge = function (s) {
        switch (s) {
            case 0: return '<span class="label label-light-warning label-inline">Bekliyor</span>';
            case 1: return '<span class="label label-light-primary label-inline">İşleniyor</span>';
            case 2: return '<span class="label label-light-success label-inline">Tamamlandı</span>';
            case 3: return '<span class="label label-light-danger label-inline">Hatalı</span>';
            default: return s;
        }
    };

    var itemStatusBadge = function (s) {
        switch (s) {
            case 0: return '<span class="label label-light-warning label-inline">Gönderilmedi</span>';
            case 1: return '<span class="label label-light-success label-inline">Aktarıldı</span>';
            case 2: return '<span class="label label-light-danger label-inline">Başarısız</span>';
            default: return s;
        }
    };

    // --- Oturumlar ---
    var loadSessions = function () {
        $.ajax({
            url: '/BulkInvoice/Sessions',
            method: 'GET',
            success: function (res) {
                var data = (res && res.isSuccess && res.data) ? res.data : [];
                if (sessionsTable) sessionsTable.destroy();
                sessionsTable = $('#sessionsTable').DataTable({
                    data: data,
                    pageLength: 10,
                    order: [[0, 'desc']],
                    language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json' },
                    columns: [
                        { data: 'id' },
                        { data: 'invoiceDate', render: fmtDate },
                        { data: 'status', render: sessionStatusBadge },
                        { data: 'totalItems', className: 'text-right' },
                        { data: 'completedItems', className: 'text-right' },
                        { data: 'failedItems', className: 'text-right' },
                        { data: 'createdBy' },
                        { data: 'createdAt', render: fmtDateTime }
                    ]
                });

                handleDeepLink();
            },
            error: function () { toastr.error('Oturumlar yüklenemedi'); }
        });
    };

    // Dashboard'dan gelen ?sessionId=N — oturumu bir kez otomatik aç.
    var handleDeepLink = function () {
        if (deepLinkHandled) return;
        var id = window.__bulkInvoiceSessionId;
        if (!id) return;

        deepLinkHandled = true;
        selectedSessionId = id;
        loadItems(id, true);
    };

    // --- Satırlar ---
    // autoPrepare: liste boşsa bir kez senkronla (deep-link ile gelindiğinde).
    var loadItems = function (sessionId, autoPrepare) {
        $.ajax({
            url: '/BulkInvoice/Items?sessionId=' + sessionId,
            method: 'GET',
            success: function (res) {
                var data = (res && res.isSuccess && res.data) ? res.data : [];

                if (autoPrepare && data.length === 0) {
                    prepareItems(sessionId, true);
                    return;
                }

                renderItems(sessionId, data);
            },
            error: function () { toastr.error('Satırlar yüklenemedi'); }
        });
    };

    // Seçili oturumun özet bilgisi (çıktı başlığında kullanılır).
    var getSessionInfo = function (sessionId) {
        if (!sessionsTable) return null;
        var found = null;
        sessionsTable.rows().data().each(function (row) {
            if (row && row.id === sessionId) found = row;
        });
        return found;
    };

    // Çıktı başlığı: "Aktarım Yapılacak Firmalar — 03.08.2026 (25 firma, toplam 12.345,00 ₺)"
    var buildExportTitle = function (sessionId, rowCount, total) {
        var info = getSessionInfo(sessionId);
        var datePart = info ? fmtDate(info.invoiceDate) : ('Oturum #' + sessionId);
        return 'Aktarım Yapılacak Firmalar — ' + datePart +
            ' (' + rowCount + ' firma, toplam ' + money(total) + ')';
    };

    var AMOUNT_COL = 3; // Tutar sütunu

    // Hücreyi çıktıya hazırlar: HTML etiketlerini at, tutardan para simgesini kaldır.
    // format.body override edilince DataTables'ın kendi strip/decode adımı devre dışı kalır,
    // o yüzden ikisini de burada yapıyoruz.
    // NOT: &lt; / &gt; bilerek ÇÖZÜLMEZ — yazdırma penceresi hücreleri ham HTML olarak
    // birleştirdiği için, REST hata metnindeki açılı parantezler markup'a dönüşmemeli.
    var formatExportCell = function (inner, rowIdx, colIdx) {
        var v = String(inner == null ? '' : inner).replace(/<[^>]*>/g, '');

        v = v.replace(/&nbsp;/g, ' ')
             .replace(/&quot;/g, '"')
             .replace(/&#39;/g, "'")
             .replace(/&amp;/g, '&')
             .trim();

        if (colIdx === AMOUNT_COL) {
            // "1234.56 ₺" → "1234.56" : Excel sayısal hücre olarak yazsın, toplam alınabilsin.
            v = v.replace('₺', '').trim();
        }
        return v;
    };

    var renderItems = function (sessionId, data) {
        if (itemsTable) itemsTable.destroy();
        $('#itemsExportButtons').empty();

        var total = data.reduce(function (sum, i) { return sum + (parseFloat(i.amount) || 0); }, 0);
        var exportTitle = buildExportTitle(sessionId, data.length, total);

        var exportOptions = {
            columns: ':visible',
            format: { body: formatExportCell }
        };

        itemsTable = $('#itemsTable').DataTable({
            data: data,
            pageLength: 25,
            language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json' },
            // language.url asenkron yüklenir → tablo (ve Buttons) init'i ertelenir.
            // Bu yüzden butonlar initComplete'te taşınır, init çağrısının hemen ardından DEĞİL.
            initComplete: function () {
                $('#itemsExportButtons').empty();
                this.api().buttons().container().appendTo('#itemsExportButtons');
            },
            columns: [
                { data: 'clientCode' },
                { data: 'clientName' },
                { data: 'monthName' },
                { data: 'amount', className: 'text-right', render: money },
                { data: 'status', className: 'text-center', render: itemStatusBadge },
                { data: 'retryCount', className: 'text-center' },
                { data: 'logoInvoiceRef', render: function (v) { return v || ''; } },
                { data: 'note', render: function (v) { return v || ''; } },
                { data: 'restError', render: function (v) { return v ? $('<div>').text(v).html() : ''; } }
            ],
            buttons: [
                {
                    extend: 'excelHtml5',
                    text: '<i class="far fa-file-excel"></i> Excel',
                    className: 'btn btn-sm btn-light-success',
                    titleAttr: 'Tabloyu Excel olarak indir',
                    title: exportTitle,
                    filename: function () {
                        var info = getSessionInfo(sessionId);
                        var d = info ? fmtDate(info.invoiceDate).replace(/\./g, '-') : ('oturum-' + sessionId);
                        return 'Aktarim-Yapilacak-Firmalar_' + d;
                    },
                    exportOptions: exportOptions
                },
                {
                    extend: 'print',
                    text: '<i class="fas fa-print"></i> Yazdır',
                    className: 'btn btn-sm btn-light-primary',
                    titleAttr: 'Tabloyu yazdır',
                    title: exportTitle,
                    // Yazdırma yeni bir pencerede yalnızca tabloyu açar; menü, kartlar,
                    // butonlar gibi uygulama alanları çıktıya GİRMEZ.
                    exportOptions: exportOptions,
                    customize: function (win) {
                        var $doc = $(win.document.body);

                        $doc.find('h1').css({
                            'font-size': '14pt',
                            'font-family': 'Arial, sans-serif',
                            'margin-bottom': '12px'
                        });

                        $doc.find('table')
                            .addClass('compact')
                            .css({
                                'font-size': '9pt',
                                'font-family': 'Arial, sans-serif',
                                'border-collapse': 'collapse',
                                'width': '100%'
                            });

                        $doc.find('table th, table td').css({
                            'border': '1px solid #999',
                            'padding': '4px 6px'
                        });

                        $doc.find('table th').css({
                            'background-color': '#f2f2f2',
                            'text-align': 'left'
                        });

                        // 9 sütun dar sığmıyor → yatay sayfa.
                        $(win.document.head).append(
                            '<style>@page { size: landscape; margin: 10mm; }</style>');
                    }
                }
            ]
        });

        $('#itemsCard').show();
        $('#itemsSubtitle').text('Oturum #' + sessionId + ' — ' + data.length + ' firma, toplam ' + money(total));
    };

    var prepareItems = function (sessionId, silent) {
        $.ajax({
            url: '/BulkInvoice/PrepareItems?sessionId=' + sessionId,
            method: 'POST',
            success: function (res) {
                if (res && res.isSuccess) {
                    if (!silent) toastr.success((res.data || 0) + ' satır aktarıma hazır.');
                    loadItems(sessionId, false);
                    if (!silent) loadSessions();
                } else {
                    toastr.error((res && res.message) || 'Aktarılacak veriler oluşturulamadı');
                }
            },
            error: function () { toastr.error('Aktarılacak veriler oluşturulamadı'); }
        });
    };

    var retryFailed = function (sessionId) {
        $.ajax({
            url: '/BulkInvoice/RetryFailed?sessionId=' + sessionId,
            method: 'POST',
            success: function (res) {
                if (res && res.isSuccess) {
                    toastr.success('Yeniden aktarım başlatıldı. Tamamlanınca rapor maili gelecek.');
                } else {
                    toastr.error((res && res.message) || 'Yeniden aktarım başlatılamadı');
                }
            },
            error: function () { toastr.error('Yeniden aktarım başlatılamadı'); }
        });
    };

    // --- Olaylar ---
    var initEvents = function () {
        $(document).on('click', '#sessionsTable tbody tr', function () {
            var row = sessionsTable.row(this).data();
            if (!row) return;
            selectedSessionId = row.id;
            loadItems(selectedSessionId, false);
            $('html, body').animate({ scrollTop: $('#itemsCard').offset().top - 80 }, 300);
        });

        $('#btnPrepareItems').on('click', function () {
            if (!selectedSessionId) { toastr.info('Önce bir oturum seçin'); return; }
            prepareItems(selectedSessionId, false);
        });

        $('#btnRetryFailed').on('click', function () {
            if (!selectedSessionId) { toastr.info('Önce bir oturum seçin'); return; }
            if (!confirm('Bu oturumda eksik kalan (başarısız + hiç denenmemiş) tüm satırlar yeniden aktarılacak. Onaylıyor musunuz?')) return;
            retryFailed(selectedSessionId);
        });

        $('#btnRefreshSessions').on('click', loadSessions);
    };

    return {
        init: function () {
            initEvents();
            loadSessions();
        }
    };
}();

jQuery(document).ready(function () {
    KLBulkInvoiceManage.init();
});
