(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("AppointmentController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            "permissionService",
            "ScreenKeys",
            function ($scope, $http, notificationService, confirmationService, $filter, permissionService, ScreenKeys) {

                $scope.appointments = [];
                $scope.doctors = [];
                $scope.patients = [];

                $scope.searchQuery = "";
                $scope.filterDoctorId = "";
                $scope.filterFromDate = null;
                $scope.filterToDate = null;
                $scope.filterStatus = "";

                $scope.isLoading = false;
                $scope.isSaving = false;
                $scope.isError = false;

                $scope.viewState = "list";
                $scope.currentAppointment = {};
                $scope.statusForm = {};
                $scope.availableSlots = [];
                $scope.isLoadingSlots = false;

                $scope.permissions = {
                    Show: false,
                    Add: false,
                    Edit: false,
                    Delete: false
                };

                $scope.pagination = {
                    page: 1,
                    pageSize: 10,
                    totalItems: 0
                };

                $scope.filteredAppointments = [];

                $scope.formatTimeSpan = function (timeSpan) {
                    if (!timeSpan) return "";

                    var hours = 0;
                    var minutes = 0;

                    if (typeof timeSpan === 'string') {
                        var parts = timeSpan.split(':');
                        if (parts.length >= 2) {
                            hours = parseInt(parts[0], 10) || 0;
                            minutes = parseInt(parts[1], 10) || 0;
                        }
                    } else if (typeof timeSpan === 'object') {
                        if (timeSpan.Hours !== undefined && timeSpan.Minutes !== undefined) {
                            hours = timeSpan.Hours;
                            minutes = timeSpan.Minutes;
                        } else {
                            return "";
                        }
                    } else {
                        return "";
                    }

                    var hh = (hours < 10 ? '0' : '') + hours;
                    var mm = (minutes < 10 ? '0' : '') + minutes;
                    return hh + ':' + mm;
                };

                $scope.getStatusLocalizationKey = function (status) {
                    switch (status) {
                        case 'Booked': return 'appointment.Booked';
                        case 'Confirmed': return 'appointment.Confirmed';
                        case 'Completed': return 'appointment.Completed';
                        case 'Cancelled': return 'appointment.Cancelled';
                        case 'NoShow': return 'appointment.NoShow';
                        default: return '';
                    }
                };

                $scope.getStatusBadgeClass = function (status) {
                    switch (status) {
                        case 'Booked': return 'c-badge c-badge-booked';
                        case 'Confirmed': return 'c-badge c-badge-confirmed';
                        case 'Completed': return 'c-badge c-badge-completed';
                        case 'Cancelled': return 'c-badge c-badge-cancelled';
                        case 'NoShow': return 'c-badge c-badge-noshow';
                        default: return 'c-badge c-badge-neutral';
                    }
                };

                function parseDate(dateStr) {
                    if (!dateStr) return null;
                    if (typeof dateStr === 'string' && dateStr.indexOf('/Date(') === 0) {
                        return new Date(parseInt(dateStr.substr(6)));
                    }
                    return new Date(dateStr);
                }

                $scope.isAppointmentExpired = function(appt) {
                    if (!appt || !appt.AppointmentDate) return false;
                    
                    var apptDate = parseDate(appt.AppointmentDate);
                    if (!apptDate) return false;

                    var today = new Date();
                    today.setHours(0, 0, 0, 0);

                    var apptDay = new Date(apptDate.getFullYear(), apptDate.getMonth(), apptDate.getDate(), 0, 0, 0, 0);
                    return apptDay < today;
                };

                var filterTimeout = null;
                $scope.$watchGroup(['searchQuery', 'filterDoctorId', 'filterFromDate', 'filterToDate', 'filterStatus'], function (newValues, oldValues) {
                    var filtersChanged = false;
                    for (var i = 0; i < newValues.length; i++) {
                        if (newValues[i] !== oldValues[i]) {
                            filtersChanged = true;
                            break;
                        }
                    }

                    if (filtersChanged) {
                        var fromStr = $scope.filterFromDate ? $filter('date')($scope.filterFromDate, 'yyyy-MM-dd') : null;
                        var toStr = $scope.filterToDate ? $filter('date')($scope.filterToDate, 'yyyy-MM-dd') : null;

                        if (fromStr && toStr && fromStr > toStr) {
                            notificationService.error($filter('i18n')('appointment.InvalidDateRange') || 'Invalid Date Range');
                            $scope.appointments = [];
                            $scope.filteredAppointments = [];
                            $scope.pagination.totalItems = 0;
                            return;
                        }

                        if (filterTimeout) {
                            clearTimeout(filterTimeout);
                        }
                        filterTimeout = setTimeout(function() {
                            $scope.$apply(function() {
                                $scope.pagination.page = 1;
                                loadAppointments();
                            });
                        }, 400);
                    }
                });

                $scope.getPaginatedAppointments = function () {
                    var begin = (($scope.pagination.page - 1) * $scope.pagination.pageSize);
                    var end = begin + $scope.pagination.pageSize;
                    return $scope.filteredAppointments.slice(begin, end);
                };

                function loadPermissions() {
                    return permissionService.loadPermissions()
                        .then(function () {
                            $scope.permissions.Show = permissionService.hasPermission(ScreenKeys.Appointments, "Show");
                            $scope.permissions.Add = permissionService.hasPermission(ScreenKeys.Appointments, "Add");
                            $scope.permissions.Edit = permissionService.hasPermission(ScreenKeys.Appointments, "Edit");
                            $scope.permissions.Delete = permissionService.hasPermission(ScreenKeys.Appointments, "Delete");

                            if ($scope.permissions.Show) {
                                loadDoctors();
                                if ($scope.permissions.Add || $scope.permissions.Edit) {
                                    loadPatients();
                                }
                                loadAppointments();
                            } else {
                                $scope.isError = true;
                                notificationService.error($filter("i18n")("common.Unauthorized") || "Unauthorized");
                            }
                        })
                        .catch(function () {
                            $scope.isError = true;
                        });
                }

                function loadDoctors() {
                    return $http.get("/Reception/Appointment/GetAppointmentDoctors")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.doctors = response.data.Data || [];
                            }
                        });
                }

                function loadPatients() {
                    return $http.get("/Reception/Patient/GetPatients")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.patients = response.data.Data || [];
                            }
                        });
                }

                function loadAppointments() {
                    $scope.isLoading = true;
                    $scope.isError = false;

                    var params = {};
                    if ($scope.filterFromDate) params.fromDate = $filter('date')($scope.filterFromDate, 'yyyy-MM-dd');
                    if ($scope.filterToDate) params.toDate = $filter('date')($scope.filterToDate, 'yyyy-MM-dd');
                    if ($scope.filterDoctorId) params.doctorId = $scope.filterDoctorId;
                    if ($scope.filterStatus) params.status = $scope.filterStatus;
                    if ($scope.searchQuery) params.searchText = $scope.searchQuery;

                    return $http.get("/Reception/Appointment/GetAppointments", { params: params })
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
                                notificationService.error(response.data.Message);
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

                $scope.onDoctorOrDateChanged = function () {
                    $scope.currentAppointment.AppointmentTime = null;
                    $scope.availableSlots = [];

                    if (!$scope.currentAppointment.DoctorId || !$scope.currentAppointment.AppointmentDate) {
                        return;
                    }

                    $scope.isLoadingSlots = true;

                    var d = $scope.currentAppointment.AppointmentDate;
                    var dateString = d.getFullYear() + '-' + ('0' + (d.getMonth() + 1)).slice(-2) + '-' + ('0' + d.getDate()).slice(-2);

                    $http.get("/Reception/Appointment/GetAvailableSlots", { params: { doctorId: $scope.currentAppointment.DoctorId, date: dateString } })
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.availableSlots = response.data.Data || [];
                            } else {
                                notificationService.error(response.data.Message || $filter('i18n')('appointment.ErrorLoadingSlots'));
                            }
                        })
                        .catch(function () {
                            notificationService.error($filter('i18n')('appointment.ErrorLoadingSlots'));
                        })
                        .finally(function () {
                            $scope.isLoadingSlots = false;
                        });
                };

                $scope.openAdd = function () {
                    if (!$scope.permissions.Add) return;
                    $scope.viewState = "add";
                    $scope.availableSlots = [];
                    $scope.currentAppointment = {
                        PatientId: "",
                        DoctorId: "",
                        AppointmentDate: new Date(),
                        AppointmentTime: null,
                        Notes: ""
                    };
                };

                $scope.openEdit = function (appt) {
                    if (!$scope.permissions.Edit) return;
                    if (appt.Status === 'Completed' || appt.Status === 'Cancelled' || $scope.isAppointmentExpired(appt)) {
                        notificationService.error($filter('i18n')('appointment.AppointmentCannotBeEdited'));
                        return;
                    }

                    $scope.viewState = "edit";
                    $scope.availableSlots = [];
                    $scope.currentAppointment = {
                        Id: appt.Id,
                        PatientId: appt.PatientId,
                        DoctorId: appt.DoctorId,
                        AppointmentDate: parseDate(appt.AppointmentDate),
                        Notes: appt.Notes
                    };

                    $scope.isLoadingSlots = true;
                    var d = $scope.currentAppointment.AppointmentDate;
                    var dateString = d.getFullYear() + '-' + ('0' + (d.getMonth() + 1)).slice(-2) + '-' + ('0' + d.getDate()).slice(-2);

                    $http.get("/Reception/Appointment/GetAvailableSlots", { params: { doctorId: $scope.currentAppointment.DoctorId, date: dateString } })
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.availableSlots = response.data.Data || [];

                                var timeStr = appt.AppointmentTime;
                                var exists = $scope.availableSlots.some(function (s) { return s.Time === timeStr; });
                                if (!exists) {
                                    $scope.availableSlots.unshift({ Time: timeStr });
                                }
                                $scope.currentAppointment.AppointmentTime = timeStr;
                            }
                        })
                        .finally(function () {
                            $scope.isLoadingSlots = false;
                        });
                };

                $scope.openView = function (appt) {
                    $scope.viewState = "view";
                    $scope.currentAppointment = angular.copy(appt);
                    $scope.currentAppointment.AppointmentDate = parseDate(appt.AppointmentDate);
                };

                $scope.openChangeStatus = function (appt) {
                    if (!$scope.permissions.Edit || appt.Status === 'Completed' || $scope.isAppointmentExpired(appt)) return;
                    $scope.viewState = "changeStatus";
                    $scope.statusForm = {
                        AppointmentId: appt.Id,
                        CurrentStatus: appt.Status,
                        Status: ""
                    };
                };

                $scope.cancel = function () {
                    $scope.viewState = "list";
                    $scope.currentAppointment = {};
                    $scope.statusForm = {};
                };

                $scope.clearFilters = function () {
                    $scope.filterFromDate = null;
                    $scope.filterToDate = null;
                    $scope.filterDoctorId = "";
                    $scope.filterStatus = "";
                    $scope.searchQuery = "";
                    $scope.pagination.page = 1;
                };

                function handleServerFailure(message, fallbackKey) {
                    var text = message && message.indexOf("appointment.") === 0
                        ? $filter("i18n")(message)
                        : (message || $filter("i18n")(fallbackKey));
                    notificationService.error(text);

                    if (message === "appointment.AppointmentExpired") {
                        $scope.viewState = "list";
                        $scope.currentAppointment = {};
                        $scope.statusForm = {};
                        loadAppointments();
                    }
                }

                $scope.saveAppointment = function () {
                    if ($scope.isSaving) return;
                    $scope.isSaving = true;

                    var url = $scope.viewState === "add" ? "/Reception/Appointment/AddAppointment" : "/Reception/Appointment/EditAppointment";

                    var payload = angular.copy($scope.currentAppointment);
                    if (payload.AppointmentDate) {
                        var d = payload.AppointmentDate;
                        payload.AppointmentDate = d.getFullYear() + '-' + ('0' + (d.getMonth() + 1)).slice(-2) + '-' + ('0' + d.getDate()).slice(-2);
                    }

                    $http.post(url, payload)
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                var msg = $scope.viewState === "add" ? "AppointmentSavedSuccess" : "AppointmentUpdatedSuccess";
                                notificationService.success($filter("i18n")("appointment." + msg));
                                $scope.viewState = "list";
                                $scope.currentAppointment = {};
                                return loadAppointments();
                            }
                            var errorMsg = response.data.Message;
                            if (response.data.Errors && response.data.Errors.length > 0) {
                                errorMsg = response.data.Errors.join(", ");
                            }
                            handleServerFailure(errorMsg, "appointment.ErrorSavingAppointment");
                        })
                        .catch(function (error) {
                            var errorMsg = $filter("i18n")("appointment.ErrorSavingAppointment");
                            if (error && error.data) {
                                if (error.data.Message) errorMsg = error.data.Message;
                                if (error.data.ExceptionMessage) errorMsg = error.data.ExceptionMessage;
                            }
                            notificationService.error(errorMsg);
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                $scope.saveStatus = function () {
                    if ($scope.isSaving) return;
                    $scope.isSaving = true;

                    var payload = {
                        AppointmentId: $scope.statusForm.AppointmentId,
                        Status: $scope.statusForm.Status
                    };

                    $http.post("/Reception/Appointment/ChangeStatus", payload)
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                notificationService.success($filter("i18n")("appointment.AppointmentStatusUpdatedSuccess"));
                                $scope.viewState = "list";
                                $scope.statusForm = {};
                                return loadAppointments();
                            }
                            handleServerFailure(response.data.Message, "appointment.AppointmentCannotChangeStatus");
                        })
                        .catch(function (error) {
                            notificationService.error($filter("i18n")("appointment.AppointmentCannotChangeStatus"));
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                $scope.deleteAppointment = function (id) {
                    if ($scope.isLoading || !$scope.permissions.Delete) return;

                    confirmationService.confirm($filter("i18n")("common.Confirm") || "Are you sure?")
                        .then(function (confirm) {
                            if (!confirm) return;

                            $scope.isLoading = true;

                            $http.post("/Reception/Appointment/DeleteAppointment", { id: id })
                                .then(function (response) {
                                    if (response.data.IsSuccess) {
                                        notificationService.success($filter("i18n")("appointment.AppointmentDeletedSuccess"));
                                        return loadAppointments();
                                    }
                                    notificationService.error(response.data.Message || $filter("i18n")("appointment.ErrorDeletingAppointment"));
                                })
                                .catch(function () {
                                    notificationService.error($filter("i18n")("appointment.ErrorDeletingAppointment"));
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
