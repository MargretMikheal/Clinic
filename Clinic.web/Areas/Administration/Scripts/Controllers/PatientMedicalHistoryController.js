(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("PatientMedicalHistoryReportController", [
            "$scope",
            "$http",
            "notificationService",
            "$filter",
            "permissionService",
            "ScreenKeys",
            function ($scope, $http, notificationService, $filter, permissionService, ScreenKeys) {

                $scope.permissions = {
                    Show: false
                };
                $scope.isUnauthorized = false;
                $scope.isLoadingPatients = false;
                $scope.patients = [];
                $scope.selectedPatientId = null;
                $scope.searchQuery = "";

                $scope.searchPatients = function (query) {
                    $scope.isLoadingPatients = true;
                    $http.get("/Administration/PatientMedicalHistoryReport/SearchPatients", {
                        params: { term: query || "" }
                    })
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess && response.data.Data) {
                                $scope.patients = response.data.Data || [];
                            } else {
                                $scope.patients = [];
                            }
                        })
                        .catch(function () {
                            $scope.patients = [];
                        })
                        .finally(function () {
                            $scope.isLoadingPatients = false;
                        });
                };

                $scope.clearPatient = function () {
                    $scope.selectedPatientId = null;
                    $scope.searchQuery = "";
                };

                $scope.resetFilters = $scope.clearPatient;

                function openReportInNewTab(actionUrl, params) {
                    var form = document.createElement("form");
                    form.method = "POST";
                    form.action = actionUrl;
                    form.target = "_blank";
                    form.style.display = "none";

                    if (params) {
                        for (var key in params) {
                            if (params.hasOwnProperty(key) && params[key] !== null && params[key] !== undefined && params[key] !== "") {
                                var input = document.createElement("input");
                                input.type = "hidden";
                                input.name = key;
                                input.value = params[key];
                                form.appendChild(input);
                            }
                        }
                    }

                    document.body.appendChild(form);
                    form.submit();
                    setTimeout(function () {
                        if (form.parentNode) {
                            form.parentNode.removeChild(form);
                        }
                    }, 500);
                }

                $scope.generateReport = function () {
                    var element = document.getElementById("ddlPatientSelect");
                    var ngModel = angular.element(element).controller("ngModel");

                    var patientId = parseInt(ngModel.$modelValue, 10);

                    if (!patientId || isNaN(patientId) || patientId <= 0) {
                        notificationService.warning(
                            $filter("i18n")("report.PatientRequired") ||
                            "Please select a patient before generating the report."
                        );
                        return;
                    }

                    openReportInNewTab(
                        "/Administration/PatientMedicalHistoryReport/ViewReport",
                        {
                            patientId: patientId
                        }
                    );
                };


                $scope.init = function () {
                    permissionService.loadPermissions()
                        .then(function () {
                            $scope.permissions.Show = permissionService.hasPermission(
                                ScreenKeys.PatientMedicalHistoryReport,
                                "Show"
                            );

                            if ($scope.permissions.Show) {
                                $scope.searchPatients("");
                            } else {
                                $scope.isUnauthorized = true;
                            }
                        })
                        .catch(function () {
                            $scope.isUnauthorized = true;
                        });
                };

                $scope.init();
            }
        ]);
})();
