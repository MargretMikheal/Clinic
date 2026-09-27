(function () {
    "use strict";

    var app = angular.module("clinicApp");

    app.controller("LoginController", ["$scope", "$window", "authService", 
        function ($scope, $window, authService) {
        
        $scope.credentials = {
            username: "",
            password: ""
        };

        $scope.isLoading = false;
        $scope.errorMessage = "";
        $scope.showPassword = false;

        $scope.togglePassword = function () {
            $scope.showPassword = !$scope.showPassword;
        };

        $scope.login = function () {
            if ($scope.loginForm.$invalid) {
                return;
            }

            $scope.isLoading = true;
            $scope.errorMessage = "";

            authService.login($scope.credentials)
                .then(function (response) {
                    $scope.isLoading = false;
                    if (response.IsSuccess) {
                        $window.location.href = "/";
                    } else {
                        $scope.errorMessage = response.Message;
                    }
                })
                .catch(function (error) {
                    $scope.isLoading = false;
                    if (error.data && error.data.Message) {
                        $scope.errorMessage = error.data.Message;
                    } else {
                        // network error or something unhandled globally
                        $scope.errorMessage = ""; // handled by global error handler usually, or we can just leave empty if it's network
                    }
                });
        };
    }]);

    app.controller("LogoutController", ["$scope", "$window", "authService", 
        function ($scope, $window, authService) {
        
        $scope.logout = function () {
            authService.logout()
                .then(function () {
                    $window.location.href = "/Auth/Account/Login";
                })
                .catch(function (err) {
                    console.error("Logout failed", err);
                    $window.location.href = "/Auth/Account/Login";
                });
        };
    }]);

})();
