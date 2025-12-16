"use strict";

// Class Definition
var KTLogin = function () {
    var _login;

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

    return {
        init: function () {
            _login = $('#kt_login');

            _handleSignInForm();
            _handleForgotForm();
        }
    };
}();

// Class Initialization
jQuery(document).ready(function () {
    KTLogin.init();
});
