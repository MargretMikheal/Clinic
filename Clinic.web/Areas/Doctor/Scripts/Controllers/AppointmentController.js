(function () {
    "use strict";

    function DoctorAppointmentController($scope, $http, notificationService, $filter, permissionService, ScreenKeys) {
        $scope.appointments = [];
        $scope.filteredAppointments = [];

        $scope.searchQuery = "";
        $scope.filterFromDate = null;
        $scope.filterToDate = null;
        $scope.filterStatus = "";

        $scope.isLoading = false;
        $scope.isError = false;

        $scope.permissions = {
            Show: false
        };

        $scope.pagination = {
            page: 1,
            pageSize: 10,
            totalItems: 0
        };

        $scope.formatTimeSpan = function (timeSpan) {
            if (!timeSpan) return "";

            var hours = 0;
            var minutes = 0;

            if (typeof timeSpan === "string") {
                var parts = timeSpan.split(":");
                if (parts.length >= 2) {
                    hours = parseInt(parts[0], 10) || 0;
                    minutes = parseInt(parts[1], 10) || 0;
                }
            } else if (typeof timeSpan === "object") {
                if (timeSpan.Hours !== undefined && timeSpan.Minutes !== undefined) {
                    hours = timeSpan.Hours;
                    minutes = timeSpan.Minutes;
                } else {
                    return "";
                }
            } else {
                return "";
            }

            var hh = (hours < 10 ? "0" : "") + hours;
            var mm = (minutes < 10 ? "0" : "") + minutes;
            return hh + ":" + mm;
        };

        $scope.getStatusLocalizationKey = function (status) {
            switch (status) {
                case "Booked": return "appointment.Booked";
                case "Confirmed": return "appointment.Confirmed";
                case "Completed": return "appointment.Completed";
                case "Cancelled": return "appointment.Cancelled";
                case "NoShow": return "appointment.NoShow";
                default: return "";
            }
        };

        $scope.getStatusBadgeClass = function (status) {
            switch (status) {
                case "Booked": return "c-badge c-badge-booked";
                case "Confirmed": return "c-badge c-badge-confirmed";
                case "Completed": return "c-badge c-badge-completed";
                case "Cancelled": return "c-badge c-badge-cancelled";
                case "NoShow": return "c-badge c-badge-noshow";
                default: return "c-badge c-badge-neutral";
            }
        };

        $scope.getPaginatedAppointments = function () {
            var begin = (($scope.pagination.page - 1) * $scope.pagination.pageSize);
            var end = begin + $scope.pagination.pageSize;
            return $scope.filteredAppointments.slice(begin, end);
        };

        var filterTimeout = null;
        $scope.$watchGroup(["searchQuery", "filterFromDate", "filterToDate", "filterStatus"], function (newValues, oldValues) {
            var filtersChanged = false;
            for (var i = 0; i < newValues.length; i++) {
                if (newValues[i] !== oldValues[i]) {
                    filtersChanged = true;
                    break;
                }
            }

            if (filtersChanged) {
                var fromStr = $scope.filterFromDate ? $filter("date")($scope.filterFromDate, "yyyy-MM-dd") : null;
                var toStr = $scope.filterToDate ? $filter("date")($scope.filterToDate, "yyyy-MM-dd") : null;

                if (fromStr && toStr && fromStr > toStr) {
                    notificationService.error($filter("i18n")("appointment.InvalidDateRange") || "Invalid Date Range");
                    $scope.appointments = [];
                    $scope.filteredAppointments = [];
                    $scope.pagination.totalItems = 0;
                    return;
                }

                if (filterTimeout) {
                    clearTimeout(filterTimeout);
                }
                filterTimeout = setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.pagination.page = 1;
                        loadAppointments();
                    });
                }, 400);
            }
        });

        $scope.clearFilters = function () {
            $scope.filterFromDate = null;
            $scope.filterToDate = null;
            $scope.filterStatus = "";
            $scope.searchQuery = "";
            $scope.pagination.page = 1;
        };

        function loadPermissions() {
            return permissionService.loadPermissions()
                .then(function () {
                    $scope.permissions.Show = permissionService.hasPermission(ScreenKeys.MyAppointments, "Show");

                    if ($scope.permissions.Show) {
                        loadAppointments();
                    } else {
                        $scope.isError = true;
                        notificationService.error($filter("i18n")("common.Unauthorized") || "Unauthorized");
                    }
                })
                .catch(function () {
                    $scope.isError = true;
                    notificationService.error($filter("i18n")("common.Unauthorized") || "Unauthorized");
                });
        }

        function loadAppointments() {
            $scope.isLoading = true;
            $scope.isError = false;

            var params = {};
            if ($scope.filterFromDate) params.fromDate = $filter("date")($scope.filterFromDate, "yyyy-MM-dd");
            if ($scope.filterToDate) params.toDate = $filter("date")($scope.filterToDate, "yyyy-MM-dd");
            if ($scope.filterStatus) params.status = $scope.filterStatus;
            if ($scope.searchQuery) params.searchText = $scope.searchQuery;

            return $http.get("/Doctor/Appointment/GetMyAppointments", { params: params })
                .then(function (response) {
                    if (response.data.IsSuccess) {
                        $scope.appointments = response.data.Data || [];
                        $scope.filteredAppointments = $scope.appointments;
                        $scope.pagination.totalItems = $scope.filteredAppointments.length;
                        var totalPages = Math.ceil($scope.filteredAppointments.length / $scope.pagination.pageSize) || 1;
                        if ($scope.pagination.page > totalPages) {
                            $scope.pagination.page = totalPages;
                        }
                    } else {
                        $scope.isError = true;
                        notificationService.error(response.data.Message || $filter("i18n")("appointment.ErrorLoadingAppointments"));
                    }
                })
                .catch(function () {
                    $scope.isError = true;
                    notificationService.error($filter("i18n")("appointment.ErrorLoadingAppointments") || "Error loading appointments");
                })
                .finally(function () {
                    $scope.isLoading = false;
                });
        }

        $scope.init = function () {
            loadPermissions();
        };

        $scope.init();
    }

    DoctorAppointmentController.$inject = [
        "$scope",
        "$http",
        "notificationService",
        "$filter",
        "permissionService",
        "ScreenKeys"
    ];

    angular.module("clinicApp")
        .controller("AppointmentController", DoctorAppointmentController)
        .controller("DoctorAppointmentController", DoctorAppointmentController);
})();
