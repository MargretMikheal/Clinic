(function () {
    "use strict";

    var app = angular.module("clinicApp");

    app.controller("LanguageController", ["$scope", "$window", "locale", function ($scope, $window, locale) {
        $scope.changeLanguage = function (lang) {
            if (lang !== "ar" && lang !== "en") return;
            locale.setLocale(lang);
            $window.location.reload();
        };
    }]);

})();
