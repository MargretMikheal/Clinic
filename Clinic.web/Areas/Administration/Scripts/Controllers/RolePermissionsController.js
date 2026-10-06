(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("RolePermissionController", [
            "$scope",
            "$http",
            "notificationService",
            "$filter",
            "permissionService",
            "ScreenKeys",
            function ($scope, $http, notificationService, $filter, permissionService, ScreenKeys) {
                
                $scope.roles = [];
                $scope.selectedRoleId = null;
                $scope.permissions = [];
                $scope.newRoleName = "";
                
                $scope.isLoading = false;
                $scope.isSaving = false;
                
                $scope.permissionsObj = { Show: false, Add: false, Edit: false, Delete: false, Print: false };

                $scope.selectedTabIndex = 0;

                $scope.screenPagination = {
                    page: 1,
                    pageSize: 5 ,
                    totalItems: 0
                };

                $scope.getPaginatedScreens = function () {
                    var begin = (($scope.screenPagination.page - 1) * $scope.screenPagination.pageSize);
                    var end = begin + $scope.screenPagination.pageSize;
                    return $scope.permissions.slice(begin, end);
                };

                function loadRoles() {
                    $scope.isLoading = true;
                    return $http.get("/Administration/RolePermissions/GetRoles")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.roles = response.data.Data || [];
                            } else {
                                notificationService.error(response.data.Message);
                            }
                        })
                        .catch(function () {
                            notificationService.error($filter("i18n")("common.SomethingWentWrong"));
                        })
                        .finally(function () {
                            $scope.isLoading = false;
                        });
                }

                $scope.onRoleChanged = function () {
                    $scope.screenPagination.page = 1;
                    if (!$scope.selectedRoleId) {
                        $scope.permissions = [];
                        $scope.screenPagination.totalItems = 0;
                        return;
                    }
                    $scope.isLoading = true;
                    $http.get("/Administration/RolePermissions/GetRolePermissions?roleId=" + $scope.selectedRoleId)
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.permissions = response.data.Data || [];
                                $scope.screenPagination.totalItems = $scope.permissions.length;
                            } else {
                                notificationService.error(response.data.Message);
                            }
                        })
                        .catch(function () {
                            notificationService.error($filter("i18n")("common.SomethingWentWrong"));
                        })
                        .finally(function () {
                            $scope.isLoading = false;
                        });
                };

                $scope.addRole = function () {
                    if (!$scope.permissionsObj.Add) return;
                    if (!$scope.newRoleName) {
                        notificationService.warning($filter("i18n")("rolepermission.RoleNameRequired"));
                        return;
                    }
                    $scope.isSaving = true;
                    $http.post("/Administration/RolePermissions/AddRole", { Name: $scope.newRoleName })
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                notificationService.success($filter("i18n")("rolepermission.RoleAddedSuccessfully"));
                                $scope.newRoleName = "";
                                loadRoles();
                            } else {
                                notificationService.error(response.data.Message);
                            }
                        })
                        .catch(function () {
                            notificationService.error($filter("i18n")("common.SomethingWentWrong"));
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                $scope.savePermissions = function () {
                    if (!$scope.permissionsObj.Edit) return;
                    if (!$scope.selectedRoleId) {
                        notificationService.warning($filter("i18n")("rolepermission.RoleIsRequired"));
                        return;
                    }
                    $scope.isSaving = true;
                    var payload = {
                        RoleId: $scope.selectedRoleId,
                        Permissions: $scope.permissions
                    };
                    $http.post("/Administration/RolePermissions/SaveRolePermissions", payload)
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                notificationService.success($filter("i18n")("rolepermission.PermissionsSavedSuccessfully"));
                            } else {
                                notificationService.error(response.data.Message);
                            }
                        })
                        .catch(function () {
                            notificationService.error($filter("i18n")("common.SomethingWentWrong"));
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                $scope.init = function () {
                    permissionService.loadPermissions().then(function () {
                        $scope.permissionsObj.Show = permissionService.hasPermission(ScreenKeys.RolePermissions, "Show");
                        $scope.permissionsObj.Add = permissionService.hasPermission(ScreenKeys.RolePermissions, "Add");
                        $scope.permissionsObj.Edit = permissionService.hasPermission(ScreenKeys.RolePermissions, "Edit");
                        $scope.permissionsObj.Delete = permissionService.hasPermission(ScreenKeys.RolePermissions, "Delete");
                        $scope.permissionsObj.Print = permissionService.hasPermission(ScreenKeys.RolePermissions, "Print");

                        if ($scope.permissionsObj.Show) {
                            loadRoles();
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
