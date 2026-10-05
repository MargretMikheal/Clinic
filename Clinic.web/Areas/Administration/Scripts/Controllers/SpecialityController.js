(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("SpecialityController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            function ($scope, $http, notificationService, confirmationService, $filter) {

                $scope.specialities = [];
                $scope.searchQuery = "";

                $scope.isLoading = false;
                $scope.isSaving = false;
                $scope.isError = false;

                $scope.viewState = "list"; 

                $scope.currentSpeciality = {};
                $scope.pagination = {
                    page: 1,
                    pageSize: 10,
                    totalItems: 0
                };
                $scope.filteredSpecialities = [];

                $scope.$watchGroup(['specialities', 'searchQuery'], function() {
                    $scope.filteredSpecialities = $filter("filter")($scope.specialities, $scope.searchQuery) || [];
                    $scope.pagination.totalItems = $scope.filteredSpecialities.length;
                    var totalPages = Math.ceil($scope.filteredSpecialities.length / $scope.pagination.pageSize) || 1;
                    if ($scope.pagination.page > totalPages) {
                        $scope.pagination.page = totalPages;
                    }
                });

                $scope.getPaginatedSpecialities = function () {
                    var begin = (($scope.pagination.page - 1) * $scope.pagination.pageSize);
                    var end = begin + $scope.pagination.pageSize;
                    return $scope.filteredSpecialities.slice(begin, end);
                };

                function loadSpecialities() {
                    $scope.isLoading = true;
                    $scope.isError = false;

                    return $http.get("/Administration/Speciality/GetSpecialities")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.specialities = response.data.Data || [];
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
                                $filter("i18n")("common.SomethingWentWrong")
                            );
                        })
                        .finally(function () {
                            $scope.isLoading = false;
                        });
                }

                $scope.openAdd = function () {
                    $scope.viewState = "add";

                    $scope.currentSpeciality = {
                        NameAr: "",
                        NameEn: "",
                        IsActive: true
                    };
                };

                $scope.openEdit = function (speciality) {
                    $scope.viewState = "edit";
                    $scope.currentSpeciality = angular.copy(speciality);
                };

                $scope.cancel = function () {
                    $scope.viewState = "list";
                    $scope.currentSpeciality = {};
                    $scope.pagination.page = 1;
                    loadSpecialities();
                };

                $scope.saveSpeciality = function () {
                    if ($scope.isSaving) {
                        return;
                    }
                    $scope.isSaving = true;

                    var url =
                        $scope.viewState === "add"
                            ? "/Administration/Speciality/AddSpeciality"
                            : "/Administration/Speciality/EditSpeciality";
                    $http.post(url, $scope.currentSpeciality)
                        .then(function (response) {

                            if (response.data.IsSuccess) {
                                notificationService.success(
                                    $filter("i18n")("common.Success")
                                );

                                $scope.viewState = "list";
                                $scope.currentSpeciality = {};
                                $scope.pagination.page = 1;
                                return loadSpecialities();
                            }
                            notificationService.error(
                                response.data.Message
                            );
                        })
                        .catch(function () {
                            notificationService.error(
                                $filter("i18n")("common.SomethingWentWrong")
                            );
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                $scope.deleteSpeciality = function (id) {
                    if ($scope.isLoading) {
                        return;
                    }

                    confirmationService.confirm(
                        $filter("i18n")("speciality.DeleteConfirmation")
                    ).then(function (confirm) {
                        if (!confirm) {
                            return;
                        }

                        $scope.isLoading = true;

                        $http.post(
                            "/Administration/Speciality/DeleteSpeciality",
                            { id: id }
                        )
                            .then(function (response) {

                                if (response.data.IsSuccess) {

                                    notificationService.success(
                                        $filter("i18n")("common.Success")
                                    );

                                    return loadSpecialities();
                                }

                                notificationService.error(
                                    response.data.Message
                                );
                            })
                            .catch(function () {

                                notificationService.error(
                                    $filter("i18n")("common.SomethingWentWrong")
                                );
                            })
                            .finally(function () {
                                $scope.isLoading = false;
                            });
                    });
                };

                $scope.init = function () {
                    loadSpecialities();
                };

                $scope.init();
            }
        ]);
})();
