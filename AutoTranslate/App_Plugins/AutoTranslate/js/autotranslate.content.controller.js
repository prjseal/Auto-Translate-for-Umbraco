(function () {
    'use strict';

    function AutoTranslateContent($scope, $http, editorState, $routeParams, $window, navigationService) {
        var apiUrl;
        var vm = this;
        vm.includeDescendants = false;
        vm.overwriteExisting = false;

        function init() {
            var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
            apiUrl = Umbraco.Sys.ServerVariables["AutoTranslate"]["TextTranslateApiUrl"];
        }

        $scope.getTranslatedText = function () {
            var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
            vm.loading = true;
            $http({
                method: 'POST',
                url: apiUrl + 'GetTranslatedText/',
                data: JSON.stringify({
                    CurrentCulture: culture,
                    NodeId: editorState.current.id,
                    OverwriteExistingValues: vm.overwriteExisting,
                    IncludeDescendants: vm.includeDescendants
                }),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function (response) {
                vm.loading = false;
                $window.location.reload(true);
            });
        };

        $scope.closeDialog = function () {
            navigationService.hideDialog();
        };

        $scope.toggle = toggleHandler;

        function toggleHandler(type) {
            // If the recurvise toggle is clicked
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
        }

        init();
    }

    angular.module('umbraco').controller('AutoTranslateContent', AutoTranslateContent);

})();