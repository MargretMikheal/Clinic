(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("ScheduleController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            "permissionService",
            "ScreenKeys",
            function ($scope, $http, notificationService, confirmationService, $filter, permissionService, ScreenKeys) {

                // =========================================================
                // State Variables
                // =========================================================
                $scope.isLoading = false;
                $scope.isSaving = false;
                $scope.isError = false;
                $scope.errorMessage = "";

                // Role Mode State
                $scope.isAdmin = false;
                $scope.isDoctor = false;
                $scope.doctorList = [];
                $scope.selectedDoctorId = null;
                $scope.isLoadingDoctors = false;

                $scope.permissions = {
                    Show: false,
                    Add: false,
                    Edit: false,
                    Delete: false
                };

                $scope.doctorInfo = {
                    DoctorId: 0,
                    DoctorName: "",
                    DoctorCode: "",
                    SlotDuration: 0
                };

                $scope.schedules = [];
                $scope.weeklyDays = [];

                $scope.totalWorkingDays = 0;
                $scope.totalPeriods = 0;
                $scope.totalWeeklyHours = 0;

                // Form / Modal State
                $scope.showModal = false;
                $scope.modalMode = "add"; // "add" | "edit"
                $scope.activePeriod = {
                    Id: 0,
                    DayOfWeek: 0,
                    StartTimeObj: null,
                    EndTimeObj: null
                };
                $scope.formError = "";

                // Weekday definitions (0 = Sunday ... 6 = Saturday)
                $scope.daysOfWeekOptions = [
                    { id: 0, key: "common.Sunday" },
                    { id: 1, key: "common.Monday" },
                    { id: 2, key: "common.Tuesday" },
                    { id: 3, key: "common.Wednesday" },
                    { id: 4, key: "common.Thursday" },
                    { id: 5, key: "common.Friday" },
                    { id: 6, key: "common.Saturday" }
                ];

                // =========================================================
                // Initialization
                // =========================================================
                $scope.init = function () {
                    permissionService.loadPermissions().then(function () {
                        $scope.permissions.Show = permissionService.hasPermission(ScreenKeys.MySchedule, "Show");
                        $scope.permissions.Add = permissionService.hasPermission(ScreenKeys.MySchedule, "Add");
                        $scope.permissions.Edit = permissionService.hasPermission(ScreenKeys.MySchedule, "Edit");
                        $scope.permissions.Delete = permissionService.hasPermission(ScreenKeys.MySchedule, "Delete");

                        if ($scope.permissions.Show) {
                            checkRoleAndInitialize();
                        }
                    }).catch(function (err) {
                        console.error("Error loading permissions", err);
                        $scope.isError = true;
                    });
                };

                function checkRoleAndInitialize() {
                    $http.get("/Doctor/Schedule/GetCurrentRole")
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess && response.data.Data) {
                                $scope.isAdmin = !!response.data.Data.IsAdmin;
                                $scope.isDoctor = !!response.data.Data.IsDoctor;

                                if ($scope.isAdmin) {
                                    loadActiveDoctors();
                                } else if ($scope.isDoctor) {
                                    $scope.loadSchedule();
                                } else {
                                    $scope.isError = true;
                                    $scope.errorMessage = translateMessage("schedule.Unauthorized");
                                }
                            } else {
                                $scope.isError = true;
                                $scope.errorMessage = translateMessage("schedule.ErrorLoadingSchedule");
                                notificationService.error($scope.errorMessage);
                            }
                        })
                        .catch(function (err) {
                            console.error("Error checking role", err);
                            $scope.isError = true;
                            $scope.errorMessage = translateMessage("schedule.ErrorLoadingSchedule");
                            notificationService.error($scope.errorMessage);
                        });
                }

                function loadActiveDoctors() {
                    $scope.isLoadingDoctors = true;
                    $http.get("/Doctor/Schedule/GetActiveDoctors")
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess) {
                                $scope.doctorList = response.data.Data || [];
                            } else {
                                notificationService.error(translateMessage("schedule.ErrorLoadingSchedule"));
                            }
                        })
                        .catch(function (err) {
                            console.error("Error loading doctors", err);
                            notificationService.error(translateMessage("schedule.ErrorLoadingSchedule"));
                        })
                        .finally(function () {
                            $scope.isLoadingDoctors = false;
                        });
                }

                $scope.onDoctorChanged = function () {
                    var docId = parseInt($scope.selectedDoctorId, 10);
                    if (docId > 0) {
                        $scope.selectedDoctorId = docId;
                        clearScheduleData();
                        $scope.loadSchedule(docId);
                    } else {
                        $scope.selectedDoctorId = null;
                        clearScheduleData();
                    }
                };

                function clearScheduleData() {
                    $scope.doctorInfo = {
                        DoctorId: 0,
                        DoctorName: "",
                        DoctorCode: "",
                        SlotDuration: 0
                    };
                    $scope.schedules = [];
                    $scope.weeklyDays = [];
                    $scope.totalWorkingDays = 0;
                    $scope.totalPeriods = 0;
                    $scope.totalWeeklyHours = 0;
                    $scope.isError = false;
                    $scope.errorMessage = "";
                }

                // =========================================================
                // Load Schedule
                // =========================================================
                $scope.loadSchedule = function (doctorId) {
                    var targetDocId = doctorId || $scope.selectedDoctorId;
                    if ($scope.isAdmin && (!targetDocId || targetDocId <= 0)) {
                        clearScheduleData();
                        return;
                    }

                    $scope.isLoading = true;
                    $scope.isError = false;
                    $scope.errorMessage = "";

                    var params = {};
                    if ($scope.isAdmin && targetDocId) {
                        params.doctorId = targetDocId;
                    }

                    $http.get("/Doctor/Schedule/GetMySchedule", { params: params })
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess) {
                                var data = response.data.Data;
                                if (data) {
                                    $scope.doctorInfo = {
                                        DoctorId: data.DoctorId,
                                        DoctorName: data.DoctorName,
                                        DoctorCode: data.DoctorCode,
                                        SlotDuration: data.SlotDuration
                                    };
                                    $scope.schedules = data.Schedules || [];
                                    buildWeeklySchedule();
                                } else {
                                    clearScheduleData();
                                }
                            } else {
                                $scope.isError = true;
                                var msg = response.data ? response.data.Message : null;
                                $scope.errorMessage = translateMessage(msg || "schedule.ErrorLoadingSchedule");
                                notificationService.error($scope.errorMessage);
                            }
                        })
                        .catch(function (error) {
                            console.error("Error loading schedule", error);
                            $scope.isError = true;
                            $scope.errorMessage = translateMessage("schedule.ErrorLoadingSchedule");
                            notificationService.error($scope.errorMessage);
                        })
                        .finally(function () {
                            $scope.isLoading = false;
                        });
                };

                // =========================================================
                // Build Seven-Day Weekly Schedule (Recurring Hours)
                // =========================================================
                function format12Hour(timeStr) {
                    if (!timeStr) return "";
                    var parts = timeStr.split(":");
                    if (parts.length < 2) return timeStr;
                    var h = parseInt(parts[0], 10);
                    var m = parts[1];
                    var ampm = h >= 12 ? "PM" : "AM";
                    var h12 = h % 12;
                    if (h12 === 0) h12 = 12;
                    var hStr = (h12 < 10 ? "0" : "") + h12;
                    return hStr + ":" + m + " " + ampm;
                }

                function buildWeeklySchedule() {
                    var days = [];
                    var totalPeriodsCount = 0;
                    var totalWorkingMinutes = 0;
                    var activeDaysCount = 0;

                    for (var i = 0; i < 7; i++) {
                        var dayId = i;

                        // Filter recurring periods for this day of week
                        var dayPeriods = ($scope.schedules || []).filter(function (s) {
                            return s.DayOfWeek === dayId;
                        }).sort(function (a, b) {
                            return (a.StartTime || "").localeCompare(b.StartTime || "");
                        });

                        var dayTotalMinutes = 0;
                        angular.forEach(dayPeriods, function (p) {
                            var dur = calculateDurationMinutes(p.StartTime, p.EndTime);
                            p.durationMinutes = dur;
                            p.durationLabel = formatDurationLabel(dur);
                            p.formattedTimeRange = format12Hour(p.StartTime) + " – " + format12Hour(p.EndTime);
                            dayTotalMinutes += dur;
                        });

                        if (dayPeriods.length > 0) {
                            activeDaysCount++;
                            totalPeriodsCount += dayPeriods.length;
                            totalWorkingMinutes += dayTotalMinutes;
                        }

                        days.push({
                            id: dayId,
                            nameKey: $scope.daysOfWeekOptions[dayId].key,
                            periods: dayPeriods,
                            isDayOff: dayPeriods.length === 0,
                            totalDayMinutes: dayTotalMinutes,
                            totalDayLabel: formatDurationLabel(dayTotalMinutes)
                        });
                    }

                    $scope.weeklyDays = days;
                    $scope.totalWorkingDays = activeDaysCount;
                    $scope.totalPeriods = totalPeriodsCount;
                    $scope.totalWeeklyHours = Math.round((totalWorkingMinutes / 60) * 10) / 10;
                }

                function calculateDurationMinutes(startTime, endTime) {
                    if (!startTime || !endTime) return 0;
                    var sp = startTime.split(":");
                    var ep = endTime.split(":");
                    var sm = parseInt(sp[0], 10) * 60 + parseInt(sp[1], 10);
                    var em = parseInt(ep[0], 10) * 60 + parseInt(ep[1], 10);
                    return Math.max(0, em - sm);
                }

                function formatDurationLabel(minutes) {
                    if (!minutes || minutes <= 0) return "";
                    var h = Math.floor(minutes / 60);
                    var m = minutes % 60;
                    if (h > 0 && m > 0) {
                        return h + "h " + m + "m";
                    } else if (h > 0) {
                        return h + "h";
                    } else {
                        return m + "m";
                    }
                }

                // =========================================================
                // Time Helpers for HTML5 <input type="time"> (Date Object)
                // =========================================================
                function parseTimeSpanString(timeString) {
                    if (!timeString) return new Date(2026, 0, 1, 9, 0, 0);
                    var parts = timeString.split(':');
                    var h = parseInt(parts[0], 10) || 0;
                    var m = parseInt(parts[1], 10) || 0;
                    return new Date(2026, 0, 1, h, m, 0);
                }

                function formatTimeObjToTimeString(dateObj) {
                    if (!dateObj) return "00:00";
                    if (typeof dateObj === "string") return dateObj.substring(0, 5);
                    if (dateObj instanceof Date) {
                        var h = String(dateObj.getHours()).padStart(2, '0');
                        var m = String(dateObj.getMinutes()).padStart(2, '0');
                        return h + ":" + m;
                    }
                    return "00:00";
                }

                // =========================================================
                // Period Add / Edit Modal Actions
                // =========================================================
                $scope.openAddPeriod = function (dayId) {
                    if (!$scope.permissions.Add) return;
                    if ($scope.isAdmin && (!$scope.selectedDoctorId || $scope.selectedDoctorId <= 0)) {
                        notificationService.warning(translateMessage("schedule.PleaseSelectDoctor"));
                        return;
                    }

                    var targetDay = (typeof dayId === "number") ? dayId : 0;
                    $scope.modalMode = "add";
                    $scope.activePeriod = {
                        Id: 0,
                        DoctorId: $scope.isAdmin ? $scope.selectedDoctorId : $scope.doctorInfo.DoctorId,
                        DayOfWeek: targetDay,
                        StartTimeObj: new Date(2026, 0, 1, 9, 0, 0),
                        EndTimeObj: new Date(2026, 0, 1, 17, 0, 0)
                    };
                    $scope.formError = "";
                    $scope.showModal = true;
                };

                $scope.openEditPeriod = function (period) {
                    if (!$scope.permissions.Edit) return;

                    $scope.modalMode = "edit";
                    $scope.activePeriod = {
                        Id: period.Id,
                        DoctorId: period.DoctorId || $scope.doctorInfo.DoctorId,
                        DayOfWeek: period.DayOfWeek,
                        StartTimeObj: parseTimeSpanString(period.StartTime),
                        EndTimeObj: parseTimeSpanString(period.EndTime)
                    };
                    $scope.formError = "";
                    $scope.showModal = true;
                };

                $scope.closeModal = function () {
                    if ($scope.isSaving) return;
                    $scope.showModal = false;
                    $scope.formError = "";
                };

                // =========================================================
                // Save Period
                // =========================================================
                $scope.savePeriod = function () {
                    $scope.formError = "";

                    var period = $scope.activePeriod;
                    if (!period.StartTimeObj || !period.EndTimeObj) {
                        $scope.formError = translateMessage("common.Required");
                        return;
                    }

                    var startTimeStr = formatTimeObjToTimeString(period.StartTimeObj);
                    var endTimeStr = formatTimeObjToTimeString(period.EndTimeObj);

                    if (startTimeStr >= endTimeStr) {
                        $scope.formError = translateMessage("schedule.InvalidTimeRange");
                        return;
                    }

                    var dur = calculateDurationMinutes(startTimeStr, endTimeStr);
                    if (dur <= 0) {
                        $scope.formError = translateMessage("schedule.ZeroDurationError");
                        return;
                    }

                    var slotDuration = $scope.doctorInfo.SlotDuration || 0;
                    if (slotDuration > 0 && dur < slotDuration) {
                        $scope.formError = translateMessage("schedule.PeriodShorterThanSlot", [slotDuration]);
                        return;
                    }

                    // Client-side overlap validation
                    var dayPeriods = ($scope.schedules || []).filter(function (s) {
                        return s.DayOfWeek === period.DayOfWeek && s.Id !== period.Id;
                    });

                    var isOverlapping = dayPeriods.some(function (s) {
                        var sStart = s.StartTime ? s.StartTime.substring(0, 5) : "";
                        var sEnd = s.EndTime ? s.EndTime.substring(0, 5) : "";
                        return (startTimeStr < sEnd && sStart < endTimeStr);
                    });

                    if (isOverlapping) {
                        $scope.formError = translateMessage("schedule.OverlappingPeriod");
                        return;
                    }

                    $scope.isSaving = true;

                    var docId = $scope.isAdmin ? $scope.selectedDoctorId : $scope.doctorInfo.DoctorId;
                    var payload = {
                        dto: {
                            Id: period.Id,
                            DoctorId: docId,
                            DayOfWeek: period.DayOfWeek,
                            StartTime: startTimeStr,
                            EndTime: endTimeStr
                        },
                        doctorId: $scope.isAdmin ? docId : null
                    };

                    var url = ($scope.modalMode === "add")
                        ? "/Doctor/Schedule/AddPeriod"
                        : "/Doctor/Schedule/UpdatePeriod";

                    $http.post(url, payload)
                        .then(function (response) {
                            if (response.data && response.data.IsSuccess) {
                                var msg = translateMessage(response.data.Message || "schedule.PeriodSavedSuccess");
                                notificationService.success(msg);
                                $scope.showModal = false;
                                $scope.loadSchedule($scope.isAdmin ? $scope.selectedDoctorId : null);
                            } else {
                                var errMsg = response.data ? response.data.Message : null;
                                $scope.formError = translateMessage(errMsg || "common.SomethingWentWrong");
                                notificationService.error($scope.formError);
                            }
                        })
                        .catch(function (error) {
                            console.error("Error saving period", error);
                            $scope.formError = translateMessage("common.SomethingWentWrong");
                            notificationService.error($scope.formError);
                        })
                        .finally(function () {
                            $scope.isSaving = false;
                        });
                };

                // =========================================================
                // Delete Period
                // =========================================================
                $scope.deletePeriod = function (period) {
                    if (!$scope.permissions.Delete) return;

                    var confirmText = $filter("i18n")("schedule.ConfirmDeletePeriod") || "Are you sure you want to delete this working period?";
                    confirmationService.confirm(confirmText).then(function (isConfirmed) {
                        if (!isConfirmed) return;

                        $scope.isLoading = true;
                        var docId = $scope.isAdmin ? $scope.selectedDoctorId : null;
                        var payload = {
                            id: period.Id,
                            doctorId: docId
                        };

                        $http.post("/Doctor/Schedule/DeletePeriod", payload)
                            .then(function (response) {
                                if (response.data && response.data.IsSuccess) {
                                    var msg = translateMessage(response.data.Message || "schedule.PeriodDeletedSuccess");
                                    notificationService.success(msg);
                                    $scope.loadSchedule($scope.isAdmin ? $scope.selectedDoctorId : null);
                                } else {
                                    var errMsg = response.data ? response.data.Message : null;
                                    var translated = translateMessage(errMsg || "common.SomethingWentWrong");
                                    notificationService.error(translated);
                                }
                            })
                            .catch(function (error) {
                                console.error("Error deleting period", error);
                                notificationService.error(translateMessage("common.SomethingWentWrong"));
                            })
                            .finally(function () {
                                $scope.isLoading = false;
                            });
                    });
                };

                // =========================================================
                // Helpers
                // =========================================================
                $scope.getSlotHintText = function () {
                    var dur = $scope.doctorInfo.SlotDuration || 0;
                    return translateMessage("schedule.PeriodShorterThanSlot", [dur]);
                };

                function translateMessage(key, params) {
                    if (!key) return "";

                    if (key.indexOf("::") !== -1) {
                        var parts = key.split("::");
                        var baseKey = parts[0];
                        var subParams = parts.slice(1);
                        var rawTranslated = $filter("i18n")(baseKey);
                        if (rawTranslated && rawTranslated !== baseKey) {
                            for (var p = 0; p < subParams.length; p++) {
                                rawTranslated = rawTranslated.replace("{" + p + "}", subParams[p]);
                            }
                            return rawTranslated;
                        }
                        return baseKey + " (" + subParams.join(", ") + ")";
                    }

                    var translated = $filter("i18n")(key);
                    if (translated && translated !== key) {
                        if (params && params.length) {
                            for (var i = 0; i < params.length; i++) {
                                translated = translated.replace("{" + i + "}", params[i]);
                            }
                        }
                        return translated;
                    }

                    return key;
                }

                $scope.translateMessage = translateMessage;

                // Start
                $scope.init();
            }
        ]);
})();