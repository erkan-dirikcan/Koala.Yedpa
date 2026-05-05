var chartColors = {
    primary: '#3699FF',
    success: '#1BC5BD',
    warning: '#FFA800',
    danger: '#F64E60',
    info: '#8950FC',
    gray: '#B5B5C3'
};

function loadWidget(widgetId) {
    var $body = $('#widget-' + widgetId + ' .widget-body');
    if ($body.length === 0) return;

    var loaders = {
        'W1': loadBalanceSummary,
        'W2': loadPendingInvoices,
        'W3': loadOverduePayments,
        'W4': loadBalanceDistribution,
        'W5': loadMonthlyTrend,
        'W6': loadRecentTransactions,
        'W7': loadDuesCollection,
        'W8': loadMonthlyBudget,
        'W9': loadYearlyBudget,
        'W10': loadShopCount
    };

    if (loaders[widgetId]) loaders[widgetId]($body);
}

function widgetError($el, msg) {
    $el.html('<div class="widget-error"><i class="fas fa-exclamation-circle mr-2"></i>' + (msg || 'Veri yuklenemedi') + '</div>');
}

function formatCurrency(val) {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 2 }).format(val || 0);
}

function formatDate(val) {
    if (!val) return '-';
    return new Date(val).toLocaleDateString('tr-TR');
}

// W1: Balance Summary KPI
function loadBalanceSummary($el) {
    $.get('/api/LogoClCardApi/CustomerListWithBalance?perPage=1000').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var data = res.data;
        var totalDebit = data.reduce(function (s, c) { return s + (c.balance > 0 ? c.balance : 0); }, 0);
        var totalCredit = data.reduce(function (s, c) { return s + (c.balance < 0 ? Math.abs(c.balance) : 0); }, 0);
        var net = totalDebit - totalCredit;
        $el.html(
            '<div class="kpi-row">' +
            '<div class="kpi-item kpi-danger"><div class="kpi-value">' + formatCurrency(totalDebit) + '</div><div class="kpi-label">Toplam Alacak</div></div>' +
            '<div class="kpi-item kpi-success"><div class="kpi-value">' + formatCurrency(totalCredit) + '</div><div class="kpi-label">Toplam Borc</div></div>' +
            '<div class="kpi-item kpi-primary"><div class="kpi-value">' + formatCurrency(net) + '</div><div class="kpi-label">Net Bakiye</div></div>' +
            '</div>');
    }).fail(function () { widgetError($el); });
}

// W2: Pending Invoices
function loadPendingInvoices($el) {
    $.get('/api/LogoClCardApi/PendingInvoices?perPage=10').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var data = res.data;
        var total = data.reduce(function (s, i) { return s + i.remainingAmount; }, 0);
        var html = '<div class="kpi-item kpi-danger mb-3" style="border-radius:0.5rem;padding:0.75rem"><div class="kpi-value">' + formatCurrency(total) + '</div><div class="kpi-label">Toplam Bekleyen</div></div>';
        html += '<table class="widget-table"><thead><tr><th>Fatura</th><th>Cari</th><th>Tutar</th><th>Vade</th></tr></thead><tbody>';
        data.forEach(function (inv) {
            var rowClass = inv.remainingDays < 0 ? 'style="color:#F64E60"' : '';
            html += '<tr ' + rowClass + '><td>' + inv.invoiceNumber + '</td><td>' + (inv.customerName || '').substring(0, 20) + '</td><td>' + formatCurrency(inv.remainingAmount) + '</td><td>' + formatDate(inv.dueDate) + '</td></tr>';
        });
        html += '</tbody></table>';
        $el.html(html);
    }).fail(function () { widgetError($el); });
}

// W3: Overdue Payments
function loadOverduePayments($el) {
    $.ajax({
        url: '/api/LogoClCardApi/PendingInvoicesSearch?perPage=100',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({})
    }).done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var overdue = res.data.filter(function (i) { return i.remainingDays < 0; });
        var total = overdue.reduce(function (s, i) { return s + i.remainingAmount; }, 0);
        var html = '<div class="kpi-item kpi-danger" style="border-radius:0.5rem;padding:0.75rem;margin-bottom:0.75rem"><div class="kpi-value">' + formatCurrency(total) + '</div><div class="kpi-label">Vadesi Gecen Toplam (' + overdue.length + ' fatura)</div></div>';
        html += '<table class="widget-table"><thead><tr><th>Fatura</th><th>Cari</th><th>Tutar</th><th>Gecikme</th></tr></thead><tbody>';
        overdue.slice(0, 5).forEach(function (inv) {
            html += '<tr style="color:#F64E60"><td>' + inv.invoiceNumber + '</td><td>' + (inv.customerName || '').substring(0, 20) + '</td><td>' + formatCurrency(inv.remainingAmount) + '</td><td>' + Math.abs(inv.remainingDays) + ' gun</td></tr>';
        });
        html += '</tbody></table>';
        $el.html(html);
    }).fail(function () { widgetError($el); });
}

