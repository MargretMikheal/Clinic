(function () {

    "use strict";

    // =========================================================
    // Clinic AngularJS Application
    // =========================================================

    var app;
    try {
        app = angular.module("clinicApp");
    } catch (e) {
        app = angular.module("clinicApp", []);
    }


    // =========================================================
    // HTTP Configuration
    // =========================================================

    app.config([
        "$httpProvider",
        function ($httpProvider) {

            // Convert API date strings to JavaScript Date objects
            $httpProvider.defaults.transformResponse.push(
                function (responseData) {

                    if (responseData) {
                        convertDateStringsToDates(responseData);
                    }

                    return responseData;
                }
            );

            $httpProvider.interceptors.push(function () {
                return {
                    request: function (config) {
                        var match = document.cookie.match(/(^|;)\s*clinic_lang\s*=\s*([^;]+)/);
                        var lang = match ? match.pop() : "ar";
                        config.headers["Accept-Language"] = lang;
                        return config;
                    }
                };
            });

        }
    ]);


    // =========================================================
    // Regular Expression Input Directive
    // =========================================================

    app.directive("regExInput", function () {

        return {
            restrict: "A",

            link: function (scope, element, attrs) {

                element.on("keypress", function (event) {

                    var regex = new RegExp(attrs.regEx);

                    var key = String.fromCharCode(
                        !event.charCode
                            ? event.which
                            : event.charCode
                    );

                    if (!regex.test(key)) {

                        event.preventDefault();

                        return false;
                    }
                });
            }
        };
    });


    // =========================================================
    // Notification Service
    // =========================================================

    app.factory("notificationService", ["localizationService", function (localizationService) {

        return {

            success: function (message, title) {

                toastr.success(
                    message,
                    title || localizationService.instant("common.Success")
                );
            },

            error: function (message, title) {
                toastr.error(
                    message,
                    title || localizationService.instant("common.Error")
                );
            },

            warning: function (message, title) {
                toastr.warning(
                    message,
                    title || localizationService.instant("common.Warning")
                );
            },

            info: function (message, title) {
                toastr.info(
                    message,
                    title || localizationService.instant("common.Information")
                );
            }
        };
    }]);


    // =========================================================
    // Toastr Configuration
    // =========================================================

    toastr.options = {

        closeButton: true,

        progressBar: true,

        positionClass: "toast-top-right",

        preventDuplicates: true,

        timeOut: 4000,

        extendedTimeOut: 1000,

        showDuration: 300,

        hideDuration: 1000,

        showEasing: "swing",

        hideEasing: "linear",

        showMethod: "fadeIn",

        hideMethod: "fadeOut"
    };


    // =========================================================
    // Date Conversion
    // =========================================================

    var regexIso8601 =
        /^(\d{4})-(\d{2})-(\d{2})[T\s](\d{2}):(\d{2}):(\d{2})(\.(\d{1,3}))?$/;


    function convertDateStringsToDates(input) {

        // Ignore null / undefined values
        if (input === null || input === undefined) {
            return input;
        }


        // Ignore values that are not objects
        if (typeof input !== "object") {
            return input;
        }


        // Loop through object properties
        for (var key in input) {

            if (!input.hasOwnProperty(key)) {
                continue;
            }


            var value = input[key];

            var match;


            // -------------------------------------------------
            // String that looks like an ISO date
            // -------------------------------------------------

            if (
                typeof value === "string" &&
                (match = value.match(regexIso8601))
            ) {

                var milliseconds = Date.parse(match[0]);

                if (!isNaN(milliseconds)) {

                    input[key] = new Date(milliseconds);
                }
            }


            // -------------------------------------------------
            // Nested object / array
            // -------------------------------------------------

            else if (
                typeof value === "object" &&
                value !== null
            ) {

                convertDateStringsToDates(value);
            }
        }


        return input;
    }


})();