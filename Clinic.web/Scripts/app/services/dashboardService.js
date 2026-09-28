(function () {
    "use strict";

    var app = angular.module("clinicApp");

    app.factory("dashboardService", ["$http", function ($http) {

        return {

            // =========================================================
            // Dashboard Data
            // =========================================================

            getAdminDashboard: function () {
                return $http.get("/api/dashboard/admin")
                    .then(function (response) {
                        return response.data;
                    });
            },

            getReceptionistDashboard: function () {
                return $http.get("/api/dashboard/receptionist")
                    .then(function (response) {
                        return response.data;
                    });
            },

            getDoctorDashboard: function () {
                return $http.get("/api/dashboard/doctor")
                    .then(function (response) {
                        return response.data;
                    });
            },

        };
    }]);

})();