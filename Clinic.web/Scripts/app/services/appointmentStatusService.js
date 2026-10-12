(function () {
    "use strict";

    var app = angular.module("clinicApp");

    app.factory("appointmentStatusService", [function () {

        function getStatusBadgeClass(status) {
            switch (status) {
                case "Booked": return "c-badge c-badge-booked";
                case "Confirmed": return "c-badge c-badge-confirmed";
                case "Completed": return "c-badge c-badge-completed";
                case "Cancelled": return "c-badge c-badge-cancelled";
                case "NoShow": return "c-badge c-badge-noshow";
                default: return "c-badge c-badge-neutral";
            }
        }

        function getStatusLocalizationKey(status) {
            switch (status) {
                case "Booked": return "appointment.Booked";
                case "Confirmed": return "appointment.Confirmed";
                case "Completed": return "appointment.Completed";
                case "Cancelled": return "appointment.Cancelled";
                case "NoShow": return "appointment.NoShow";
                default: return "";
            }
        }

        return {
            getStatusBadgeClass: getStatusBadgeClass,
            getStatusLocalizationKey: getStatusLocalizationKey
        };
    }]);

})();
