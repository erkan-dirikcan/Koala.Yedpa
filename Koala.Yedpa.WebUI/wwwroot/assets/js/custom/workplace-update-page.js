'use strict';
// Class definition
var ClcWorkplaceUpdate = function () {

    // Bütçe alanları
    var budgetMetreFields = [
        'BudgetDepotMetre1', 'BudgetDepotMetre2', 'BudgetGroundMetre',
        'BudgetHungMetre', 'BudgetFloorMetre', 'BudgetPassageMetre1', 'BudgetPassageMetre2'
    ];
    var budgetCoeffFields = [
        'BudgetDepotCoefficient1', 'BudgetDepotCoefficient2', 'BudgetGroundCoefficient',
        'BudgetHungCoefficient', 'BudgetFloorCoefficient', 'BudgetPassageCoefficient1', 'BudgetPassageCoefficient2'
    ];

    // Yakıt alanları
    var fuelMetreFields = [
        'FuelDepotMetre1', 'FuelDepotMetre2', 'FuelGroundMetre',
        'FuelHungMetre', 'FuelFloorMetre', 'FuelPassageMetre1', 'FuelPassageMetre2'
    ];
    var fuelCoeffFields = [
        'FuelDepotCoefficient1', 'FuelDepotCoefficient2', 'FuelGroundCoefficient',
        'FuelHungCoefficient', 'FuelFloorCoefficient', 'FuelPassageCoefficient1', 'FuelPassageCoefficient2'
    ];

    // Güvenli parse fonksiyonu (259,2 → 259.2 yapar, boş/null gelirse 0 döner)
    var safeParse = function (val) {
        if (!val) return 0;
        return parseFloat(String(val).replace(',', '.')) || 0;
    };

    var handleCustomerTypeChange = function () {
        var selectedType = $('input[name="CustomerType"]:checked').val();

        // 1) Tüm kartı göster (Diğer seçiliyse gizleyeceğiz)
        var $card = $('.card-header:contains("Ev Sahibi - Kiracı Bilgileri")').closest('.card');
        $card.show();

        // 2) Sadece bu 3 alan kiracıda gizlensin, tarihler DAİMA görünür kalsın!
        $('#ProfitingOwner').show();
        $('#TaxPayer').show();
        $('#DeedInfo').show();

        $('#ProfitingOwner_lb').show();
        $('#TaxPayer_lb').show();
        $('#DeedInfo_lb').show();

        if (selectedType === '2') { // Kiracı
            $('#ProfitingOwner_lb').hide();
            $('#TaxPayer_lb').hide();
            $('#DeedInfo_lb').hide();

                $('#ProfitingOwner').hide();
                $('#TaxPayer').hide();
                $('#DeedInfo').hide();

            $('#ProfitingOwner, #TaxPayer, #DeedInfo').val('');
        }

        if (selectedType === '4') { // Diğer
            $card.hide();
            $card.find('input, select, textarea').val('').prop('checked', false);
        }
    };

    var calculateTotals = function () {
        let netMetreTotal = 0;
        let fuelTotal = 0;
        let brutCoeffTotal = 0;

        // Net M² = Tüm Bütçe M² toplamı
        budgetMetreFields.forEach(function (field) {
            netMetreTotal += safeParse($(`#${ field }`).val());
        });

        // Katsayılı hesaplamalar
        for (let i = 0; i < budgetMetreFields.length; i++) {
            let metre = safeParse($(`#${ budgetMetreFields[ i ] }`).val());
            let coeff = safeParse($(`#${ budgetCoeffFields[ i ] }`).val()) || 1;
            brutCoeffTotal += (metre * coeff);

            let fuelMetre = safeParse($(`#${ fuelMetreFields[ i ] }`).val());
            let fuelCoeff = safeParse($(`#${ fuelCoeffFields[ i ] }`).val()) || 1;
            fuelTotal += (fuelMetre * fuelCoeff);
        }

        // Türkiye formatı: decimal ayracı virgül
        $('#TotalNetMetre').val(netMetreTotal.toFixed(2).replace('.', ','));
        $('#TotalBrutCoefficientMetre').val(brutCoeffTotal.toFixed(2).replace('.', ','));
        $('#TotalFuelMetre').val(fuelTotal.toFixed(2).replace('.', ','));
    };

    var initEvents = function () {
        // Radio button değişimi
        $(document).on('change', 'input[name="CustomerType"]', function () {
            handleCustomerTypeChange();
            calculateTotals();
        });

        // Herhangi bir input değiştiğinde (özellikle virgüllü giriş için keyup da ekledik)
        $(document).on('input change keyup', 'input', function () {
            var id = this.id;
            if (id && (
                budgetMetreFields.includes(id) || budgetCoeffFields.includes(id) ||
                fuelMetreFields.includes(id) || fuelCoeffFields.includes(id)
            )) {
                calculateTotals();
            }
        });

        // Sayfa yüklendiğinde
        handleCustomerTypeChange();
        calculateTotals();
    };

    return {
        init: function () {
            initEvents();
        }
    };

}();

KTUtil.ready(function () {
    ClcWorkplaceUpdate.init();
});
