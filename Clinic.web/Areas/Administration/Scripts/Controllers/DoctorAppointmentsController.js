(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("DoctorAppointmentsController", [
            "$scope",
            "$http",
            "notificationService",
            "$filter",
            "permissionService",
            "ScreenKeys",
            "appointmentStatusService",
            function ($scope, $http, notificationService, $filter, permissionService, ScreenKeys, appointmentStatusService) {

                $scope.permissions = {
                    Show: false
                };
                $scope.isUnauthorized = false;
                $scope.isLoadingMasterData = false;

                $scope.doctors = [];
                $scope.statuses = [];

                $scope.filters = {
                    doctorId: null,
                    fromDate: null,
                    toDate: null,
                    status: ""
                };

                function formatDate(date) {
                    if (!date) return null;
                    if (angular.isString(date)) {
                        return date.substring(0, 10);
                    }
                    var d = new Date(date);
                    if (isNaN(d.getTime())) return null;
                    var month = "" + (d.getMonth() + 1);
                    var day = "" + d.getDate();
                    var year = d.getFullYear();

                    if (month.length < 2) month = "0" + month;
                    if (day.length < 2) day = "0" + day;

                    return [year, month, day].join("-");
                }

                $scope.getStatusLocalizationKey = function (status) {
                    return appointmentStatusService.getStatusLocalizationKey(status);
                };

                $scope.loadFilterData = function () {
                    $scope.isLoadingMasterData = true;

                    $http.get("/Administration/DoctorAppointmentsReport/GetFilterData")
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess && response.data.Data) {
                                $scope.doctors = response.data.Data.Doctors || [];
                                $scope.statuses = response.data.Data.Statuses || [];
                            } else {
                                notificationService.error(
                                    (response.data && response.data.Message) ||
                                    $filter("i18n")("common.SomethingWentWrong")
                                );
                            }
                        })
                        .catch(function () {
                            notificationService.error($filter("i18n")("common.SomethingWentWrong"));
                        })
                        .finally(function () {
                            $scope.isLoadingMasterData = false;
                        });
                };

                $scope.resetFilters = function () {
                    $scope.filters = {
                        doctorId: null,
                        fromDate: null,
                        toDate: null,
                        status: ""
                    };
                };

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
                    if ($scope.filters.fromDate && $scope.filters.toDate) {
                        var fromD = new Date($scope.filters.fromDate);
                        var toD = new Date($scope.filters.toDate);
                        if (!isNaN(fromD.getTime()) && !isNaN(toD.getTime()) && fromD > toD) {
                            notificationService.warning(
                                $filter("i18n")("report.InvalidDateRange") ||
                                "From Date cannot be after To Date."
                            );
                            return;
                        }
                    }

                    var doctorId = $scope.filters.doctorId ? parseInt($scope.filters.doctorId, 10) : 0;
                    var fromDateStr = formatDate($scope.filters.fromDate);
                    var toDateStr = formatDate($scope.filters.toDate);
                    var statusStr = $scope.filters.status || "";

                    var params = {
                        doctorId: doctorId,
                        fromDate: fromDateStr,
                        toDate: toDateStr,
                        status: statusStr
                    };

                    openReportInNewTab("/Administration/DoctorAppointmentsReport/ViewReport", params);
                };

                $scope.init = function () {
                    permissionService.loadPermissions()
                        .then(function () {
                            $scope.permissions.Show = permissionService.hasPermission(
                                ScreenKeys.DoctorAppointmentsReport,
                                "Show"
                            );

                            if ($scope.permissions.Show) {
                                $scope.loadFilterData();
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
