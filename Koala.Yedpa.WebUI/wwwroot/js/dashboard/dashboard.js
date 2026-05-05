$(function () {
    var grid = GridStack.init({
        column: 12,
        cellHeight: 70,
        minRow: 2,
        margin: 8,
        animate: true,
        float: false,
        resizable: { handles: 'se, sw' },
        draggable: { handle: '.widget-header' }
    });

    window._dashboardGrid = grid;

    // Debounced save
    var saveTimeout;
    function debouncedSave() {
        clearTimeout(saveTimeout);
        saveTimeout = setTimeout(saveLayout, 800);
    }

    grid.on('change', function () {
        debouncedSave();
    });

    // Remove widget button
    $(document).on('click', '.widget-remove', function () {
        var widgetId = $(this).data('widget-id');
        var items = grid.getGridItems();
        for (var i = 0; i < items.length; i++) {
            if ($(items[i]).data('gs-id') === widgetId) {
                grid.removeWidget(items[i]);
                break;
            }
        }
        $('.widget-toggle[data-widget-id="' + widgetId + '"]').prop('checked', false);
        debouncedSave();
    });

    // Sidebar toggle
    $('#btnToggleSidebar').on('click', function () {
        $('#widgetSidebar').toggleClass('open');
    });
    $('#btnCloseSidebar').on('click', function () {
        $('#widgetSidebar').removeClass('open');
    });

    // Widget visibility toggle
    $(document).on('change', '.widget-toggle', function () {
        var widgetId = $(this).data('widget-id');
        var isChecked = $(this).is(':checked');
        if (isChecked) {
            addWidgetToGrid(widgetId);
        } else {
            removeWidgetFromGrid(widgetId);
        }
        debouncedSave();
    });

    function addWidgetToGrid(widgetId) {
        var allWidgets = JSON.parse($('#allWidgetsData').val());
        var widgetDef = allWidgets.find(function (w) { return w.Id === widgetId; });
        if (!widgetDef) return;

        var html = '<div class="grid-stack-item" data-gs-id="' + widgetId + '" data-gs-w="' + widgetDef.Width + '" data-gs-h="' + widgetDef.Height + '" data-gs-min-w="3" data-gs-min-h="2">' +
            '<div class="grid-stack-item-content">' +
            '<div class="widget-card" id="widget-' + widgetId + '">' +
            '<div class="widget-header">' +
            '<span class="widget-title">' + widgetDef.Title + '</span>' +
            '<button type="button" class="btn btn-icon btn-xs btn-light btn-circle widget-remove" data-widget-id="' + widgetId + '"><i class="fas fa-times icon-xs"></i></button>' +
            '</div>' +
            '<div class="widget-body"><div class="widget-loading"><i class="fas fa-spinner fa-spin mr-2"></i> Yukleniyor...</div></div>' +
            '</div></div></div>';

        grid.addWidget(html);
        if (typeof loadWidget === 'function') loadWidget(widgetId);
    }

    function removeWidgetFromGrid(widgetId) {
        var items = grid.getGridItems();
        for (var i = 0; i < items.length; i++) {
            if ($(items[i]).data('gs-id') === widgetId) {
                grid.removeWidget(items[i]);
                break;
            }
        }
    }

    // Reset layout
    $('#btnResetLayout').on('click', function () {
        Swal.fire({
            title: 'Layout Sifirla',
            text: 'Varsayilan layout\'a donmek istediginize emin misiniz?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Evet, Sifirla',
            cancelButtonText: 'Iptal',
            confirmButtonColor: '#F64E60'
        }).then(function (result) {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Dashboard/ResetLayout',
                    method: 'POST',
                    success: function () {
                        location.reload();
                    },
                    error: function () {
                        toastr.error('Layout sifirlanamadi');
                    }
                });
            }
        });
    });

    // Save layout to server
    function saveLayout() {
        var items = [];
        var nodes = grid.engine.nodes || [];
        for (var i = 0; i < nodes.length; i++) {
            var node = nodes[i];
            var widgetId = $(node.el).data('gs-id');
            if (!widgetId) continue;
            items.push({
                WidgetId: widgetId,
                GridX: typeof node.x === 'number' ? node.x : 0,
                GridY: typeof node.y === 'number' ? node.y : 0,
                Width: typeof node.w === 'number' ? node.w : 6,
                Height: typeof node.h === 'number' ? node.h : 3,
                Visible: true
            });
        }

        // Include hidden widgets
        $('.widget-toggle:not(:checked)').each(function () {
            items.push({
                WidgetId: $(this).data('widget-id'),
                GridX: 0, GridY: 0, Width: 6, Height: 3, Visible: false
            });
        });

        $.ajax({
            url: '/Dashboard/SaveLayout',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(items),
            error: function () {
                toastr.error('Layout kaydedilemedi');
            }
        });
    }
});
