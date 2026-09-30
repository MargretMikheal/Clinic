(function () {
    "use strict";

    var app = angular.module("clinicApp");

    app.controller("DashboardController", [
        "$scope",
        "authService",
        "dashboardService",
        function ($scope, authService, dashboardService) {

            $scope.userInfo = null;
            $scope.dashboardType = null;
            $scope.dashboardData = null;

            $scope.isLoading = true;
            $scope.errorMessage = "";

            // =========================================================
            // Details State
            // =========================================================

            $scope.selectedStatistic = null;
            $scope.selectedDetails = null;
            $scope.isLoadingDetails = false;
            $scope.detailsError = "";

            $scope.statisticUiConfig = {
                Doctors: { icon: "fa-user-md", colorClass: "stat-primary", localizationKey: "dashboard.Doctors" },
                Patients: { icon: "fa-users", colorClass: "stat-success", localizationKey: "dashboard.Patients" },
                Specialities: { icon: "fa-stethoscope", colorClass: "stat-primary", localizationKey: "dashboard.Specialities" },
                TodayAppointments: { icon: "fa-calendar", colorClass: "stat-primary", localizationKey: "dashboard.TodayAppointments" },
                TodayCompletedVisits: { icon: "fa-check-circle", colorClass: "stat-success", localizationKey: "dashboard.TodayCompletedVisits" },
                TodayCancelledAppointments: { icon: "fa-times-circle", colorClass: "stat-danger", localizationKey: "dashboard.TodayCancelledAppointments" },
                ConfirmedAppointments: { icon: "fa-calendar-check-o", colorClass: "stat-success", localizationKey: "dashboard.ConfirmedAppointments" },
                CompletedAppointments: { icon: "fa-check-circle", colorClass: "stat-success", localizationKey: "dashboard.CompletedAppointments" },
                RemainingAppointments: { icon: "fa-clock-o", colorClass: "stat-warning", localizationKey: "dashboard.RemainingAppointments" }
            };

            var detailsMap = {
                Doctors: "Doctors",
                Patients: "Patients",
                Specialities: "Specialities",
                TodayAppointments: "TodayAppointments",
                TodayCompletedVisits: "TodayCompletedVisits",
                TodayCancelledAppointments: "TodayCancelledAppointments",
                ConfirmedAppointments: "ConfirmedAppointments",
                CompletedAppointments: "CompletedAppointments",
                RemainingAppointments: "RemainingAppointments"
            };

            // =========================================================
            // Initialize
            // =========================================================

            $scope.init = function () {

                $scope.isLoading = true;
                $scope.errorMessage = "";

                authService.getCurrentUser()
                    .then(function (response) {

                        if (!response.IsSuccess) {
                            $scope.errorMessage = response.Message;
                            return null;
                        }

                        $scope.userInfo = response.Data;

                        if (!$scope.userInfo.Role) {

                            $scope.errorMessage = "User has no role.";
                            return null;
                        }

                        var role = $scope.userInfo.Role;

                        switch (role) {

                            case "Admin":
                                $scope.dashboardType = "Admin";
                                return dashboardService.getAdminDashboard();

                            case "Receptionist":
                                $scope.dashboardType = "Receptionist";
                                return dashboardService.getReceptionistDashboard();

                            case "Doctor":
                                $scope.dashboardType = "Doctor";
                                return dashboardService.getDoctorDashboard();

                            default:
                                $scope.errorMessage = "Unsupported user role.";
                                return null;
                        }
                    })
                    .then(function (response) {

                        if (!response) {
                            return;
                        }

                        // Map UI config to statistics
                        if (response.Statistics) {
                            angular.forEach(response.Statistics, function (stat) {
                                var config = $scope.statisticUiConfig[stat.Key];
                                if (config) {
                                    stat.Icon = config.icon;
                                    stat.ColorClass = config.colorClass;
                                    stat.LocalizationKey = config.localizationKey;
                                } else {
                                    stat.Icon = "fa-chart-bar";
                                    stat.ColorClass = "stat-primary";
                                    stat.LocalizationKey = "dashboard." + stat.Key;
                                }
                            });
                        }

                        $scope.dashboardData = response;
                    })
                    .catch(function (error) {

                        $scope.errorMessage =
                            error.data && error.data.Message
                                ? error.data.Message
                                : "An error occurred while loading dashboard.";
                    })
                    .finally(function () {
                        $scope.isLoading = false;
                    });
            };

            // =========================================================
            // Statistic Click
            // =========================================================

            $scope.selectStatistic = function (statistic) {

                // Toggle off if clicking the same statistic
                if ($scope.selectedStatistic &&
                    $scope.selectedStatistic.Key === statistic.Key) {

                    $scope.selectedStatistic = null;
                    $scope.selectedDetails = null;
                    $scope.detailsError = "";
                    return;
                }

                $scope.selectedStatistic = statistic;
                $scope.selectedDetails = null;
                $scope.isLoadingDetails = false;
                $scope.detailsError = "";

                if ($scope.dashboardData) {
                    var propertyName = detailsMap[statistic.Key];
                    if (propertyName && $scope.dashboardData[propertyName]) {
                        $scope.selectedDetails = $scope.dashboardData[propertyName];
                    }
                }
                
                if (!$scope.selectedDetails) {
                    $scope.detailsError = "Details not found for " + statistic.Key;
                }
            };

            // =========================================================
            // Helper - Format TimeSpan for display
            // =========================================================

            $scope.formatTime = function (timeValue) {
                if (!timeValue) return "";

                // Handle TimeSpan serialized as "HH:mm:ss" string
                if (typeof timeValue === "string") {
                    var parts = timeValue.split(":");
                    if (parts.length >= 2) {
                        var h = parseInt(parts[0], 10);
                        var m = parts[1];
                        var ampm = h >= 12 ? "PM" : "AM";
                        h = h % 12 || 12;
                        return h + ":" + m + " " + ampm;
                    }
                    return timeValue;
                }

                // Handle if it's a Date object
                if (timeValue instanceof Date) {
                    var hours = timeValue.getHours();
                    var minutes = timeValue.getMinutes();
                    var suffix = hours >= 12 ? "PM" : "AM";
                    hours = hours % 12 || 12;
                    minutes = minutes < 10 ? "0" + minutes : minutes;
                    return hours + ":" + minutes + " " + suffix;
                }

                return timeValue.toString();
            };

            // =========================================================
            // Helper - Get day name
            // =========================================================

            $scope.getDayName = function (dayOfWeek) {
                var days = [
                    "dashboard.Sunday",
                    "dashboard.Monday",
                    "dashboard.Tuesday",
                    "dashboard.Wednesday",
                    "dashboard.Thursday",
                    "dashboard.Friday",
                    "dashboard.Saturday"
                ];
                return days[dayOfWeek] || "";
            };

            // =========================================================
            // Boot
            // =========================================================

            $scope.init();
        }
    ]);

})();