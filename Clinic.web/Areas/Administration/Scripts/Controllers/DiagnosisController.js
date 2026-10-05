(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("DiagnosisController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            function ($scope, $http, notificationService, confirmationService, $filter) {

                $scope.diagnoses = [];
                $scope.searchQuery = "";

                $scope.isLoading = false;
                $scope.isSaving = false;
                $scope.isError = false;

                $scope.viewState = "list"; 

                $scope.currentDiagnosis = {};
                $scope.pagination = {
                    page: 1,
                    pageSize: 5,
                    totalItems: 0
                };
                $scope.filteredDiagnoses = [];

                $scope.$watchGroup(['diagnoses', 'searchQuery'], function() {
                    $scope.filteredDiagnoses = $filter("filter")($scope.diagnoses, $scope.searchQuery) || [];
                    $scope.pagination.totalItems = $scope.filteredDiagnoses.length;
                    var totalPages = Math.ceil($scope.filteredDiagnoses.length / $scope.pagination.pageSize) || 1;
                    if ($scope.pagination.page > totalPages) {
                        $scope.pagination.page = totalPages;
                    }
                });

                $scope.getPaginatedDiagnoses = function () {
                    var begin = (($scope.pagination.page - 1) * $scope.pagination.pageSize);
                    var end = begin + $scope.pagination.pageSize;
                    return $scope.filteredDiagnoses.slice(begin, end);
                };

                function loadDiagnoses() {
                    $scope.isLoading = true;
                    $scope.isError = false;

                    return $http.get("/Administration/Diagnosis/GetDiagnoses")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.diagnoses = response.data.Data || [];
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

                    $scope.currentDiagnosis = {
                        NameAr: "",
                        NameEn: "",
                        IsActive: true
                    };
                };

                $scope.openEdit = function (diagnosis) {
                    $scope.viewState = "edit";
                    $scope.currentDiagnosis = angular.copy(diagnosis);
                };

                $scope.cancel = function () {
                    $scope.viewState = "list";
                    $scope.currentDiagnosis = {};
                    $scope.pagination.page = 1;
                    loadDiagnoses();
                };

                $scope.saveDiagnosis = function () {
                    if ($scope.isSaving) {
                        return;
                    }
                    $scope.isSaving = true;

                    var url =
                        $scope.viewState === "add"
                            ? "/Administration/Diagnosis/AddDiagnosis"
                            : "/Administration/Diagnosis/EditDiagnosis";
                    $http.post(url, $scope.currentDiagnosis)
                        .then(function (response) {

                            if (response.data.IsSuccess) {
                                notificationService.success(
                                    $filter("i18n")("common.Success")
                                );

                                $scope.viewState = "list";
                                $scope.currentDiagnosis = {};
                                $scope.pagination.page = 1;
                                return loadDiagnoses();
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

                $scope.deleteDiagnosis = function (id) {
                    if ($scope.isLoading) {
                        return;
                    }

                    confirmationService.confirm(
                        $filter("i18n")("diagnosis.DeleteConfirmation")
                    ).then(function (confirm) {
                        if (!confirm) {
                            return;
                        }

                        $scope.isLoading = true;

                        $http.post(
                            "/Administration/Diagnosis/DeleteDiagnosis",
                            { id: id }
                        )
                            .then(function (response) {

                                if (response.data.IsSuccess) {

                                    notificationService.success(
                                        $filter("i18n")("common.Success")
                                    );

                                    return loadDiagnoses();
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
                    loadDiagnoses();
                };

                $scope.init();
            }
        ]);
})();
