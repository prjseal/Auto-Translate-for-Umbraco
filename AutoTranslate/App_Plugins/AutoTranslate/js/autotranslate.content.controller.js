(function () {
    'use strict';

    function AutoTranslateContent($scope, $http, $routeParams, $window, navigationService, contentResource, contentEditingHelper) {
        var apiUrl;
        var vm = this;
        vm.includeDescendants = false;
        vm.overwriteExisting = false;
        vm.properties = [];
        vm.chosenEditors = [];

        function init() {
            apiUrl = Umbraco.Sys.ServerVariables["AutoTranslate"]["ApiUrl"];
            getProperties();
        }

        function getProperties() {
            $http({
                method: 'GET',
                url: apiUrl + 'GetEditorAliasesFromContentItem/?contentId=' + $scope.currentNode.id,
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function (response) {
                vm.properties = response.data;
            }); 
        }

        $scope.submitTranslateContent = function () {
            var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
            vm.loading = true;
            $http({
                method: 'POST',
                url: apiUrl + 'SubmitTranslateContent/',
                data: JSON.stringify({
                    CurrentCulture: culture,
                    NodeId: $scope.currentNode.id,
                    OverwriteExistingValues: vm.overwriteExisting,
                    IncludeDescendants: vm.includeDescendants,
                    AllowedPropertyEditors: vm.chosenEditors.join()
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
            // If the recurvise toggle is clicked
            if (type === "recursive") {
                if (vm.includeDescendants) {
                    vm.includeDescendants = false;
                    return;
                }
                vm.includeDescendants = true;
            } else if (type === "overwrite") {
                if (vm.overwriteExisting) {
                    vm.overwriteExisting = false;
                    return;
                }
                vm.overwriteExisting = true;
            } else {
                var index = vm.chosenEditors.indexOf(type);
                if (index > -1) {
                    vm.chosenEditors.splice(index, 1);
                } else {
                    vm.chosenEditors.push(type);
                }
                console.log(vm.chosenEditors);
            }
        }

        init();
    }

    angular.module('umbraco').controller('AutoTranslateContent', AutoTranslateContent);

})();