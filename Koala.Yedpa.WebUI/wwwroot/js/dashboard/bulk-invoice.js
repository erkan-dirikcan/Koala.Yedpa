(function () {
    var dataTable = null;
    var pendingLinesData = [];
    var selectedLineIds = new Set();

    // --- Alert Kontrolü ---
    function checkBulkInvoiceAlert() {
        $.ajax({
            url: '/api/BulkInvoice/check-alert',
            method: 'GET',
            success: function (response) {
                if (response.isSuccess && response.data && response.data.showAlert) {
                    $('#bulkInvoiceAlert').removeClass('d-none');
                }
            },
            error: function () {
                console.error('Bulk invoice alert kontrolü başarısız');
            }
        });
    }

    // --- Modal Açma ---
    function openBulkInvoiceModal() {
        $('#bulkInvoiceModal').modal('show');
        loadPendingLines();
    }

    // --- Pending Lines Yükleme ---
    function loadPendingLines() {
        $.ajax({
            url: '/api/BulkInvoice/pending-lines',
            method: 'GET',
            success: function (response) {
                if (response.isSuccess) {
                    pendingLinesData = response.data || [];
                    console.log('Pending lines loaded:', pendingLinesData.length, 'items');
                    initDataTable();
                } else {
                    toastr.error(response.message || 'Veriler yüklenirken hata oluştu');
                }
            },
            error: function (xhr, status, error) {
                console.error('Pending lines load error:', error);
                toastr.error('Veriler yüklenirken bir hata oluştu');
            }
        });
    }

    // --- DataTable Başlatma ---
    function initDataTable() {
        if (dataTable) {
            dataTable.destroy();
        }

        dataTable = $('#pendingLinesTable').DataTable({
            data: pendingLinesData,
            pageLength: 25,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json'
            },
            order: [[3, 'desc']], // Tutara göre sırala
            columnDefs: [
                { orderable: false, targets: 0 }, // Checkbox sıralanamaz
                { className: 'text-end', targets: 3 }, // Tutar sağa dayalı
                { className: 'text-center', targets: 5 } // Durum ortala
            ],
            columns: [
                {
                    data: null,
                    render: function (data, type, row) {
                        return `
                            <div class="form-check form-check-sm form-check-custom form-check-solid">
                                <input class="form-check-input line-checkbox" type="checkbox"
                                       data-id="${row.orflineref}" data-amount="${row.amount}">
                            </div>
                        `;
                    }
                },
                { data: 'clientCode' },
                {
                    data: 'clientName',
                    render: function (data) {
                        return data && data.length > 30 ? data.substring(0, 30) + '...' : data;
                    }
                },
                {
                    data: 'amount',
                    render: function (data) {
                        return parseFloat(data).toFixed(2) + ' ₺';
                    }
                },
                { data: 'monthName' },
                {
                    data: 'closedStatus',
                    render: function (data) {
                        if (data === 1 || data === '1') {
                            return '<span class="badge badge-light-success">ÖDENMİŞ</span>';
                        } else {
                            return '<span class="badge badge-light-warning">ÖDENMEMİŞ</span>';
                        }
                    }
                }
            ],
            drawCallback: function () {
                bindCheckboxEvents();
                updateSelectedTotal();
            }
        });
    }

    // --- Checkbox Event'leri ---
    function bindCheckboxEvents() {
        // Tekil checkbox'lar
        $('.line-checkbox').off('change').on('change', function () {
            const id = $(this).data('id');
            const amount = parseFloat($(this).data('amount'));

            if ($(this).is(':checked')) {
                selectedLineIds.add(id);
            } else {
                selectedLineIds.delete(id);
                $('#selectAllLines').prop('checked', false);
            }

            updateSelectedTotal();
        });

        // Tümünü seç checkbox'ı
        $('#selectAllLines').off('change').on('change', function () {
            const isChecked = $(this).is(':checked');
            $('.line-checkbox').prop('checked', isChecked).trigger('change');
        });
    }

    // --- Toplam Tutar Güncelleme ---
    function updateSelectedTotal() {
        let total = 0;
        $('.line-checkbox:checked').each(function () {
            const amount = parseFloat($(this).data('amount'));
            if (!isNaN(amount)) {
                total += amount;
            }
        });

        $('#selectedTotal').text(total.toFixed(2) + ' ₺');

        // Faturalandır butonu durumunu güncelle
        const hasSelection = selectedLineIds.size > 0;
        const hasDate = $('#invoiceDate').val().trim() !== '';
        $('#btnCreateInvoices').prop('disabled', !(hasSelection && hasDate));
    }

    // --- Datepicker Başlatma ---
    function initDatepicker() {
        $('#invoiceDate').datepicker({
            format: 'dd.mm.yyyy',
            language: 'tr',
            autoclose: true,
            todayHighlight: true,
            startDate: new Date()
        }).on('changeDate', function () {
            updateSelectedTotal();
        });
    }

    // --- Faturalandır Butonu ---
    function createInvoices() {
        const invoiceDate = $('#invoiceDate').val();
        if (!invoiceDate) {
            toastr.error('Lütfen fatura tarihi seçin');
            return;
        }

        if (selectedLineIds.size === 0) {
            toastr.error('Lütfen en az bir satır seçin');
            return;
        }

        // Tarih formatını kontrol et
        const dateParts = invoiceDate.split('.');
        if (dateParts.length !== 3) {
            toastr.error('Geçersiz tarih formatı. Lütfen dd.MM.yyyy formatında girin.');
            return;
        }

        const formattedDate = `${dateParts[2]}-${dateParts[1]}-${dateParts[0]}`; // yyyy-MM-dd

        // Seçili satırları hazırla
        const selectedLines = pendingLinesData.filter(function (line) {
            return selectedLineIds.has(line.orflineref);
        });

        const requestData = {
            invoiceDate: formattedDate,
            selectedLines: selectedLines.map(function (line) {
                return {
                    orficheRef: line.orficheRef,
                    orflineref: line.orflineref,
                    clientCode: line.clientCode,
                    clientName: line.clientName,
                    amount: line.amount,
                    monthName: line.monthName,
                    closedStatus: line.closedStatus,
                    lineNo: line.lineNo
                };
            })
        };

        console.log('Creating bulk invoice session:', requestData);

        // AJAX call
        $.ajax({
            url: '/api/BulkInvoice/create-session',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response) {
                if (response.isSuccess) {
                    toastr.success('Faturalandırma işlemi başarıyla başlatıldı! Tamamlandığında e-posta ile bilgilendirileceksiniz.');
                    $('#bulkInvoiceModal').modal('hide');
                    $('#bulkInvoiceAlert').addClass('d-none');

                    // Formu temizle
                    selectedLineIds.clear();
                    $('#invoiceDate').val('');
                    if (dataTable) {
                        dataTable.destroy();
                        dataTable = null;
                    }
                } else {
                    toastr.error(response.message || 'Faturalandırma başlatılırken hata oluştu');
                }
            },
            error: function (xhr, status, error) {
                console.error('Create session error:', error);
                toastr.error('Faturalandırma başlatılırken bir hata oluştu');
            }
        });
    }

    // --- Init ---
    document.addEventListener('DOMContentLoaded', function () {
        // Test butonu (sadece localhost'ta görünür)
        $('#btnTestBulkInvoice').on('click', function () {
            openBulkInvoiceModal();
        });

        // Alert kontrolü
        checkBulkInvoiceAlert();

        // Modal açma butonu
        $('#openBulkInvoiceModal').on('click', function (e) {
            e.preventDefault();
            openBulkInvoiceModal();
        });

        // Datepicker başlat
        initDatepicker();

        // Faturalandır butonu
        $('#btnCreateInvoices').on('click', createInvoices);

        // Modal kapanınca temizle
        $('#bulkInvoiceModal').on('hidden.bs.modal', function () {
            selectedLineIds.clear();
            $('#invoiceDate').val('');
            $('#selectAllLines').prop('checked', false);
            if (dataTable) {
                dataTable.destroy();
                dataTable = null;
            }
        });
    });
})();