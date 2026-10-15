(function () {
    "use strict";

    var app = angular.module("clinicApp");

    app.constant("ScreenKeys", {
        Administration: 1,
        Reception: 2,
        Doctor: 3,
        Users: 4,
        RolePermissions: 5,
        Specialities: 6,
        Symptoms: 7,
        Diagnoses: 8,
        Patients: 9,
        Appointments: 10,
        MySchedule: 11,
        MyAppointments: 12,
        PatientHistory: 13,
        Visit: 14,
        DoctorAppointmentsReport: 15,
        PatientMedicalHistoryReport: 16,
        PrescriptionReport: 17,
        Reports: 18,
        DoctorPerformanceReport: 19
    });

    app.factory("permissionService", ["$http", "$q", function ($http, $q) {
        
        var permissionsCache = null;
        var fetchPromise = null;

        function loadPermissions() {
            if (permissionsCache) {
                return $q.when(permissionsCache);
            }
            if (fetchPromise) {
                return fetchPromise;
            }

            fetchPromise = $http.get("/api/account/permissions")
                .then(function (response) {
                    if (response.data && response.data.IsSuccess) {
                        permissionsCache = response.data.Data;
                        return permissionsCache;
                    }
                    return $q.reject("Failed to load permissions");
                })
                .finally(function () {
                    fetchPromise = null;
                });
            
            return fetchPromise;
        }

        function hasPermission(screenKey, permissionType) {
            if (!permissionsCache) return false;
            var screenPerms = permissionsCache[screenKey];
            if (!screenPerms) return false;
            return !!screenPerms[permissionType];
        }

        function clearCache() {
            permissionsCache = null;
        }

        return {
            loadPermissions: loadPermissions,
            hasPermission: hasPermission,
            clearCache: clearCache
        };
    }]);

})();
