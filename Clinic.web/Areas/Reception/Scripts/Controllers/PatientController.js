(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("PatientController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            "permissionService",
            "ScreenKeys",
            function ($scope, $http, notificationService, confirmationService, $filter, permissionService, ScreenKeys) {

                $scope.patients = [];
                $scope.searchQuery = "";
                $scope.isLoading = false;
                $scope.isSaving = false;
                $scope.isError = false;

                $scope.viewState = "list"; 
                $scope.currentPatient = {};

                $scope.permissions = {
                    Show: false,
                    Add: false,
                    Edit: false,
                    Delete: false,
                    Print: false
                };

                $scope.pagination = {
                    page: 1,
                    pageSize: 5,
                    totalItems: 0
                };
                $scope.filteredPatients = [];

                $scope.$watchGroup(['patients', 'searchQuery'], function() {
                    $scope.filteredPatients = $filter("filter")($scope.patients, $scope.searchQuery) || [];
                    $scope.pagination.totalItems = $scope.filteredPatients.length;
                    var totalPages = Math.ceil($scope.filteredPatients.length / $scope.pagination.pageSize) || 1;
                    if ($scope.pagination.page > totalPages) {
                        $scope.pagination.page = totalPages;
                    }
                });

                $scope.getPaginatedPatients = function () {
                    var begin = (($scope.pagination.page - 1) * $scope.pagination.pageSize);
                    var end = begin + $scope.pagination.pageSize;
                    return $scope.filteredPatients.slice(begin, end);
                };

                function loadPermissions() {
                    return permissionService.loadPermissions()
                        .then(function () {
                            $scope.permissions.Show = permissionService.hasPermission(ScreenKeys.Patients, "Show");
                            $scope.permissions.Add = permissionService.hasPermission(ScreenKeys.Patients, "Add");
                            $scope.permissions.Edit = permissionService.hasPermission(ScreenKeys.Patients, "Edit");
                            $scope.permissions.Delete = permissionService.hasPermission(ScreenKeys.Patients, "Delete");

                            if ($scope.permissions.Show) {
                                loadPatients();
                            } else {
                                $scope.isError = true;
                                notificationService.error($filter("i18n")("common.Unauthorized") || "Unauthorized");
                            }
                        })
                        .catch(function () {
                            $scope.isError = true;
                        });
                }

                function loadPatients() {
                    $scope.isLoading = true;
                    $scope.isError = false;

                    return $http.get("/Reception/Patient/GetPatients")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.patients = response.data.Data || [];
                            } else {
                                $scope.isError = true;
                                notificationService.error(response.data.Message);
                            }
                        })
                        .catch(function () {
                            $scope.isError = true;
                            notificationService.error($filter("i18n")("patient.ErrorLoadingPatients") || "Error loading patients");
                        })
                        .finally(function () {
                            $scope.isLoading = false;
                        });
                }

                $scope.openAdd = function () {
                    if (!$scope.permissions.Add) return;
                    $scope.viewState = "add";
                    $scope.currentPatient = {
                        Name: "",
                        Gender: "",
                        BirthDate: null,
                        Phone: "",
                        Email: "",
                        Address: ""
                    };
                };

                $scope.openEdit = function (patient) {
                    if (!$scope.permissions.Edit) return;
                    $scope.viewState = "edit";
                    $scope.currentPatient = angular.copy(patient);
                    if ($scope.currentPatient.BirthDate) {
                        $scope.currentPatient.BirthDate = new Date($scope.currentPatient.BirthDate);
                    }
                };

                $scope.openView = function (patient) {
                    $scope.viewState = "view";
                    $scope.currentPatient = angular.copy(patient);
                };

                $scope.cancel = function () {
                    $scope.viewState = "list";
                    $scope.currentPatient = {};
                    $scope.pagination.page = 1;
                    if ($scope.permissions.Show) {
                        loadPatients();
                    }
                };

                $scope.savePatient = function () {
                    if ($scope.isSaving) return;
                    $scope.isSaving = true;

                    var url = $scope.viewState === "add" ? "/Reception/Patient/AddPatient" : "/Reception/Patient/EditPatient";
                    
                    var payload = angular.copy($scope.currentPatient);

                    $http.post(url, payload)
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                notificationService.success($filter("i18n")("patient.PatientSavedSuccess") || "Patient saved successfully");
                                $scope.viewState = "list";
                                $scope.currentPatient = {};
                                $scope.pagination.page = 1;
                                return loadPatients();
                            }
                            notificationService.error(response.data.Message);
                        })
                        .catch(function () {
                            notificationService.error($filter("i18n")("patient.ErrorSavingPatient") || "Error saving patient");
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                $scope.deletePatient = function (id) {
                    if ($scope.isLoading || !$scope.permissions.Delete) return;

                    confirmationService.confirm($filter("i18n")("patient.ConfirmDeletePatient") || "Are you sure you want to delete this patient?")
                        .then(function (confirm) {
                            if (!confirm) return;

                            $scope.isLoading = true;

                            $http.post("/Reception/Patient/DeletePatient", { id: id })
                                .then(function (response) {
                                    if (response.data.IsSuccess) {
                                        notificationService.success($filter("i18n")("patient.PatientDeletedSuccess") || "Patient deleted successfully");
                                        return loadPatients();
                                    }
                                    notificationService.error(response.data.Message);
                                })
                                .catch(function () {
                                    notificationService.error($filter("i18n")("patient.ErrorDeletingPatient") || "Error deleting patient");
                                })
                                .finally(function () {
                                    $scope.isLoading = false;
                                });
                        });
                };

                $scope.init = function () {
                    loadPermissions().then(function() {
                        if ((window.location.hash === '#add' || window.location.hash === '#/add') && $scope.permissions.Add) {
                            $scope.openAdd();
                            window.history.replaceState(null, null, window.location.pathname + window.location.search);
                        }
                    });
                };

                $scope.init();
            }
        ]);
})();
