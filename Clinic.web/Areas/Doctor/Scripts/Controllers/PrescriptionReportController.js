(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("PrescriptionReportDoctorController", [
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
                $scope.isLoadingPrescriptions = false;
                $scope.prescriptions = [];
                $scope.selectedPrescriptionId = null;
                $scope.searchQuery = "";

                $scope.searchPrescriptions = function (query) {
                    $scope.isLoadingPrescriptions = true;
                    $http.get("/Doctor/PrescriptionReport/SearchPrescriptions", {
                        params: { term: query || "" }
                    })
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess && response.data.Data) {
                                $scope.prescriptions = response.data.Data || [];
                            } else {
                                $scope.prescriptions = [];
                            }
                        })
                        .catch(function () {
                            $scope.prescriptions = [];
                        })
                        .finally(function () {
                            $scope.isLoadingPrescriptions = false;
                        });
                };
                $scope.$watch("selectedPrescriptionId", function (newValue, oldValue) {
                    console.log("Prescription selection changed:", {
                        oldValue: oldValue,
                        newValue: newValue,
                        type: typeof newValue
                    });
                });
                $scope.clearPrescription = function () {
                    $scope.selectedPrescriptionId = null;
                    $scope.searchQuery = "";
                };

                $scope.onPrescriptionChanged = function (value) {
                    console.log("1. ng-change value:", value);
                    console.log("2. model immediately:", $scope.selectedPrescriptionId);

                    $scope.$evalAsync(function () {
                        console.log("3. model after digest:", $scope.selectedPrescriptionId);
                    });
                };
                $scope.$watch("selectedPrescriptionId", function (newValue, oldValue) {
                    console.log("MODEL CHANGED:", {
                        oldValue: oldValue,
                        newValue: newValue
                    });
                });
                $scope.resetFilters = $scope.clearPrescription;

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

                // $scope.generateReport = function () {
                //     var rxId = parseInt($scope.selectedPrescriptionId, 10);
                //     if (!rxId || isNaN(rxId) || rxId <= 0) {
                //         notificationService.warning(
                //             $filter("i18n")("report.PrescriptionRequired") ||
                //             "Please select a prescription before generating the report."
                //         );
                //         return;
                //     }

                //     openReportInNewTab("/Doctor/PrescriptionReport/ViewReport", {
                //         prescriptionId: rxId
                //     });
                // };
                $scope.generateReport = function () {
                    var element = document.getElementById("ddlPrescriptionSelect");
                    var ngModel = angular.element(element).controller("ngModel");

                    var rxId = parseInt(ngModel.$modelValue, 10);

                    console.log("Selected ID from ngModel:", ngModel.$modelValue);
                    console.log("Parsed prescription ID:", rxId);

                    if (!rxId || isNaN(rxId) || rxId <= 0) {
                        notificationService.warning(
                            $filter("i18n")("report.PrescriptionRequired") ||
                            "Please select a prescription before generating the report."
                        );
                        return;
                    }

                    openReportInNewTab("/Doctor/PrescriptionReport/ViewReport", {
                        prescriptionId: rxId
                    });
                };

                $scope.init = function () {
                    permissionService.loadPermissions()
                        .then(function () {
                            $scope.permissions.Show = permissionService.hasPermission(
                                ScreenKeys.PrescriptionReport,
                                "Show"
                            );

                            if ($scope.permissions.Show) {
                                $scope.searchPrescriptions("");
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
