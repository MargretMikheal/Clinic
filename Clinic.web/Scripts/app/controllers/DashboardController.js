(function () {
    "use strict";

    var app = angular.module("clinicApp");

    app.controller("DashboardController", ["$scope", "authService", 
        function ($scope, authService) {
        
        $scope.userInfo = null;
        $scope.isLoading = true;
        $scope.errorMessage = "";

        $scope.init = function () {
            $scope.isLoading = true;
            authService.getCurrentUser()
                .then(function (response) {
                    $scope.isLoading = false;
                    if (response.IsSuccess) {
                        $scope.userInfo = response.Data;
                    } else {
                        $scope.errorMessage = response.Message;
                    }
                })
                .catch(function (error) {
                    $scope.isLoading = false;
                    $scope.errorMessage = error.data && error.data.Message ? error.data.Message : "";
                });
        };

        $scope.init();
    }]);

})();
