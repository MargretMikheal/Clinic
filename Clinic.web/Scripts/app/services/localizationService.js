(function () {
    "use strict";

    var app;
    try {
        app = angular.module("clinicApp");
    } catch (e) {
        app = angular.module("clinicApp", []);
    }

    app.factory("localizationService", ["$http", "$q", "$rootScope", function ($http, $q, $rootScope) {
        var bundleCache = {};
        var pendingRequests = {};
        var fallbackLocale = "en";

        function getCookie(name) {
            var match = document.cookie.match(new RegExp("(^|;)\\s*" + name + "\\s*=\\s*([^;]+)"));
            return match ? decodeURIComponent(match.pop()) : null;
        }

        function setCookie(name, value, days) {
            var expires = "";
            if (days) {
                var date = new Date();
                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                expires = "; expires=" + date.toUTCString();
            }
            document.cookie = name + "=" + encodeURIComponent(value || "") + expires + "; path=/";
        }

        var currentLocale = getCookie("clinic_lang") || "ar";
        if (currentLocale !== "en" && currentLocale !== "ar") {
            currentLocale = "ar";
        }

        function parseKey(fullKey) {
            if (!fullKey || typeof fullKey !== "string") {
                return { bundle: "", key: "" };
            }

            var dotIndex = fullKey.indexOf(".");
            if (dotIndex === -1) {
                return { bundle: "common", key: fullKey };
            }

            return {
                bundle: fullKey.substring(0, dotIndex),
                key: fullKey.substring(dotIndex + 1)
            };
        }

        function getCacheKey(locale, bundle) {
            return (locale || currentLocale).toLowerCase() + ":" + (bundle || "common").toLowerCase();
        }

        function loadBundle(bundleName, locale) {
            var targetLocale = (locale || currentLocale).toLowerCase();
            var bundle = (bundleName || "common").toLowerCase();
            var cacheKey = getCacheKey(targetLocale, bundle);

            if (bundleCache[cacheKey]) {
                return $q.resolve(bundleCache[cacheKey]);
            }

            if (pendingRequests[cacheKey]) {
                return pendingRequests[cacheKey];
            }

            var url = "/Localization/" + targetLocale + "/" + bundle + ".lang.json";

            var requestPromise = $http.get(url, { cache: false })
                .then(function (response) {
                    var data = response.data || {};
                    bundleCache[cacheKey] = data;
                    delete pendingRequests[cacheKey];
                    return data;
                })
                .catch(function (error) {
                    delete pendingRequests[cacheKey];

                    if (targetLocale !== fallbackLocale) {
                        var fallbackCacheKey = getCacheKey(fallbackLocale, bundle);
                        if (bundleCache[fallbackCacheKey]) {
                            return bundleCache[fallbackCacheKey];
                        }
                        return loadBundle(bundle, fallbackLocale);
                    }

                    bundleCache[cacheKey] = {};
                    return {};
                });

            pendingRequests[cacheKey] = requestPromise;
            return requestPromise;
        }

        return {
            getLocale: function () {
                return currentLocale;
            },

            setLocale: function (lang) {
                if (!lang || (lang !== "en" && lang !== "ar")) {
                    return;
                }
                currentLocale = lang;
                setCookie("clinic_lang", lang, 365);
                $rootScope.$broadcast("$localeChanged", { locale: lang });
            },

            ready: function (bundleName, locale) {
                return loadBundle(bundleName, locale);
            },

            isLoaded: function (bundleName, locale) {
                var cacheKey = getCacheKey(locale || currentLocale, bundleName);
                return !!bundleCache[cacheKey];
            },

            instant: function (fullKey) {
                if (!fullKey) {
                    return "";
                }

                var parsed = parseKey(fullKey);
                if (!parsed.bundle || !parsed.key) {
                    return fullKey;
                }

                var cacheKey = getCacheKey(currentLocale, parsed.bundle);
                var bundleData = bundleCache[cacheKey];

                if (bundleData) {
                    if (bundleData[parsed.key] !== undefined && bundleData[parsed.key] !== null) {
                        return bundleData[parsed.key];
                    }
                    // Check fallback locale if different
                    if (currentLocale !== fallbackLocale) {
                        var fallbackKey = getCacheKey(fallbackLocale, parsed.bundle);
                        var fallbackData = bundleCache[fallbackKey];
                        if (fallbackData && fallbackData[parsed.key] !== undefined && fallbackData[parsed.key] !== null) {
                            return fallbackData[parsed.key];
                        }
                    }
                    return fullKey;
                }

                // Trigger lazy loading
                loadBundle(parsed.bundle, currentLocale);
                return fullKey;
            },

            getString: function (fullKey) {
                if (!fullKey) {
                    return $q.resolve("");
                }

                var parsed = parseKey(fullKey);
                if (!parsed.bundle || !parsed.key) {
                    return $q.resolve(fullKey);
                }

                return loadBundle(parsed.bundle, currentLocale).then(function (bundleData) {
                    if (bundleData && bundleData[parsed.key] !== undefined && bundleData[parsed.key] !== null) {
                        return bundleData[parsed.key];
                    }
                    return fullKey;
                });
            }
        };
    }]);
})();
