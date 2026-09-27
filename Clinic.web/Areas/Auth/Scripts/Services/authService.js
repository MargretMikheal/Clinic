(function () {
    "use strict";

    var app = angular.module("clinicApp");

    app.factory("authService", ["$http", "$q", function ($http, $q) {

        return {
            login: function (loginData) {
                return $http.post("/api/account/login", loginData)
                    .then(function (response) {
                        return response.data;
                    });
            },

            logout: function () {
                return $http.post("/api/account/logout");
            },

            getCurrentUser: function () {
                return $http.get("/api/account/me")
                    .then(function (response) {
                        return response.data;
                    });
            }
        };
    }]);

})();
