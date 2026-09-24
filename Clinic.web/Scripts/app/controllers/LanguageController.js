(function () {
    "use strict";

    var app;
    try {
        app = angular.module("clinicApp");
    } catch (e) {
        app = angular.module("clinicApp", []);
    }

    app.controller("LanguageController", ["$scope", "$window", "localizationService", function ($scope, $window, localizationService) {
        $scope.changeLanguage = function (lang) {
            if (lang !== "ar" && lang !== "en") return;
            localizationService.setLocale(lang);
            $window.location.reload();
        };
    }]);

})();
