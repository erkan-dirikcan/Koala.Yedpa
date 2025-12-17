"use strict";
var ModuleList = function () {

	var initPage = function () {
		var table = $('#Lgxt211Table');

		// begin first table
		table.DataTable({
			responsive: true,
			columnDefs: [

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
	ModuleList.init();
});
