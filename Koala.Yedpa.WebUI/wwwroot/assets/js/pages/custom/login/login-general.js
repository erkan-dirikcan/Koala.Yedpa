"use strict";

// Class Definition
var KTLogin = function () {
    var _login;

    // Metronic 7 spinner button convention (bkz. pages/custom/login/login-4.js,
    // pages/custom/login/login-3.js: _buttonSpinnerClasses).
    var _buttonSpinnerClasses = 'spinner spinner-right spinner-white pr-15';

    var _showForm = function (form) {
        var cls = 'login-' + form + '-on';
        var form = 'kt_login_' + form + '_form';

        _login.removeClass('login-forgot-on');
        _login.removeClass('login-signin-on');

        _login.addClass(cls);

        KTUtil.animateClass(KTUtil.getById(form), 'animate__animated animate__backInUp');
    }

    var _handleSignInForm = function () {

        $('#kt_login_forgot').on('click', function (e) {
            e.preventDefault();
            _showForm('forgot');
        });
    }


    var _handleForgotForm = function (e) {

        $('#kt_login_forgot_cancel').on('click', function (e) {
            e.preventDefault();

            _showForm('signin');
        });
    }

    // Cift gonderim korumasi: formun normal (senkron) POST'unu ENGELLEMEZ,
    // sadece butonu devre disi birakip spinner gosterir ki ikinci tik/Enter
    // ayni istegi tekrar tetiklemesin. SMTP gonderimi (ForgetPassword) ve
    // PasswordSignInAsync istek icinde senkron calistigi icin sayfa donup
    // ikinci tik ikinci maili / ikinci giris denemesini tetikleyebiliyordu.
    var _guardDoubleSubmit = function (formId, buttonId) {
        var form = KTUtil.getById(formId);
        var button = KTUtil.getById(buttonId);

        if (!form || !button) {
            return;
        }

        $(form).on('submit', function (e) {
            if ($(form).data('kt-submitting')) {
                e.preventDefault();
                return false;
            }

            $(form).data('kt-submitting', true);
            KTUtil.btnWait(button, _buttonSpinnerClasses, null, true);
        });
    }

    return {
        init: function () {
            _login = $('#kt_login');

            _handleSignInForm();
            _handleForgotForm();

            _guardDoubleSubmit('kt_login_signin_form', 'kt_login_signin_submit');
            _guardDoubleSubmit('kt_login_forgot_form', 'kt_login_forgot_submit');
        }
    };
}();

// Class Initialization
jQuery(document).ready(function () {
    KTLogin.init();
});
