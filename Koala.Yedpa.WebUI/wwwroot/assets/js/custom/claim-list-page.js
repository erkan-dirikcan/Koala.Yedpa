"use strict";
var ModuleList = function () {

	var initPage = function () {
		var table = $('#ClaimTable');

		// begin first table
		table.DataTable({
			responsive: true,
			columnDefs: [

			]
		});

		$(".delete-module-bt").click(function () {
			var id = $(this).data("id");			
			var moduleId = $(this).data("moduleId");			
			var name = $(this).data("name");

			var title = name + " İsimli Hak Siliniyor";
			
			Swal.fire({
				title: title,
				text: "Bunu Yapmak İstediğinizden Emin misiniz?",
        icon: "warning",
				showCancelButton: true,
				confirmButtonText: "Evet, Sil!",
				cancelButtonText: "Hayır, Vazgeçtim!",
				reverseButtons: true
			}).then(function (result) {
				if (result.value) {
					var model = {
						Id: moduleId,
                        Status: status
					}
					$.get("/Claims/DeleteClaim/"+id).done(function (result) {
						if (result.isSuccess) {
							toastr.success("Hak Başarıyla Silindi", result.message);
						} else {
							toastr.error("Bir Hata Oluştu", result.message);
						}
						setTimeout(function () {
							window.location.reload();
						}, 3000);
					});
				} else if (result.dismiss === "cancel") {
					Swal.fire(
						"İptal Edildi",
						"Silme işlemi kullanıcı tarafından iptal edildi",
						"error"
					)
				}
			});


			
		});

	};

	return {


		init: function () {
			initPage();
		}

	};

}();

var Lgxt211List = function () {

	var initPage = function () {
		var table = $('#Lgxt211Table');

		// begin first table
		table.DataTable({
			responsive: true,
			processing: true,
			serverSide: true,
			ajax: {
				url: '/Site/GetPagedList',
				type: 'POST',
				contentType: 'application/json',
				data: function (d) {
					return JSON.stringify(d);
				}
			},
			language: {
				url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json'
			},
			ordering: true,
			paging: true,
			pageLength: 25,
			lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
			columns: [
				{ data: 'logRef', name: 'LogRef' },
				{ data: 'groupCode', name: 'GroupCode' },
				{ data: 'groupName', name: 'GroupName' },
				{ data: 'clientCode', name: 'ClientCode' },
				{ data: 'clientName', name: 'ClientName' },
				{ 
					data: 'customerType', 
					name: 'CustomerType',
					render: function(data, type, row) {
						if (type === 'display') {
							switch(data) {
								case 1: return 'Mal Sahibi';
								case 2: return 'Kiracı';
								case 4: return 'Diğer';
								default: return '-';
							}
						}
						return data;
					}
				},
				{ 
					data: 'begDate', 
					name: 'BegDate',
					render: function(data, type, row) {
						if (type === 'display' && data) {
							var date = new Date(data);
							return date.toLocaleDateString('tr-TR');
						}
						return data || '-';
					}
				},
				{ 
					data: 'endDate', 
					name: 'EndDate',
					render: function(data, type, row) {
						if (type === 'display' && data) {
							var date = new Date(data);
							return date.toLocaleDateString('tr-TR');
						}
						return data || '-';
					}
				},
				{ 
					data: 'totalBrutCoefficientMetre', 
					name: 'TotalBrutCoefficientMetre',
					render: function(data, type, row) {
						if (type === 'display' && data != null) {
							return parseFloat(data).toFixed(2);
						}
						return data || '-';
					}
				},
				{ 
					data: 'totalNetMetre', 
					name: 'TotalNetMetre',
					render: function(data, type, row) {
						if (type === 'display' && data != null) {
							return parseFloat(data).toFixed(2);
						}
						return data || '-';
					}
				},
				{ 
					data: 'totalFuelMetre', 
					name: 'TotalFuelMetre',
					render: function(data, type, row) {
						if (type === 'display' && data != null) {
							return parseFloat(data).toFixed(2);
						}
						return data || '-';
					}
				},
				{ 
					data: 'id', 
					name: 'Actions',
					orderable: false,
					render: function(data, type, row) {
						if (type === 'display') {
							return '<a href="/Site/Update?id=' + data + '" class="btn btn-icon btn-warning btn-sm" data-toggle="tooltip" title="Güncelle" data-theme="dark"><i class="flaticon-edit"></i></a>';
						}
						return data;
					}
				}
			],
			columnDefs: [
				{
					targets: [5, 8, 9, 10, 11],
					orderable: false
				},
				{
					targets: -1,
					responsivePriority: 1
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
	ModuleList.init();
	Lgxt211List.init();
});
