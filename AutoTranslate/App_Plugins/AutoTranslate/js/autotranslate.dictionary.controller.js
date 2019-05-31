(function () {
    'use strict';

    function AutoTranslateDictionary($scope, $http, editorState, $routeParams, $window, navigationService) {
        var apiUrl;
        var vm = this;
        vm.includeDescendants = true;
        vm.overwriteExisting = false;
        vm.fallbackToKey = true;

        function init() {
            apiUrl = Umbraco.Sys.ServerVariables["AutoTranslate"]["ApiUrl"];
        }

        $scope.submitTranslateDictionary = function () {
            var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
            vm.loading = true;
            $http({
                method: 'POST',
                url: apiUrl + 'SubmitTranslateDictionary/',
                data: JSON.stringify({
                    CurrentCulture: culture,
                    NodeId: editorState.current.id,
                    OverwriteExistingValues: vm.overwriteExisting,
                    IncludeDescendants: vm.includeDescendants,
                    FallbackToKey: vm.fallbackToKey
                }),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function (response) {
                $window.location.reload(true);
            });
        };

        $scope.closeDialog = function () {
            navigationService.hideDialog();
        };

        $scope.toggle = toggleHandler;

        function toggleHandler(type) {
            if (type === "recursive") {
                if (vm.includeDescendants) {
                    vm.includeDescendants = false;
                    return;
                }
                vm.includeDescendants = true;
            }

            if (type === "overwrite") {
                if (vm.overwriteExisting) {
                    vm.overwriteExisting = false;
                    return;
                }
                vm.overwriteExisting = true;
            }

            if (type === "fallbackToKey") {
                if (vm.fallbackToKey) {
                    vm.fallbackToKey = false;
                    return;
                }
                vm.fallbackToKey = true;
            }
        }

        init();
    }

    angular.module('umbraco').controller('AutoTranslateDictionary', AutoTranslateDictionary);

})();