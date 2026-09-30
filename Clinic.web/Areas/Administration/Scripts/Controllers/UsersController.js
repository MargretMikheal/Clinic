(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("UsersController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            function ($scope, $http, notificationService, confirmationService, $filter) {

                $scope.users = [];
                $scope.roles = [];
                $scope.searchQuery = "";

                $scope.isLoading = false;
                $scope.isSaving = false;
                $scope.isError = false;

                $scope.viewState = "list"; 

                $scope.currentUser = {};
                $scope.currentPage = 1;
                $scope.pageSize = 10;

                $scope.getPaginatedUsers = function () {
                    var filtered = $filter("filter")(
                        $scope.users,
                        $scope.searchQuery
                    );

                    var begin = (($scope.currentPage - 1) * $scope.pageSize);
                    var end = begin + $scope.pageSize;

                    return filtered ? filtered.slice(begin, end) : [];
                };

                $scope.getTotalPages = function () {
                    var filtered = $filter("filter")(
                        $scope.users,
                        $scope.searchQuery
                    );

                    if (!filtered) {
                        return 1;
                    }

                    return Math.ceil(filtered.length / $scope.pageSize) || 1;
                };

                $scope.nextPage = function () {
                    if ($scope.currentPage < $scope.getTotalPages()) {
                        $scope.currentPage++;
                    }
                };

                $scope.prevPage = function () {
                    if ($scope.currentPage > 1) {
                        $scope.currentPage--;
                    }
                };

                function loadUsers() {
                    $scope.isLoading = true;
                    $scope.isError = false;

                    return $http.get("/Administration/Users/GetUsers")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.users = response.data.Data || [];
                                var totalPages = $scope.getTotalPages();
                                if ($scope.currentPage > totalPages) {
                                    $scope.currentPage = totalPages;
                                }
                            }
                            else {
                                $scope.isError = true;
                                notificationService.error(
                                    response.data.Message
                                );
                            }
                        })
                        .catch(function () {
                            $scope.isError = true;
                            notificationService.error(
                                "An error occurred while loading users."
                            );
                        })
                        .finally(function () {
                            $scope.isLoading = false;
                        });
                }

                function loadRoles() {
                    return $http.get("/Administration/Users/GetRoles")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.roles = response.data.Data || [];
                            }
                            else {
                                notificationService.error(
                                    response.data.Message
                                );
                            }
                        })
                        .catch(function () {
                            notificationService.error(
                                "An error occurred while loading roles."
                            );
                        });
                }

                $scope.openAdd = function () {
                    $scope.viewState = "add";

                    $scope.currentUser = {
                        UserName: "",
                        Email: "",
                        Password: "",
                        Role: ""
                    };
                };

                $scope.openEdit = function (user) {
                    $scope.viewState = "edit";
                    $scope.currentUser = angular.copy(user);
                    $scope.currentUser.Password = "";
                };

                $scope.cancel = function () {
                    $scope.viewState = "list";
                    $scope.currentUser = {};
                    $scope.currentPage = 1;

                    loadUsers();
                };

                $scope.saveUser = function () {
                    if ($scope.isSaving) {
                        return;
                    }
                    $scope.isSaving = true;

                    var url =
                        $scope.viewState === "add"
                            ? "/Administration/Users/AddUser"
                            : "/Administration/Users/EditUser";
                    $http.post(url, $scope.currentUser)
                        .then(function (response) {

                            if (response.data.IsSuccess) {
                                notificationService.success(
                                    $filter("i18n")("users.UserSavedSuccess")
                                );

                                $scope.viewState = "list";
                                $scope.currentUser = {};
                                $scope.currentPage = 1;
                                return loadUsers();
                            }
                            notificationService.error(
                                response.data.Message
                            );
                        })
                        .catch(function () {
                            notificationService.error(
                                "An error occurred while saving the user."
                            );
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                $scope.deleteUser = function (id) {
                    if ($scope.isLoading) {
                        return;
                    }

                    confirmationService.confirm(
                        $filter("i18n")("users.ConfirmDeleteUser")
                    ).then(function (confirm) {
                        if (!confirm) {
                            return;
                        }

                        $scope.isLoading = true;

                        $http.post(
                            "/Administration/Users/DeleteUser",
                            { id: id }
                        )
                            .then(function (response) {

                                if (response.data.IsSuccess) {

                                    notificationService.success(
                                        $filter("i18n")("users.UserDeletedSuccess")
                                    );

                                    return loadUsers();
                                }

                                notificationService.error(
                                    response.data.Message
                                );
                            })
                            .catch(function () {

                                notificationService.error(
                                    "An error occurred while deleting the user."
                                );
                            })
                            .finally(function () {
                                $scope.isLoading = false;
                            });
                    });
                }; $scope.deleteUser = function (id) {
                    if ($scope.isLoading) {
                        return;
                    }

                    confirmationService.confirm(
                        $filter("i18n")("users.ConfirmDeleteUser")
                    ).then(function (confirm) {
                        if (!confirm) {
                            return;
                        }

                        $scope.isLoading = true;

                        $http.post(
                            "/Administration/Users/DeleteUser",
                            { id: id }
                        )
                            .then(function (response) {

                                if (response.data.IsSuccess) {

                                    notificationService.success(
                                        $filter("i18n")("users.UserDeletedSuccess")
                                    );

                                    return loadUsers();
                                }

                                notificationService.error(
                                    response.data.Message
                                );
                            })
                            .catch(function () {

                                notificationService.error(
                                    "An error occurred while deleting the user."
                                );
                            })
                            .finally(function () {
                                $scope.isLoading = false;
                            });
                    });
                };

                $scope.init = function () {
                    loadRoles();
                    loadUsers();
                };

                $scope.init();
            }
        ]);
})();