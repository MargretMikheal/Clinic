(function () {
    "use strict";

    angular.module("clinicApp")
        .controller("DoctorsController", [
            "$scope",
            "$http",
            "notificationService",
            "confirmationService",
            "$filter",
            "permissionService",
            "ScreenKeys",
            function ($scope, $http, notificationService, confirmationService, $filter, permissionService, ScreenKeys) {

                // ----------------------------------------------------
                // DOCTORS STATE & LOGIC
                // ----------------------------------------------------
                $scope.doctors = [];
                $scope.specialities = [];
                $scope.receptionists = [];

                $scope.isDoctorLoading = false;
                $scope.isDoctorSaving = false;
                $scope.isDoctorError = false;

                $scope.permissions = { Show: false, Add: false, Edit: false, Delete: false, Print: false };

                $scope.doctorViewState = "list";
                $scope.currentDoctor = {};

                $scope.daysOfWeek = [
                    { id: 0, name: $filter("i18n")("common.Sunday") || "Sunday" },
                    { id: 1, name: $filter("i18n")("common.Monday") || "Monday" },
                    { id: 2, name: $filter("i18n")("common.Tuesday") || "Tuesday" },
                    { id: 3, name: $filter("i18n")("common.Wednesday") || "Wednesday" },
                    { id: 4, name: $filter("i18n")("common.Thursday") || "Thursday" },
                    { id: 5, name: $filter("i18n")("common.Friday") || "Friday" },
                    { id: 6, name: $filter("i18n")("common.Saturday") || "Saturday" }
                ];

                $scope.$watch('selectedTabIndex', function(newVal) {
                    if (newVal === 1) {
                        permissionService.loadPermissions().then(function () {
                            $scope.permissions.Show = permissionService.hasPermission(ScreenKeys.Users, "Show");
                            $scope.permissions.Add = permissionService.hasPermission(ScreenKeys.Users, "Add");
                            $scope.permissions.Edit = permissionService.hasPermission(ScreenKeys.Users, "Edit");
                            $scope.permissions.Delete = permissionService.hasPermission(ScreenKeys.Users, "Delete");
                            $scope.permissions.Print = permissionService.hasPermission(ScreenKeys.Users, "Print");

                            if ($scope.permissions.Show && $scope.doctors.length === 0) {
                                loadDoctors();
                                loadSpecialities();
                                loadReceptionists();
                            }
                        });
                    }
                });

                function loadDoctors() {
                    $scope.isDoctorLoading = true;
                    $scope.isDoctorError = false;

                    return $http.get("/Administration/Doctors/GetDoctors")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.doctors = response.data.Data || [];
                            } else {
                                $scope.isDoctorError = true;
                                notificationService.error(response.data.Message);
                            }
                        })
                        .catch(function () {
                            $scope.isDoctorError = true;
                            notificationService.error($filter("i18n")("users.ErrorLoadingDoctors"));
                        })
                        .finally(function () {
                            $scope.isDoctorLoading = false;
                        });
                }

                function loadSpecialities() {
                    return $http.get("/Administration/Doctors/GetDoctorSpecialities")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.specialities = response.data.Data || [];
                            }
                        });
                }

                function loadReceptionists() {
                    return $http.get("/Administration/Doctors/GetDoctorReceptionists")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.receptionists = response.data.Data || [];
                            }
                        });
                }

                $scope.openAddDoctor = function () {
                    if (!$scope.permissions.Add) return;
                    $scope.doctorViewState = "add";
                    $scope.currentDoctor = {
                        UserName: "",
                        Email: "",
                        Password: "",
                        DoctorCode: "",
                        Name: "",
                        SpecialityId: null,
                        Phone: "",
                        ExaminationPrice: 0,
                        SlotDuration: 15,
                        IsActive: true,
                        ReceptionistUserId: null,
                        Schedules: []
                    };
                    
                    $scope.isDoctorLoading = true;
                    $http.get("/Administration/Doctors/GetNextDoctorCode")
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.currentDoctor.DoctorCode = response.data.Data;
                            }
                        })
                        .finally(function () {
                            $scope.isDoctorLoading = false;
                        });
                };

                $scope.openEditDoctor = function (doctor) {
                    if (!$scope.permissions.Edit) return;
                    $scope.isDoctorLoading = true;
                    $http.get("/Administration/Doctors/GetDoctor?id=" + doctor.Id)
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                $scope.currentDoctor = response.data.Data;
                                
                                if ($scope.currentDoctor.Schedules) {
                                    $scope.currentDoctor.Schedules.forEach(function(s) {
                                        s.StartTimeObj = parseTimeSpanString(s.StartTime);
                                        s.EndTimeObj = parseTimeSpanString(s.EndTime);
                                    });
                                } else {
                                    $scope.currentDoctor.Schedules = [];
                                }

                                $scope.doctorViewState = "edit";
                            } else {
                                notificationService.error(response.data.Message);
                            }
                        })
                        .catch(function (error) {
                            console.error("Error loading doctor details:", error);
                            notificationService.error($filter("i18n")("users.ErrorLoadingDoctorDetails"));
                        })
                        .finally(function () {
                            $scope.isDoctorLoading = false;
                        });
                };

                function parseTimeSpanString(timeString) {
                    if (!timeString) return null;
                    var parts = timeString.split(':');
                    var date = new Date(2026, 0, 1, parts[0], parts[1], parts[2] ? parts[2].substring(0,2) : 0);
                    return date;
                }

                function formatTimeObjToTimeSpan(dateObj) {
                    if (!dateObj) return "00:00:00";
                    var h = String(dateObj.getHours()).padStart(2, '0');
                    var m = String(dateObj.getMinutes()).padStart(2, '0');
                    return h + ":" + m + ":00";
                }

                $scope.addSchedule = function () {
                    $scope.currentDoctor.Schedules.push({
                        Id: 0,
                        DayOfWeek: 0,
                        StartTimeObj: new Date(2026, 0, 1, 9, 0, 0),
                        EndTimeObj: new Date(2026, 0, 1, 17, 0, 0)
                    });
                };

                $scope.removeSchedule = function (index) {
                    $scope.currentDoctor.Schedules.splice(index, 1);
                };

                $scope.cancelDoctor = function () {
                    $scope.doctorViewState = "list";
                    $scope.currentDoctor = {};
                    loadDoctors();
                };

                $scope.saveDoctor = function () {
                    if ($scope.isDoctorSaving) return;
                    $scope.isDoctorSaving = true;

                    var payload = angular.copy($scope.currentDoctor);
                    if (payload.Schedules) {
                        for (var i = 0; i < payload.Schedules.length; i++) {
                            var s = payload.Schedules[i];
                            if (s.StartTimeObj >= s.EndTimeObj) {
                                notificationService.error($filter("i18n")("users.InvalidSchedule"));
                                $scope.isDoctorSaving = false;
                                return;
                            }
                            s.StartTime = formatTimeObjToTimeSpan(s.StartTimeObj);
                            s.EndTime = formatTimeObjToTimeSpan(s.EndTimeObj);
                        }
                    }

                    var url = $scope.doctorViewState === "add" 
                        ? "/Administration/Doctors/AddDoctor" 
                        : "/Administration/Doctors/EditDoctor";

                    $http.post(url, payload)
                        .then(function (response) {
                            if (response.data.IsSuccess) {
                                notificationService.success($filter("i18n")("users.DoctorSavedSuccess"));
                                $scope.doctorViewState = "list";
                                $scope.currentDoctor = {};
                                loadDoctors();
                            } else {
                                notificationService.error(response.data.Message);
                            }
                        })
                        .catch(function (error) {
                            console.error("Error saving doctor:", error);
                            notificationService.error($filter("i18n")("common.SomethingWentWrong"));
                        })
                        .finally(function () {
                            $scope.isDoctorSaving = false;
                        });
                };
            }
        ]);
})();
