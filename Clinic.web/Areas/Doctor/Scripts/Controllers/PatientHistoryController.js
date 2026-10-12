(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("PatientHistoryController", [
            "$scope",
            "$http",
            "$timeout",
            "notificationService",
            "$filter",
            "locale",
            "permissionService",
            "ScreenKeys",
            function ($scope, $http, $timeout, notificationService, $filter, locale, permissionService, ScreenKeys) {

                $scope.permissions = {
                    Show: false
                };
                $scope.isUnauthorized = false;

                // Role Information
                $scope.isAdmin = false;
                $scope.isDoctor = false;

                $scope.viewState = "list";

                $scope.patients = [];
                $scope.filteredPatients = [];
                $scope.searchQuery = "";

                // Maintain backwards compatibility for searchText binding
                Object.defineProperty($scope, "searchText", {
                    get: function () {
                        return $scope.searchQuery;
                    },
                    set: function (val) {
                        $scope.searchQuery = val;
                    },
                    enumerable: true,
                    configurable: true
                });

                $scope.pagination = {
                    page: 1,
                    pageSize: 10,
                    totalItems: 0
                };

                $scope.isLoadingPatients = false;
                $scope.isPatientListError = false;
                $scope.patientListErrorMessage = "";

                $scope.selectedPatient = null;
                $scope.historyDetails = null;
                $scope.isLoadingHistory = false;
                $scope.isHistoryError = false;
                $scope.historyErrorMessage = "";

                var currentHistoryRequestId = 0;

                // =========================================================
                // Client-Side Search & Pagination
                // =========================================================
                function applyFilter() {
                    var term = ($scope.searchQuery || "").trim().toLowerCase();

                    if (!term) {
                        $scope.filteredPatients = ($scope.patients || []).slice();
                    } else {
                        $scope.filteredPatients = ($scope.patients || []).filter(function (patient) {
                            if (!patient) return false;

                            var name = (patient.Name || "").toLowerCase();
                            var code = (patient.PatientCode || "").toLowerCase();
                            var phone = (patient.Phone || "").toLowerCase();
                            var email = (patient.Email || "").toLowerCase();

                            return name.indexOf(term) !== -1 ||
                                   code.indexOf(term) !== -1 ||
                                   phone.indexOf(term) !== -1 ||
                                   email.indexOf(term) !== -1;
                        });
                    }

                    $scope.pagination.totalItems = $scope.filteredPatients.length;
                    var totalPages = Math.ceil($scope.filteredPatients.length / $scope.pagination.pageSize) || 1;
                    if ($scope.pagination.page > totalPages) {
                        $scope.pagination.page = totalPages;
                    }
                }

                $scope.$watchGroup(["patients", "searchQuery"], function (newValues, oldValues) {
                    if (newValues && oldValues && newValues[1] !== oldValues[1]) {
                        $scope.pagination.page = 1;
                    }
                    applyFilter();
                });

                $scope.onSearchChanged = function () {
                    $scope.pagination.page = 1;
                    applyFilter();
                };

                $scope.clearSearch = function () {
                    $scope.searchQuery = "";
                    $scope.pagination.page = 1;
                    applyFilter();
                };

                $scope.getPaginatedPatients = function () {
                    if (!$scope.filteredPatients || $scope.filteredPatients.length === 0) {
                        return [];
                    }
                    var begin = (($scope.pagination.page - 1) * $scope.pagination.pageSize);
                    var end = begin + $scope.pagination.pageSize;
                    return $scope.filteredPatients.slice(begin, end);
                };

                // =========================================================
                // Localization Helpers (Reusing existing locale service)
                // =========================================================
                $scope.isArabic = function () {
                    return locale.getLocale() === "ar";
                };

                $scope.getLocalizedText = function (arText, enText) {
                    return locale.getLocale() === "ar"
                        ? (arText || enText || "")
                        : (enText || arText || "");
                };

                // =========================================================
                // Formatting Helpers
                // =========================================================
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

                $scope.calculateAge = function (birthDate) {
                    if (!birthDate) return null;
                    var dob = new Date(birthDate);
                    if (isNaN(dob.getTime())) return null;
                    var diffMs = Date.now() - dob.getTime();
                    var ageDate = new Date(diffMs);
                    return Math.abs(ageDate.getUTCFullYear() - 1970);
                };

                $scope.isMale = function (gender) {
                    if (!gender) return false;
                    var g = gender.toString().trim().toLowerCase();
                    return g === "male" || g === "ذكر" || g === "m";
                };

                $scope.isFemale = function (gender) {
                    if (!gender) return false;
                    var g = gender.toString().trim().toLowerCase();
                    return g === "female" || g === "أنثى" || g === "f";
                };

                // =========================================================
                // Data Loading: Role & Patients
                // =========================================================
                function loadRole() {
                    return $http.get("/Doctor/Patient/GetCurrentRole")
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess && response.data.Data) {
                                $scope.isAdmin = !!response.data.Data.IsAdmin;
                                $scope.isDoctor = !!response.data.Data.IsDoctor;
                            }
                        })
                        .catch(function () {
                            // Non-blocking, server-side authorization handles access
                        });
                }

                function loadPatients() {
                    $scope.isLoadingPatients = true;
                    $scope.isPatientListError = false;
                    $scope.patientListErrorMessage = "";

                    return $http.get("/Doctor/Patient/GetPatients")
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess) {
                                $scope.patients = response.data.Data || [];
                            } else {
                                $scope.isPatientListError = true;
                                $scope.patientListErrorMessage = (response.data && response.data.Message)
                                    || $filter("i18n")("patient.ErrorLoadingPatients");
                                notificationService.error($scope.patientListErrorMessage);
                            }
                        })
                        .catch(function () {
                            $scope.isPatientListError = true;
                            $scope.patientListErrorMessage = $filter("i18n")("patient.ErrorLoadingPatients")
                                || "Error loading patients.";
                            notificationService.error($scope.patientListErrorMessage);
                        })
                        .finally(function () {
                            $scope.isLoadingPatients = false;
                        });
                }

                // =========================================================
                // Patient Selection & History Details
                // =========================================================
                $scope.selectPatient = function (patient) {
                    if (!patient || !patient.Id) return;

                    $scope.viewState = "history";
                    $scope.selectedPatient = patient;
                    $scope.historyDetails = null;
                    $scope.isHistoryError = false;
                    $scope.historyErrorMessage = "";
                    $scope.isLoadingHistory = true;

                    var requestId = ++currentHistoryRequestId;

                    $http.get("/Doctor/Patient/GetPatientHistory", { params: { id: patient.Id } })
                        .then(function (response) {
                            // Discard if stale request or user navigated away
                            if (requestId !== currentHistoryRequestId || $scope.viewState !== "history") {
                                return;
                            }

                            if (response.data && response.data.IsSuccess && response.data.Data) {
                                $scope.historyDetails = response.data.Data;
                                if (response.data.Data.Patient) {
                                    $scope.selectedPatient = response.data.Data.Patient;
                                }
                            } else {
                                $scope.isHistoryError = true;
                                $scope.historyErrorMessage = (response.data && response.data.Message)
                                    || $filter("i18n")("patient.ErrorLoadingHistory");
                                notificationService.error($scope.historyErrorMessage);
                            }
                        })
                        .catch(function () {
                            if (requestId !== currentHistoryRequestId || $scope.viewState !== "history") return;
                            $scope.isHistoryError = true;
                            $scope.historyErrorMessage = $filter("i18n")("patient.ErrorLoadingHistory")
                                || "Error loading patient history.";
                            notificationService.error($scope.historyErrorMessage);
                        })
                        .finally(function () {
                            if (requestId === currentHistoryRequestId) {
                                $scope.isLoadingHistory = false;
                            }
                        });
                };

                // =========================================================
                // Back to Patient List Action
                // =========================================================
                $scope.backToList = function () {
                    // Invalidate in-flight history requests so they cannot overwrite or reopen
                    currentHistoryRequestId++;

                    $scope.viewState = "list";
                    $scope.selectedPatient = null;
                    $scope.historyDetails = null;
                    $scope.isLoadingHistory = false;
                    $scope.isHistoryError = false;
                    $scope.historyErrorMessage = "";
                    $scope.pagination.page = 1;
                };

                // =========================================================
                // Initialization
                // =========================================================
                $scope.init = function () {
                    permissionService.loadPermissions()
                        .then(function () {
                            $scope.permissions.Show = permissionService.hasPermission(ScreenKeys.PatientHistory, "Show");

                            if ($scope.permissions.Show) {
                                loadRole();
                                loadPatients();
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
        ])
        .controller("DoctorPatientController", ["$controller", "$scope", function ($controller, $scope) {
            $controller("PatientHistoryController", { $scope: $scope });
        }]);
})();
