"use strict";
$(document).ready(function () {
	$("#data-Table-Search").on("keyup", function () {
		var value = $(this).val().toLowerCase();
		$("#data-Table tbody tr").filter(function () {
			$(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
		});
	});
	//$("#data-Table").DataTable();
});


var UserList = function () {

	var initPage = function () {
		var table = $('#RoleTable');

		// begin first table
		table.DataTable({
			responsive: true,
			local: 'tr',
			ordering: false,
			paging: true,
			columnDefs: [
				
			]
		});

		$(".change-status-bt").click(function () {
			var user_id = $(this).data("user_id");
			var status = $(this).data("status");
			var name = $(this).data("name");

			var title = status == "Passive" ? name + " İsimli Kullanıcı Pasife Çekiliyor" : name + " İsimli Kullanıcı Aktif Ediliyor"

			Swal.fire({
				title: title,
				text: "Bunu Yapmak İstediğinizden Emin misiniz?",
				icon: "warning",
				showCancelButton: true,
				confirmButtonText: status == "Passive" ? "Evet, Pasife Çek!" : "Evet, Aktif Et!",
				cancelButtonText: "Hayır, vazgeçtim!",
				reverseButtons: true
			}).then(function (result) {
				if (result.value) {
					var model = {
						Id: user_id,
						Status: status

					}
					$.post("/User/UserChangeStatus/", { model: model }).done(function (result) {
						if (result.isSuccess) {
							toastr.success(status == "Passive" ? "Kullanıcı başarıyla pasife çekildi" : "Kullanıcı başarıyla aktif edildi", result.message);
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
						status == "Passive" ? "Kullanıcının pasife çekilmesi iptal edildi" : "Kullanıcının aktif edilmesi iptal edildi",
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
	UserList.init();
});
/*
PEŞİN~1
30 GÜN~2
30~3
15 GÜN~4
20 GÜN~5
45 GÜN~6
60 GÜN~7

*/