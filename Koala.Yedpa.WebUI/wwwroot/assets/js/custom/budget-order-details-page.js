"use strict";
var DuoStatisticsList = function () {

    var initPage = function () {
        var table = $('#duesStatisticsTable');

        // begin first table
        table.DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json'
            },
            sortable: false,
            responsive: true,
            autoWidth: false,
            search: {
                input: $('#kt_datatable_search_query')
            },

            columnDefs: [
                {
                    targets: 1,
                    width: '300px'
                },
                {
                    targets: 2,
                    className: 'none'
                },
                {
                    targets: 4,
                    className: 'none'
                },
                {
                    targets: 5,
                    className: 'none'
                },
                {
                    targets: 6,
                    className: 'none'
                },
                {
                    targets: 7,
                    className: 'none'
                },
                {
                    targets: 8,
                    className: 'none'
                },
                {
                    targets: 9,
                    className: 'none'
                },
                {
                    targets: 10,
                    className: 'none'
                },
                {
                    targets: 11,
                    className: 'none'
                },
                {
                    targets: 12,
                    className: 'none'
                },
                {
                    targets: 13,
                    className: 'none'
                },
                {
                    targets: 14,
                    className: 'none'
                },
                {
                    targets: 15,
                    className: 'none'
                }
            ]

        });
    };

    return {


        init: function () {
            initPage();
        }

    };

}();



jQuery(document).ready(function () {
    DuoStatisticsList.init();

});
