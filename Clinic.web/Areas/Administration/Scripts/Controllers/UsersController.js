(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("UsersController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            "permissionService",
            "ScreenKeys",
            function ($scope, $http, notificationService, confirmationService, $filter, permissionService, ScreenKeys) {

                // ----------------------------------------------------
                // USERS STATE & LOGIC
                // ----------------------------------------------------
                $scope.users = [];
                $scope.roles = [];
                $scope.searchQuery = "";

                $scope.isLoading = false;
                $scope.isSaving = false;
                $scope.isError = false;

                $scope.permissions = { Show: false, Add: false, Edit: false, Delete: false, Print: false };

                $scope.viewState = "list"; 

                $scope.currentUser = {};
                $scope.pagination = {
                    page: 1,
                    pageSize: 5,
                    totalItems: 0
                };
                $scope.filteredUsers = [];

                $scope.$watchGroup(['users', 'searchQuery'], function() {
                    $scope.filteredUsers = $filter("filter")($scope.users, $scope.searchQuery) || [];
                    $scope.pagination.totalItems = $scope.filteredUsers.length;
                    var totalPages = Math.ceil($scope.filteredUsers.length / $scope.pagination.pageSize) || 1;
                    if ($scope.pagination.page > totalPages) {
                        $scope.pagination.page = totalPages;
                    }
                });

                $scope.getPaginatedUsers = function () {
                    var begin = (($scope.pagination.page - 1) * $scope.pagination.pageSize);
                    var end = begin + $scope.pagination.pageSize;
                    return $scope.filteredUsers.slice(begin, end);
                };

                function loadUsers() {
                    $scope.isLoading = true;
                    $scope.isError = false;

                    return $http.get("/Administration/Users/GetUsers")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.users = response.data.Data || [];
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
                                $filter("i18n")("users.ErrorLoadingUsers")
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
                                $filter("i18n")("users.ErrorLoadingRoles")
                            );
                        });
                }

                $scope.openAdd = function () {
                    if (!$scope.permissions.Add) return;
                    $scope.viewState = "add";

                    $scope.currentUser = {
                        UserName: "",
                        Email: "",
                        Password: "",
                        Role: ""
                    };
                };

                $scope.openEdit = function (user) {
                    if (!$scope.permissions.Edit) return;
                    $scope.viewState = "edit";
                    $scope.currentUser = angular.copy(user);
                    $scope.currentUser.Password = "";
                };

                $scope.cancel = function () {
                    $scope.viewState = "list";
                    $scope.currentUser = {};
                    $scope.pagination.page = 1;

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
                                $scope.pagination.page = 1;
                                return loadUsers();
                            }
                            notificationService.error(
                                response.data.Message
                            );
                        })
                        .catch(function () {
                            notificationService.error(
                                $filter("i18n")("users.ErrorSavingUser")
                            );
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                $scope.deleteUser = function (id) {
                    if ($scope.isLoading || !$scope.permissions.Delete) {
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
                                    $filter("i18n")("users.ErrorDeletingUser")
                                );
                            })
                            .finally(function () {
                                $scope.isLoading = false;
                            });
                    });
                };

                $scope.init = function () {
                    permissionService.loadPermissions().then(function () {
                        $scope.permissions.Show = permissionService.hasPermission(ScreenKeys.Users, "Show");
                        $scope.permissions.Add = permissionService.hasPermission(ScreenKeys.Users, "Add");
                        $scope.permissions.Edit = permissionService.hasPermission(ScreenKeys.Users, "Edit");
                        $scope.permissions.Delete = permissionService.hasPermission(ScreenKeys.Users, "Delete");

                        if ($scope.permissions.Show) {
                            loadRoles();
                            loadUsers();
                        } else {
                            $scope.isError = true;
                            notificationService.error($filter("i18n")("common.Unauthorized") || "Unauthorized");
                        }
                    }).catch(function () {
                        $scope.isError = true;
                    });
                };

                $scope.init();
            }
        ]);
})();