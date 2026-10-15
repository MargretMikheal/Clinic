(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("DoctorPerformanceController", [
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

                $scope.filters = {
                    fromDate: null,
                    toDate: null
                };

                function formatDate(date) {
                    if (!date) return "";
                    if (angular.isString(date)) {
                        return date.substring(0, 10);
                    }
                    var d = new Date(date);
                    if (isNaN(d.getTime())) return "";
                    var month = "" + (d.getMonth() + 1);
                    var day = "" + d.getDate();
                    var year = d.getFullYear();

                    if (month.length < 2) month = "0" + month;
                    if (day.length < 2) day = "0" + day;

                    return [year, month, day].join("-");
                }

                $scope.resetFilters = function () {
                    $scope.filters = {
                        fromDate: null,
                        toDate: null
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

                    var fromDateStr = formatDate($scope.filters.fromDate);
                    var toDateStr = formatDate($scope.filters.toDate);

                    var params = {
                        fromDate: fromDateStr,
                        toDate: toDateStr
                    };

                    openReportInNewTab("/Administration/DoctorPerformanceReport/ViewReport", params);
                };

                $scope.init = function () {
                    permissionService.loadPermissions()
                        .then(function () {
                            $scope.permissions.Show = permissionService.hasPermission(
                                ScreenKeys.DoctorPerformanceReport,
                                "Show"
                            );

                            if (!$scope.permissions.Show) {
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