// W4: Balance Distribution Bar Chart
function loadBalanceDistribution($el) {
    $.get('/api/LogoClCardApi/CustomerListWithBalance?perPage=50').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var sorted = res.data.filter(function (c) { return c.balance > 0; }).sort(function (a, b) { return b.balance - a.balance; }).slice(0, 10);
        var labels = sorted.map(function (c) { return (c.definition || c.code).substring(0, 15); });
        var values = sorted.map(function (c) { return c.balance; });
        $el.html('<canvas id="chartBalanceDist"></canvas>');
        new Chart(document.getElementById('chartBalanceDist'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{ label: 'Bakiye (TL)', data: values, backgroundColor: chartColors.primary }]
            },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
        });
    }).fail(function () { widgetError($el); });
}

// W5: Monthly Trend (placeholder - needs aggregated data)
function loadMonthlyTrend($el) {
    $el.html('<div style="text-align:center;padding:2rem;color:#B5B5C3"><i class="fas fa-chart-line fa-2x mb-2"></i><p>Aylik trend icin veri hazirlaniyor</p></div>');
}

// W6: Recent Transactions (placeholder)
function loadRecentTransactions($el) {
    $el.html('<div style="text-align:center;padding:2rem;color:#B5B5C3"><i class="fas fa-list fa-2x mb-2"></i><p>Son islemler icin veri hazirlaniyor</p></div>');
}

// W7: Dues Collection Doughnut
function loadDuesCollection($el) {
    $.get('/api/DuesStatisticApi/GetMonthlyBudgetSummary').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var d = res.data;
        var collected = d.collectedAmount || d.totalCollected || 0;
        var remaining = d.remainingAmount || d.totalRemaining || 0;
        $el.html('<canvas id="chartDues"></canvas>');
        new Chart(document.getElementById('chartDues'), {
            type: 'doughnut',
            data: {
                labels: ['Tahsil Edilen', 'Acik'],
                datasets: [{ data: [collected, remaining], backgroundColor: [chartColors.success, chartColors.danger] }]
            },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }
        });
    }).fail(function () { widgetError($el); });
}

// W8: Monthly Budget KPI
function loadMonthlyBudget($el) {
    $.get('/api/DuesStatisticApi/GetMonthlyBudgetSummary').done(function (res) {
        if (!res.isSuccess || !res.data) { widgetError($el); return; }
        var d = res.data;
        $el.html(
            '<div class="kpi-row">' +
            '<div class="kpi-item kpi-primary"><div class="kpi-value">' + formatCurrency(d.totalBudget || 0) + '</div><div class="kpi-label">Toplam Butce</div></div>' +
            '<div class="kpi-item kpi-success"><div class="kpi-value">' + formatCurrency(d.collectedAmount || d.totalCollected || 0) + '</div><div class="kpi-label">Tahsil Edilen</div></div>' +
            '<div class="kpi-item kpi-danger"><div class="kpi-value">' + formatCurrency(d.remainingAmount || d.totalRemaining || 0) + '</div><div class="kpi-label">Kalan</div></div>' +
            '</div>');
    }).fail(function () { widgetError($el); });
}

// W9: Yearly Budget Comparison
function loadYearlyBudget($el) {
    $.get('/api/DuesStatisticApi/GetDistinctYears').done(function (res) {
        if (!res.isSuccess || !res.data || res.data.length === 0) { widgetError($el); return; }
        var years = res.data.slice(-3);
        var promises = years.map(function (y) {
            return $.get('/api/DuesStatisticApi/GetByYearAndType?year=' + y);
        });
        $.when.apply($, promises).done(function () {
            var labels = years.map(String);
            var budgets = [];
            var collections = [];
            for (var i = 0; i < arguments.length; i++) {
                var resp = arguments[i][0];
                budgets.push(resp.data ? resp.data.totalBudget || 0 : 0);
                collections.push(resp.data ? resp.data.collectedAmount || resp.data.totalCollected || 0 : 0);
            }
            $el.html('<canvas id="chartYearlyBudget"></canvas>');
            new Chart(document.getElementById('chartYearlyBudget'), {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [
                        { label: 'Butce', data: budgets, backgroundColor: chartColors.primary },
                        { label: 'Tahsilat', data: collections, backgroundColor: chartColors.success }
                    ]
                },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
            });
        });
    }).fail(function () { widgetError($el); });
}

// W10: Shop Count
function loadShopCount($el) {
    $.get('/api/LogoClCardApi/ClCardInfoAll?perPage=1').done(function (res) {
        if (!res.isSuccess) { widgetError($el); return; }
        var count = res.recordsTotal || 0;
        $el.html(
            '<div style="display:flex;align-items:center;justify-content:center;height:100%">' +
            '<div class="kpi-item kpi-primary" style="min-width:200px"><div class="kpi-value" style="font-size:2.5rem">' + count + '</div><div class="kpi-label">Aktif Dukkan</div></div>' +
            '</div>');
    }).fail(function () { widgetError($el); });
}

// Init all visible widgets on page load
$(function () {
    setTimeout(function () {
        var allWidgets = JSON.parse($('#allWidgetsData').val());
        allWidgets.forEach(function (w) {
            if (w.Visible) loadWidget(w.Id);
        });
    }, 300);
});
