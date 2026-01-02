"use strict";
var ModuleList = function () {

	var initPage = function () {
		var table = $('#ModuleTable');

		// begin first table
		table.DataTable({
			responsive: true,
			columnDefs: [

			]
		});

		$(".change-status-bt").click(function () {
			var moduleId = $(this).data("moduleid");
			var status = $(this).data("status");
			var name = $(this).data("name");

			var title = status == "Passive" ? name + " İsimli Modül Pasife Çekiliyor" : name +" İsimli Modül Aktif Ediliyor"
			
			Swal.fire({
				title: title,
				text: "Bunu Yapmak İstediğinizden Emin misiniz?",
        icon: "warning",
				showCancelButton: true,
				confirmButtonText: status == "Passive" ? "Evet, Pasife Çek!" :"Evet, Aktif Et!",
				cancelButtonText: "Hayır, vazgeçtim!",
				reverseButtons: true
			}).then(function (result) {
				if (result.value) {
					var model = {
						Id: moduleId,
                        Status: status
					}
					$.post("/Module/ChangeStatus/", {model:model} ).done(function (result) {
						if (result.isSuccess) {
							toastr.success(status == "Passive" ? "Modül başarıyla pasife çekildi":"Modül başarıyla aktif edildi", result.message);
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
						status == 1 ? "Modülün pasife çekilmesi iptal edildi":"Modülün aktif edilmesi iptaql edildi",
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
