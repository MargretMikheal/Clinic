(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("SymptomController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            function ($scope, $http, notificationService, confirmationService, $filter) {

                $scope.symptoms = [];
                $scope.searchQuery = "";

                $scope.isLoading = false;
                $scope.isSaving = false;
                $scope.isError = false;

                $scope.viewState = "list"; 

                $scope.currentSymptom = {};
                $scope.pagination = {
                    page: 1,
                    pageSize: 10,
                    totalItems: 0
                };
                $scope.filteredSymptoms = [];

                $scope.$watchGroup(['symptoms', 'searchQuery'], function() {
                    $scope.filteredSymptoms = $filter("filter")($scope.symptoms, $scope.searchQuery) || [];
                    $scope.pagination.totalItems = $scope.filteredSymptoms.length;
                    var totalPages = Math.ceil($scope.filteredSymptoms.length / $scope.pagination.pageSize) || 1;
                    if ($scope.pagination.page > totalPages) {
                        $scope.pagination.page = totalPages;
                    }
                });

                $scope.getPaginatedSymptoms = function () {
                    var begin = (($scope.pagination.page - 1) * $scope.pagination.pageSize);
                    var end = begin + $scope.pagination.pageSize;
                    return $scope.filteredSymptoms.slice(begin, end);
                };

                function loadSymptoms() {
                    $scope.isLoading = true;
                    $scope.isError = false;

                    return $http.get("/Administration/Symptom/GetSymptoms")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.symptoms = response.data.Data || [];
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

                    $scope.currentSymptom = {
                        NameAr: "",
                        NameEn: "",
                        IsActive: true
                    };
                };

                $scope.openEdit = function (symptom) {
                    $scope.viewState = "edit";
                    $scope.currentSymptom = angular.copy(symptom);
                };

                $scope.cancel = function () {
                    $scope.viewState = "list";
                    $scope.currentSymptom = {};
                    $scope.pagination.page = 1;
                    loadSymptoms();
                };

                $scope.saveSymptom = function () {
                    if ($scope.isSaving) {
                        return;
                    }
                    $scope.isSaving = true;

                    var url =
                        $scope.viewState === "add"
                            ? "/Administration/Symptom/AddSymptom"
                            : "/Administration/Symptom/EditSymptom";
                    $http.post(url, $scope.currentSymptom)
                        .then(function (response) {

                            if (response.data.IsSuccess) {
                                notificationService.success(
                                    $filter("i18n")("common.Success")
                                );

                                $scope.viewState = "list";
                                $scope.currentSymptom = {};
                                $scope.pagination.page = 1;
                                return loadSymptoms();
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

                $scope.deleteSymptom = function (id) {
                    if ($scope.isLoading) {
                        return;
                    }

                    confirmationService.confirm(
                        $filter("i18n")("symptom.DeleteConfirmation")
                    ).then(function (confirm) {
                        if (!confirm) {
                            return;
                        }

                        $scope.isLoading = true;

                        $http.post(
                            "/Administration/Symptom/DeleteSymptom",
                            { id: id }
                        )
                            .then(function (response) {

                                if (response.data.IsSuccess) {

                                    notificationService.success(
                                        $filter("i18n")("common.Success")
                                    );

                                    return loadSymptoms();
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
                    loadSymptoms();
                };

                $scope.init();
            }
        ]);
})();
