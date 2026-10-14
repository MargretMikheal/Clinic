(function () {
    "use strict";

    function DoctorVisitController($scope, $http, notificationService, $filter, locale, permissionService, ScreenKeys, appointmentStatusService) {
        $scope.permissions = {
            Show: false,
            Add: false
        };
        $scope.isUnauthorized = false;

        $scope.appointments = [];
        $scope.filteredAppointments = [];
        $scope.selectedAppointment = null;

        $scope.searchQuery = "";
        $scope.isLoading = false;
        $scope.isError = false;
        $scope.errorMessage = "";

        // Patient History State
        $scope.showHistory = false;
        $scope.historyDetails = null;
        $scope.isLoadingHistory = false;
        $scope.isHistoryError = false;
        $scope.historyErrorMessage = "";

        var currentHistoryRequestId = 0;

        // Master Data for Clinical Form
        $scope.masterSymptoms = [];
        $scope.masterDiagnoses = [];
        $scope.isLoadingMasterData = false;
        $scope.selectedSymptomToAdd = null;
        $scope.selectedDiagnosisToAdd = null;

        // Clinical Form State & Saving
        $scope.visitForm = null;
        $scope.isSaving = false;
        $scope.isVisitSaved = false;
        $scope.savedVisitDetails = null;
        $scope.isSubmitted = false;
        $scope.validationErrors = [];
        $scope.isPrintingReport = false;

        // =========================================================
        // Client-Side Search Filtering
        // =========================================================
        $scope.$watchGroup(["appointments", "searchQuery"], function () {
            var term = ($scope.searchQuery || "").trim();
            if (!term) {
                $scope.filteredAppointments = $scope.appointments || [];
            } else {
                $scope.filteredAppointments = $filter("filter")($scope.appointments, term) || [];
            }
        });

        $scope.clearSearch = function () {
            $scope.searchQuery = "";
        };

        // =========================================================
        // Presentation Helpers
        // =========================================================
        $scope.isArabic = function () {
            return locale.getLocale() === "ar";
        };

        $scope.getLocalizedText = function (arText, enText) {
            return locale.getLocale() === "ar"
                ? (arText || enText || "")
                : (enText || arText || "");
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

        $scope.getStatusBadgeClass = function (status) {
            return appointmentStatusService.getStatusBadgeClass(status);
        };

        $scope.getStatusLocalizationKey = function (status) {
            return appointmentStatusService.getStatusLocalizationKey(status);
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
        // Clinical Form Initialization & State Management
        // =========================================================
        $scope.initVisitForm = function (appointment) {
            if (!appointment) {
                $scope.visitForm = null;
                return;
            }

            $scope.visitForm = {
                appointmentId: appointment.Id,
                patientId: appointment.PatientId,
                notes: "",
                symptoms: [],
                diagnoses: [],
                selectedSymptomId: null,
                selectedDiagnosisId: null,
                prescriptionNotes: "",
                prescriptionItems: [
                    {
                        MedicineName: "",
                        Dosage: "",
                        Frequency: "",
                        Duration: "",
                        Notes: ""
                    }
                ]
            };

            $scope.selectedSymptomToAdd = null;
            $scope.selectedDiagnosisToAdd = null;
            $scope.isSaving = false;
            $scope.isVisitSaved = (appointment.Status === "Completed");
            $scope.savedVisitDetails = null;
            $scope.isSubmitted = false;
            $scope.validationErrors = [];
        };

        $scope.clearVisitForm = function () {
            $scope.visitForm = null;
            $scope.selectedSymptomToAdd = null;
            $scope.selectedDiagnosisToAdd = null;
            $scope.isSaving = false;
            $scope.isVisitSaved = false;
            $scope.savedVisitDetails = null;
            $scope.isSubmitted = false;
            $scope.validationErrors = [];
        };

        $scope.resetForm = function () {
            if ($scope.isVisitSaved || $scope.isSaving) return;
            if ($scope.selectedAppointment) {
                $scope.initVisitForm($scope.selectedAppointment);
                notificationService.info($filter("i18n")("visit.ResetForm") || "Form reset.");
            }
        };

        // =========================================================
        // Symptoms & Diagnoses Management
        // =========================================================
        function addClinicalItem(rawItem, masterList, targetList) {
            if (!rawItem || !targetList) return;

            var selected = (rawItem && rawItem.Id) ? rawItem : null;
            if (!selected && masterList) {
                for (var i = 0; i < masterList.length; i++) {
                    if (masterList[i].Id == rawItem) {
                        selected = masterList[i];
                        break;
                    }
                }
            }

            if (!selected) return;

            var alreadyExists = targetList.some(function (item) {
                return item.Id === selected.Id;
            });

            if (alreadyExists) {
                var existsMsg = $filter("i18n")("visit.AlreadySelected") || "This item is already added.";
                notificationService.warning(existsMsg);
            } else {
                targetList.push(selected);
            }
        }

        $scope.onSymptomSelect = function (item) {
            if ($scope.isVisitSaved || $scope.isSaving || !$scope.visitForm) return;
            addClinicalItem(item || $scope.visitForm.selectedSymptomId, $scope.masterSymptoms, $scope.visitForm.symptoms);
            $scope.visitForm.selectedSymptomId = null;
        };

        $scope.removeSymptom = function (index) {
            if ($scope.isVisitSaved || $scope.isSaving || !$scope.visitForm || !$scope.visitForm.symptoms) return;
            $scope.visitForm.symptoms.splice(index, 1);
        };

        $scope.onDiagnosisSelect = function (item) {
            if ($scope.isVisitSaved || $scope.isSaving || !$scope.visitForm) return;
            addClinicalItem(item || $scope.visitForm.selectedDiagnosisId, $scope.masterDiagnoses, $scope.visitForm.diagnoses);
            $scope.visitForm.selectedDiagnosisId = null;
        };

        $scope.removeDiagnosis = function (index) {
            if ($scope.isVisitSaved || $scope.isSaving || !$scope.visitForm || !$scope.visitForm.diagnoses) return;
            $scope.visitForm.diagnoses.splice(index, 1);
        };

        // =========================================================
        // Prescription Items Management
        // =========================================================
        $scope.addPrescriptionItem = function () {
            if ($scope.isVisitSaved || $scope.isSaving || !$scope.visitForm) return;

            if (!$scope.visitForm.prescriptionItems) {
                $scope.visitForm.prescriptionItems = [];
            }

            $scope.visitForm.prescriptionItems.push({
                MedicineName: "",
                Dosage: "",
                Frequency: "",
                Duration: "",
                Notes: ""
            });
        };

        $scope.removePrescriptionItem = function (index) {
            if ($scope.isVisitSaved || $scope.isSaving || !$scope.visitForm || !$scope.visitForm.prescriptionItems) return;
            $scope.visitForm.prescriptionItems.splice(index, 1);
        };

        // =========================================================
        // Form Validation
        // =========================================================
        $scope.validateVisitForm = function () {
            var errors = [];

            if (!$scope.selectedAppointment || !$scope.visitForm || !$scope.visitForm.appointmentId) {
                errors.push($filter("i18n")("visit.ErrorNoAppointmentSelected") || "Please select an appointment before filling the form.");
                return errors;
            }

            if (!$scope.visitForm.symptoms || $scope.visitForm.symptoms.length === 0) {
                errors.push($filter("i18n")("visit.ValidationSymptomsRequired") || "At least one symptom must be selected.");
            }

            if (!$scope.visitForm.diagnoses || $scope.visitForm.diagnoses.length === 0) {
                errors.push($filter("i18n")("visit.ValidationDiagnosesRequired") || "At least one diagnosis must be selected.");
            }

            if (!$scope.visitForm.prescriptionItems || $scope.visitForm.prescriptionItems.length === 0) {
                errors.push($filter("i18n")("visit.ValidationPrescriptionItemsRequired") || "At least one prescription item is required.");
            } else {
                for (var i = 0; i < $scope.visitForm.prescriptionItems.length; i++) {
                    var item = $scope.visitForm.prescriptionItems[i];
                    var num = i + 1;

                    if (!item.MedicineName || !item.MedicineName.trim()) {
                        var medMsg = $filter("i18n")("visit.ValidationMedicineRequired") || "Medicine name is required for item #{0}.";
                        errors.push(medMsg.replace("{0}", num));
                    }

                    if (!item.Dosage || !item.Dosage.trim()) {
                        var dosageMsg = $filter("i18n")("visit.ValidationDosageRequired") || "Dosage is required for item #{0}.";
                        errors.push(dosageMsg.replace("{0}", num));
                    }

                    if (!item.Frequency || !item.Frequency.trim()) {
                        var freqMsg = $filter("i18n")("visit.ValidationFrequencyRequired") || "Frequency is required for item #{0}.";
                        errors.push(freqMsg.replace("{0}", num));
                    }

                    if (!item.Duration || !item.Duration.trim()) {
                        var durMsg = $filter("i18n")("visit.ValidationDurationRequired") || "Duration is required for item #{0}.";
                        errors.push(durMsg.replace("{0}", num));
                    }
                }
            }

            return errors;
        };

        $scope.validateAndProceed = function () {
            $scope.isSubmitted = true;
            $scope.validationErrors = $scope.validateVisitForm();

            if ($scope.validationErrors.length > 0) {
                notificationService.warning($scope.validationErrors[0]);
                return false;
            }

            var successMsg = $filter("i18n")("visit.FormValidReadyForSave") || "Visit details are valid and ready to save.";
            notificationService.success(successMsg);
            return true;
        };

        // =========================================================
        // Save Visit & Complete Appointment Operation
        // =========================================================
        $scope.saveVisit = function () {
            if ($scope.isSaving || $scope.isVisitSaved) return;

            if (!$scope.permissions.Add) {
                notificationService.error($filter("i18n")("common.NoPermission") || "No permission to save visit.");
                return;
            }

            if ($scope.selectedAppointment && $scope.selectedAppointment.Status === "Completed") {
                notificationService.warning($filter("i18n")("visit.AppointmentAlreadyCompleted") || "This appointment has already been completed.");
                return;
            }

            $scope.isSubmitted = true;
            $scope.validationErrors = $scope.validateVisitForm();

            if ($scope.validationErrors.length > 0) {
                notificationService.warning($scope.validationErrors[0]);
                return;
            }

            var payload = {
                AppointmentId: $scope.visitForm.appointmentId,
                PatientId: $scope.visitForm.patientId,
                VisitNotes: $scope.visitForm.notes,
                SymptomIds: ($scope.visitForm.symptoms || []).map(function (s) { return s.Id; }),
                DiagnosisIds: ($scope.visitForm.diagnoses || []).map(function (d) { return d.Id; }),
                PrescriptionNotes: $scope.visitForm.prescriptionNotes,
                PrescriptionItems: ($scope.visitForm.prescriptionItems || []).map(function (item) {
                    return {
                        MedicineName: item.MedicineName,
                        Dosage: item.Dosage,
                        Frequency: item.Frequency,
                        Duration: item.Duration,
                        Notes: item.Notes
                    };
                })
            };

            $scope.isSaving = true;

            $http.post("/Doctor/Visit/SaveVisit", payload)
                .then(function (response) {
                    if (response.data && response.data.IsSuccess) {
                        $scope.isVisitSaved = true;
                        $scope.savedVisitDetails = response.data.Data;

                        // Update selected appointment status to Completed
                        if ($scope.selectedAppointment) {
                            $scope.selectedAppointment.Status = "Completed";
                        }

                        // Update status in appointment list as well
                        if ($scope.appointments && $scope.appointments.length > 0) {
                            for (var i = 0; i < $scope.appointments.length; i++) {
                                if ($scope.appointments[i].Id === payload.AppointmentId) {
                                    $scope.appointments[i].Status = "Completed";
                                    break;
                                }
                            }
                        }

                        var successMsg = (response.data && response.data.Message)
                            || $filter("i18n")("visit.SaveVisitSuccess")
                            || "Visit saved successfully and appointment completed.";
                        notificationService.success(successMsg);
                    } else {
                        var errorMsg = (response.data && response.data.Message)
                            || $filter("i18n")("visit.ErrorSavingVisit")
                            || "Error saving visit.";
                        notificationService.error(errorMsg);
                    }
                })
                .catch(function () {
                    var errorMsg = $filter("i18n")("visit.ErrorSavingVisit")
                        || "Error saving visit.";
                    notificationService.error(errorMsg);
                })
                .finally(function () {
                    $scope.isSaving = false;
                });
        };

        // =========================================================
        // Prescription Report Action
        // =========================================================
        $scope.printPrescriptionReport = function () {
            if (!$scope.isVisitSaved) return;

            var prescriptionId = ($scope.savedVisitDetails && $scope.savedVisitDetails.PrescriptionId)
                ? $scope.savedVisitDetails.PrescriptionId
                : 0;

            if (!prescriptionId || prescriptionId <= 0) {
                var noPrescMsg = $filter("i18n")("visit.NoPrescriptionToPrint")
                    || "No prescription available to print for this visit.";
                notificationService.warning(noPrescMsg);
                return;
            }

            var reportParams = {
                ReportName: "PrescriptionReport.trdx",
                Parms: {
                    PrescriptionId: prescriptionId
                }
            };

            $scope.isPrintingReport = true;

            var printWindow = window.open("", "_blank");

            $http.post("/Report", reportParams)
                .then(function (response) {
                    if (printWindow) {
                        printWindow.document.open();
                        printWindow.document.write(response.data);
                        printWindow.document.close();
                    } else {
                        var popupWindow = window.open();
                        if (popupWindow) {
                            popupWindow.document.open();
                            popupWindow.document.write(response.data);
                            popupWindow.document.close();
                        } else {
                            var blockedMsg = $filter("i18n")("common.PopupBlocked")
                                || "Popup was blocked. Please allow popups for this site.";
                            notificationService.warning(blockedMsg);
                        }
                    }
                })
                .catch(function () {
                    if (printWindow) {
                        try {
                            printWindow.close();
                        } catch (e) { }
                    }
                    var errorMsg = $filter("i18n")("visit.ErrorOpeningReport")
                        || "Error generating prescription report.";
                    notificationService.error(errorMsg);
                })
                .finally(function () {
                    $scope.isPrintingReport = false;
                });
        };

        // =========================================================
        // Appointment Selection (Context Retention)
        // =========================================================
        $scope.selectAppointment = function (appointment) {
            if (!appointment || !appointment.Id) return;

            var isDifferentAppointment = !$scope.selectedAppointment || $scope.selectedAppointment.Id !== appointment.Id;

            $scope.selectedAppointment = appointment;

            if (isDifferentAppointment) {
                currentHistoryRequestId++;
                $scope.historyDetails = null;
                $scope.showHistory = false;
                $scope.isLoadingHistory = false;
                $scope.isHistoryError = false;
                $scope.historyErrorMessage = "";

                $scope.initVisitForm(appointment);
            }

            if (appointment.Status === "Completed") {
                var completedMsg = $filter("i18n")("visit.AppointmentAlreadyCompleted") || "This appointment has already been completed.";
                notificationService.warning(completedMsg);
                $scope.isVisitSaved = true;
            } else {
                var msg = $filter("i18n")("visit.AppointmentSelectedSuccess") || "Appointment selected successfully.";
                notificationService.info(msg);
            }
        };

        $scope.clearSelectedAppointment = function () {
            currentHistoryRequestId++;
            $scope.selectedAppointment = null;
            $scope.historyDetails = null;
            $scope.showHistory = false;
            $scope.isLoadingHistory = false;
            $scope.isHistoryError = false;
            $scope.historyErrorMessage = "";

            $scope.clearVisitForm();
        };

        // =========================================================
        // Patient History Management
        // =========================================================
        $scope.togglePatientHistory = function () {
            if (!$scope.selectedAppointment || !$scope.selectedAppointment.PatientId) return;

            if ($scope.showHistory) {
                $scope.showHistory = false;
                return;
            }

            $scope.showHistory = true;

            if ($scope.historyDetails && $scope.historyDetails.Patient && $scope.historyDetails.Patient.Id === $scope.selectedAppointment.PatientId) {
                return;
            }

            $scope.loadPatientHistory();
        };

        $scope.closePatientHistory = function () {
            $scope.showHistory = false;
        };

        $scope.loadPatientHistory = function () {
            if (!$scope.selectedAppointment || !$scope.selectedAppointment.PatientId) return;

            var patientId = $scope.selectedAppointment.PatientId;
            $scope.isLoadingHistory = true;
            $scope.isHistoryError = false;
            $scope.historyErrorMessage = "";
            $scope.historyDetails = null;

            var requestId = ++currentHistoryRequestId;

            $http.get("/Doctor/Patient/GetPatientHistory", { params: { id: patientId } })
                .then(function (response) {
                    if (requestId !== currentHistoryRequestId || !$scope.selectedAppointment || $scope.selectedAppointment.PatientId !== patientId) {
                        return;
                    }

                    if (response.data && response.data.IsSuccess && response.data.Data) {
                        $scope.historyDetails = response.data.Data;
                    } else {
                        $scope.isHistoryError = true;
                        $scope.historyErrorMessage = (response.data && response.data.Message)
                            || $filter("i18n")("patient.ErrorLoadingHistory")
                            || "Error loading patient history.";
                        notificationService.error($scope.historyErrorMessage);
                    }
                })
                .catch(function () {
                    if (requestId !== currentHistoryRequestId || !$scope.selectedAppointment || $scope.selectedAppointment.PatientId !== patientId) {
                        return;
                    }

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
        // Data Loading: Master Data (Symptoms & Diagnoses)
        // =========================================================
        function loadMasterData() {
            $scope.isLoadingMasterData = true;

            return $http.get("/Doctor/Visit/GetMasterData")
                .then(function (response) {
                    if (response.data && response.data.IsSuccess && response.data.Data) {
                        $scope.masterSymptoms = response.data.Data.Symptoms || [];
                        $scope.masterDiagnoses = response.data.Data.Diagnoses || [];
                    } else {
                        var errorMsg = (response.data && response.data.Message)
                            || $filter("i18n")("visit.ErrorLoadingMasterData")
                            || "Error loading master data.";
                        notificationService.error(errorMsg);
                    }
                })
                .catch(function () {
                    var errorMsg = $filter("i18n")("visit.ErrorLoadingMasterData")
                        || "Error loading master data.";
                    notificationService.error(errorMsg);
                })
                .finally(function () {
                    $scope.isLoadingMasterData = false;
                });
        }

        // =========================================================
        // Data Loading: Today's Eligible Appointments
        // =========================================================
        function loadTodayAppointments() {
            $scope.isLoading = true;
            $scope.isError = false;
            $scope.errorMessage = "";

            return $http.get("/Doctor/Visit/GetTodayAppointments")
                .then(function (response) {
                    if (response.data && response.data.IsSuccess) {
                        $scope.appointments = response.data.Data || [];
                        $scope.filteredAppointments = $scope.appointments;

                        if ($scope.selectedAppointment) {
                            var matchingAppointment = null;
                            for (var i = 0; i < $scope.appointments.length; i++) {
                                if ($scope.appointments[i].Id === $scope.selectedAppointment.Id) {
                                    matchingAppointment = $scope.appointments[i];
                                    break;
                                }
                            }

                            if (matchingAppointment) {
                                $scope.selectedAppointment = matchingAppointment;
                                if ($scope.visitForm) {
                                    $scope.visitForm.appointmentId = matchingAppointment.Id;
                                    $scope.visitForm.patientId = matchingAppointment.PatientId;
                                }
                            }
                        }
                    } else {
                        $scope.isError = true;
                        $scope.errorMessage = (response.data && response.data.Message)
                            || $filter("i18n")("visit.ErrorLoadingAppointments")
                            || "Error loading today's appointments.";
                        notificationService.error($scope.errorMessage);
                    }
                })
                .catch(function () {
                    $scope.isError = true;
                    $scope.errorMessage = $filter("i18n")("visit.ErrorLoadingAppointments")
                        || "Error loading today's appointments.";
                    notificationService.error($scope.errorMessage);
                })
                .finally(function () {
                    $scope.isLoading = false;
                });
        }

        $scope.refresh = function () {
            loadTodayAppointments();
        };

        // =========================================================
        // Initialization & Permissions
        // =========================================================
        $scope.init = function () {
            permissionService.loadPermissions()
                .then(function () {
                    $scope.permissions.Show = permissionService.hasPermission(ScreenKeys.Visit, "Show");
                    $scope.permissions.Add = permissionService.hasPermission(ScreenKeys.Visit, "Add");

                    if ($scope.permissions.Show) {
                        loadTodayAppointments();
                        loadMasterData();
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

    DoctorVisitController.$inject = [
        "$scope",
        "$http",
        "notificationService",
        "$filter",
        "locale",
        "permissionService",
        "ScreenKeys",
        "appointmentStatusService"
    ];

    angular.module("clinicApp")
        .controller("DoctorVisitController", DoctorVisitController)
        .controller("VisitController", DoctorVisitController);
})();
