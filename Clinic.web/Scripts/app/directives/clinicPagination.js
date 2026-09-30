(function () {
    "use strict";

    angular.module("clinicApp")
        .directive("clinicPagination", function () {
            return {
                restrict: "E",
                scope: {
                    pagination: "="
                },
                template: 
                    '<div style="display: flex; justify-content: space-between; align-items: center; margin-top: 16px;">' +
                        '<div>' +
                            '<span style="color: var(--c-text-muted); font-size: 14px;">' +
                                '{{ \'common.Page\' | i18n }} {{ pagination.page }} / {{ getTotalPages() }}' +
                            '</span>' +
                        '</div>' +
                        '<div style="display: flex; gap: 8px;">' +
                            '<md-button class="md-icon-button" ng-click="prevPage()" ng-disabled="pagination.page === 1" aria-label="Previous">' +
                                '<i class="fa fa-angle-left"></i>' +
                            '</md-button>' +
                            '<md-button class="md-icon-button" ng-click="nextPage()" ng-disabled="pagination.page === getTotalPages()" aria-label="Next">' +
                                '<i class="fa fa-angle-right"></i>' +
                            '</md-button>' +
                        '</div>' +
                    '</div>',
                link: function (scope) {
                    scope.getTotalPages = function () {
                        if (!scope.pagination || !scope.pagination.totalItems) return 1;
                        return Math.ceil(scope.pagination.totalItems / scope.pagination.pageSize) || 1;
                    };

                    scope.prevPage = function () {
                        if (scope.pagination.page > 1) {
                            scope.pagination.page--;
                        }
                    };

                    scope.nextPage = function () {
                        if (scope.pagination.page < scope.getTotalPages()) {
                            scope.pagination.page++;
                        }
                    };
                }
            };
        });
})();
