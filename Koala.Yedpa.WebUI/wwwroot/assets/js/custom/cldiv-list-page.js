"use strict";

var cldivList = function () {
	var Lgxt211List = function () {

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
			lengthMenu: [ [ 10, 25, 50, 100 ], [ 10, 25, 50, 100 ] ],
			columns: [
				{ data: 'logRef', name: 'LogRef' },
				{ data: 'groupCode', name: 'GroupCode' },
				{ data: 'groupName', name: 'GroupName' },
				{ data: 'clientCode', name: 'ClientCode' },
				{ data: 'clientName', name: 'ClientName' },
				{
					data: 'customerType',
					name: 'CustomerType',
					render: function (data, type, row) {
						if (type === 'display') {
							switch (data) {
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
					render: function (data, type, row) {
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
					render: function (data, type, row) {
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
					render: function (data, type, row) {
						if (type === 'display' && data != null) {
							return parseFloat(data).toFixed(2);
						}
						return data || '-';
					}
				},
				{
					data: 'totalNetMetre',
					name: 'TotalNetMetre',
					render: function (data, type, row) {
						if (type === 'display' && data != null) {
							return parseFloat(data).toFixed(2);
						}
						return data || '-';
					}
				},
				{
					data: 'totalFuelMetre',
					name: 'TotalFuelMetre',
					render: function (data, type, row) {
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
					render: function (data, type, row) {
						if (type === 'display') {
							return '<a href="/Site/Update?id=' + data + '" class="btn btn-icon btn-warning btn-sm" data-toggle="tooltip" title="Güncelle" data-theme="dark"><i class="flaticon-edit"></i></a>';
						}
						return data;
					}
				}
			],
			columnDefs: [
				{
					targets: [ 5, 8, 9, 10, 11 ],
					orderable: false
				},
				{
					targets: -1,
					responsivePriority: 1
				}
			]
		});

		return {
			init: function () {
				Lgxt211List();
			}
		};

	}();

	KTUtil.ready(function () {
		cldivList.init();

		// Senkronizasyon butonu click event
		$(document).on('click', '#senkron', function (e) {
			e.preventDefault();
			
			Swal.fire({
				title: "Logo Senkronizasyon",
				text: "Logo senkronizasyon işlemini başlatmak istediğinizden emin misiniz?",
				icon: "question",
				showCancelButton: true,
				confirmButtonText: "Evet, Başlat!",
				cancelButtonText: "Hayır, Vazgeçtim!",
				reverseButtons: true,
				buttonsStyling: false,
				customClass: {
					confirmButton: "btn font-weight-bold btn-primary",
					cancelButton: "btn font-weight-bold btn-default"
				}
			}).then(function (result) {
				if (result.value) {
					// AJAX ile TriggerLogoSync metodunu çağır
					$.ajax({
						url: '/Site/TriggerLogoSync',
						type: 'POST',
						success: function (response) {
							Swal.fire({
								title: "Başarılı",
								text: response,
								icon: "success",
								buttonsStyling: false,
								confirmButtonText: "Tamam",
								customClass: {
									confirmButton: "btn font-weight-bold btn-primary"
								}
							});
						},
						error: function (xhr, status, error) {
							Swal.fire({
								title: "Hata",
								text: "Senkronizasyon işlemi başlatılırken bir hata oluştu: " + error,
								icon: "error",
								buttonsStyling: false,
								confirmButtonText: "Tamam",
								customClass: {
									confirmButton: "btn font-weight-bold btn-primary"
								}
							});
						}
					});
				} else if (result.dismiss === "cancel") {
					Swal.fire({
						title: "İptal Edildi",
						text: "Senkronizasyon işlemi kullanıcı tarafından iptal edildi",
						icon: "info",
						buttonsStyling: false,
						confirmButtonText: "Tamam",
						customClass: {
							confirmButton: "btn font-weight-bold btn-primary"
						}
					});
				}
			});
		});
	});
}