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

jQuery(document).ready(function () {
	ModuleList.init();
});
