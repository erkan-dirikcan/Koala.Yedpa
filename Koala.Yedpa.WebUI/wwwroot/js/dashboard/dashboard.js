(function () {
    var chartColors = {
        primary: '#3699FF',
        success: '#1BC5BD',
        warning: '#FFA800',
        danger: '#F64E60',
        info: '#8950FC',
        gray: '#B5B5C3'
    };

    var _saveTimer = null;

    // --- Collect all widget states from sidebar toggles ---
    function collectWidgetStates() {
        var items = [];
        var toggles = document.querySelectorAll('.widget-toggle');
        toggles.forEach(function (el) {
            var widgetId = el.getAttribute('data-widget-id');
            var isChecked = el.checked;
            var widthEl = document.querySelector('.widget-width-select[data-widget-id="' + widgetId + '"]');
            var width = widthEl ? parseInt(widthEl.value) : 6;
            items.push({
                widgetId: widgetId,
                width: width,
                visible: isChecked,
                sortOrder: items.length
            });
        });
        return items;
    }

    // --- POST widget states to server ---
    function postLayout(items, onSuccess, onError) {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', '/Dashboard/SaveLayout', true);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.onload = function () {
            if (xhr.status >= 200 && xhr.status < 300) {
                if (onSuccess) onSuccess();
            } else {
                if (onError) onError();
            }
        };
        xhr.onerror = function () {
            if (onError) onError();
        };
        xhr.send(JSON.stringify(items));
    }

    // --- SortableJS Initialization ---
    function initSortable() {
        var container = document.getElementById('dashboardWidgets');
        if (!container || typeof Sortable === 'undefined') return;

        Sortable.create(container, {
            animation: 250,
            handle: '.card-header',
            ghostClass: 'widget-ghost',
            chosenClass: 'widget-chosen',
            dragClass: 'widget-drag',
            forceFallback: true,
            fallbackClass: 'widget-fallback',
            fallbackOnBody: true,
            swapThreshold: 0.65,
            onEnd: function () {
                if (_saveTimer) clearTimeout(_saveTimer);
                _saveTimer = setTimeout(function () {
                    var items = collectWidgetStates();
                    postLayout(items,
                        function () { toastr.success('Layout kaydedildi'); },
                        function () { toastr.error('Layout kaydedilemedi'); }
                    );
                }, 800);
            }
        });
    }

    // --- Sidebar ---
    function initSidebar() {
        document.getElementById('btnToggleSidebar').addEventListener('click', function () {
            document.getElementById('widgetSidebar').classList.toggle('open');
        });
        document.getElementById('btnCloseSidebar').addEventListener('click', function () {
            document.getElementById('widgetSidebar').classList.remove('open');
        });

        document.getElementById('btnSaveWidgets').addEventListener('click', function () {
            var btn = this;
            btn.disabled = true;
            btn.innerHTML = '<i class="fas fa-spinner fa-spin mr-1"></i> Kaydediliyor...';

            var items = collectWidgetStates();
            console.log('Saving widgets:', JSON.stringify(items));

            postLayout(items,
                function () {
                    location.reload();
                },
                function () {
                    btn.disabled = false;
                    btn.innerHTML = '<i class="fas fa-save mr-1"></i> Kaydet';
                    toastr.error('Kaydedilemedi');
                }
            );
        });
    }

    // --- Reset Layout ---
    function initResetLayout() {
        document.getElementById('btnResetLayout').addEventListener('click', function () {
            Swal.fire({
                title: 'Layout Sıfırla',
                text: 'Varsayılan layout\'a dönmek istediğinize emin misiniz?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Evet, Sıfırla',
                cancelButtonText: 'İptal',
                confirmButtonColor: '#F64E60'
            }).then(function (result) {
                if (result.isConfirmed) {
                    var xhr = new XMLHttpRequest();
                    xhr.open('POST', '/Dashboard/ResetLayout', true);
                    xhr.onload = function () { location.reload(); };
                    xhr.send();
                }
            });
        });
    }

    // --- Chart.js Initialization ---
    function initCharts() {
        var data = window.__dashboardData;
        if (!data) return;

        var visible = data.visibleWidgets || [];

        if (visible.indexOf('W4') !== -1 && data.topDebtors && data.topDebtors.length > 0) {
            var ctx4 = document.getElementById('chartBalanceDist');
            if (ctx4) {
                new Chart(ctx4, {
                    type: 'bar',
                    data: {
                        labels: data.topDebtors.map(function (c) { return c.name; }),
                        datasets: [{ label: 'Bakiye (TL)', data: data.topDebtors.map(function (c) { return c.balance; }), backgroundColor: chartColors.primary }]
                    },
                    options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
                });
            }
        }

        if (visible.indexOf('W5') !== -1) {
            var ctx5 = document.getElementById('chartMonthlyTrend');
            if (ctx5) {
                var months = ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'];
                new Chart(ctx5, {
                    type: 'line',
                    data: {
                        labels: months,
                        datasets: [{ label: 'Tahsilat (TL)', data: data.monthlyData || [], borderColor: chartColors.primary, backgroundColor: 'rgba(54,153,255,0.1)', fill: true, tension: 0.4 }]
                    },
                    options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
                });
            }
        }

        if (visible.indexOf('W7') !== -1) {
            var ctx7 = document.getElementById('chartDues');
            if (ctx7) {
                new Chart(ctx7, {
                    type: 'doughnut',
                    data: {
                        labels: ['Tahsil Edilen', 'Açık'],
                        datasets: [{ data: [data.budgetCollected || 0, data.budgetRemaining || 0], backgroundColor: [chartColors.success, chartColors.danger] }]
                    },
                    options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }
                });
            }
        }

        if (visible.indexOf('W9') !== -1 && data.yearlyBudgetData && data.yearlyBudgetData.length > 0) {
            var ctx9 = document.getElementById('chartYearlyBudget');
            if (ctx9) {
                new Chart(ctx9, {
                    type: 'bar',
                    data: {
                        labels: data.yearlyBudgetData.map(function (y) { return y.year; }),
                        datasets: [
                            { label: 'Bütçe', data: data.yearlyBudgetData.map(function (y) { return y.budget; }), backgroundColor: chartColors.primary },
                            { label: 'Tahsilat', data: data.yearlyBudgetData.map(function (y) { return y.collected; }), backgroundColor: chartColors.success }
                        ]
                    },
                    options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
                });
            }
        }
    }

    // --- Load Aidat Tahsilat Widget Data ---
    function loadAidatTahsilatWidget() {
        var widgetContainer = document.getElementById('aidatTahsilatWidget');
        if (!widgetContainer) return;

        var cardBody = widgetContainer.closest('.card-body');
        if (!cardBody) return;

        var currentYear = new Date().getFullYear();
        var currentMonth = new Date().getMonth() + 1;

        // Show loading
        widgetContainer.innerHTML = '<div class="widget-loading"><i class="fas fa-spinner fa-spin mr-2"></i> Yükleniyor...</div>';

        var xhr = new XMLHttpRequest();
        xhr.open('GET', '/api/Dashboard/aidat-tahsilat?year=' + currentYear + '&month=' + currentMonth, true);
        xhr.onload = function () {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    var response = JSON.parse(xhr.responseText);
                    if (response.isSuccess && response.data) {
                        var data = response.data;
                        var toplamAlacak = data.toplamAlacak || 0;
                        var odenen = data.odenen || 0;
                        var bekleyen = data.bekleyen || 0;

                        var odenenYuzde = toplamAlacak > 0 ? ((odenen / toplamAlacak) * 100).toFixed(1) : 0;
                        var bekleyenYuzde = toplamAlacak > 0 ? ((bekleyen / toplamAlacak) * 100).toFixed(1) : 0;

                        var html = '<div class="kpi-row">' +
                            '<div class="kpi-item kpi-primary">' +
                            '<div class="kpi-value">' + formatCurrency(toplamAlacak) + ' ₺</div>' +
                            '<div class="kpi-label">Toplam Alacak</div>' +
                            '<div class="kpi-sublabel">' + data.ay + ' ' + data.yil + '</div>' +
                            '</div>' +
                            '<div class="kpi-item kpi-success">' +
                            '<div class="kpi-value">' + formatCurrency(odenen) + ' ₺</div>' +
                            '<div class="kpi-label">Ödenen</div>' +
                            '<div class="kpi-sublabel">%' + odenenYuzde + '</div>' +
                            '</div>' +
                            '<div class="kpi-item kpi-warning">' +
                            '<div class="kpi-value">' + formatCurrency(bekleyen) + ' ₺</div>' +
                            '<div class="kpi-label">Bekleyen</div>' +
                            '<div class="kpi-sublabel">%' + bekleyenYuzde + '</div>' +
                            '</div>' +
                            '</div>';

                        widgetContainer.innerHTML = html;
                    } else {
                        showError(widgetContainer, response.message || 'Veri alınamadı');
                    }
                } catch (e) {
                    console.error('Error parsing response:', e);
                    showError(widgetContainer, 'Veri işleme hatası');
                }
            } else {
                showError(widgetContainer, 'Sunucu hatası');
            }
        };
        xhr.onerror = function () {
            showError(widgetContainer, 'Bağlantı hatası');
        };
        xhr.send();
    }

    // Format currency helper
    function formatCurrency(value) {
        return parseFloat(value).toLocaleString('tr-TR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    // Show error helper
    function showError(container, message) {
        container.innerHTML = '<div class="widget-error"><i class="fas fa-exclamation-triangle mr-2"></i>' + message + '</div>';
    }

    // --- Init ---
    document.addEventListener('DOMContentLoaded', function () {
        initSortable();
        initSidebar();
        initResetLayout();
        initCharts();
        loadAidatTahsilatWidget();
    });
})();
