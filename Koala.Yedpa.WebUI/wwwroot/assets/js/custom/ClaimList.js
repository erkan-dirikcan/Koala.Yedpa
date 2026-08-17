'use strict';
var ClaimToRoleSelectList = function () {


    var roleClaimList = function () {
        // Dual Listbox
        var _this = document.getElementById('Claims');

        // Kaynak <select>'teki option'ların title (açıklama) değerlerini value'ya göre sakla.
        // DualListbox plugin'i kendi <li> DOM'unu ürettiği için option'daki title otomatik taşınmıyor.
        var claimDescriptions = {};
        Array.prototype.forEach.call(_this.options, function (option) {
            if (option.title) {
                claimDescriptions[option.value] = option.title;
            }
        });

        // init dual listbox
        var dualListBox = new DualListbox(_this, {
            addEvent: function (value) {
                console.log(value);
            },
            removeEvent: function (value) {
                console.log(value);
            },
            availableTitle: 'Mevcut Yetkiler',
            selectedTitle: 'Seçili Yetkiler',
            addButtonText: 'Ekle',
            removeButtonText: 'Çıkart',
            addAllButtonText: 'Tümünü Ekle',
            removeAllButtonText: 'Tümünü Çıkart'
        });

        // Plugin, her <option> için dataset.id = option.value olan bir <li class="dual-listbox__item">
        // üretiyor (bkz. dual-listbox.js _createListItem). Bu <li> node'ları "Ekle/Çıkart" ile
        // available <-> selected arasında taşınırken aynı DOM node olarak taşınıyor (yeniden
        // oluşturulmuyor), bu yüzden title'ı bir kere set etmek yeterli.
        var claimListItems = (dualListBox.available || []).concat(dualListBox.selected || []);
        claimListItems.forEach(function (listItem) {
            var description = claimDescriptions[listItem.dataset.id];
            if (description) {
                listItem.setAttribute('title', description);
                listItem.setAttribute('data-toggle', 'tooltip');
            }
        });

        if (window.jQuery && dualListBox.dualListbox) {
            $(dualListBox.dualListbox).find('[data-toggle="tooltip"]').tooltip();
        }
    };

    return {
        // public functions
        init: function () {
            roleClaimList();
        },
    };
}();

window.addEventListener('load', function () {
    ClaimToRoleSelectList.init();
});
