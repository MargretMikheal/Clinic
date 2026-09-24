(function () {
    "use strict";

    var app;
    try {
        app = angular.module("clinicApp");
    } catch (e) {
        app = angular.module("clinicApp", []);
    }

    app.filter('i18n', ['localizationService', function (localizationService) {
        var filter = function (key) {
            if (!key) {
                return '';
            }
            return localizationService.instant(key);
        };

        filter.$stateful = true;
        return filter;
    }]);
})();