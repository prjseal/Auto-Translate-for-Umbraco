(function () {
    'use strict';

    function AutoTranslateDictionary($scope, $http, $routeParams, $window, navigationService) {
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
                    NodeId: $scope.currentNode.id,
                    OverwriteExistingValues: vm.overwriteExisting,
                    IncludeDescendants: vm.includeDescendants,
                    FallbackToKey: vm.fallbackToKey
                }),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function (response) {
                $window.location.reload(true);
            }).catch((err) => {
                vm.loading = false;
                vm.error = "Unable to translate text. Please check your Azure API details are correct in the web.config file.";
            });
        };

        $scope.closeDialog = function () {
            navigationService.hideDialog();
        };

        $scope.toggle = toggleHandler;

        function toggleHandler(type) {
            if (type === "recursive") {
                vm.includeDescendants = !vm.includeDescendants;
            } else if (type === "overwrite") {
                vm.overwriteExisting = !vm.overwriteExisting;
            } else if (type === "fallbackToKey") {
                vm.fallbackToKey = !vm.fallbackToKey;
            }
        }

        init();
    }

    angular.module('umbraco').controller('AutoTranslateDictionary', AutoTranslateDictionary);

})();