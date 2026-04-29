"use strict";
var KL{InitName} = function() {

	var {init1} = function() {

	};

	var {init2} = function() {

	};

	return {
		init: function() {
			{init1}();
			{init2}();
		}
	};
}();

jQuery(document).ready(function() {
	KL{InitName}.init();
});
