(function () {
    'use strict';

    function AutoTranslateContent($scope, $http, $routeParams, $window, navigationService, contentResource, contentEditingHelper) {
        var apiUrl;
        var vm = this;
        vm.includeDescendants = false;
        vm.overwriteExisting = false;
        vm.properties = [];
        vm.chosenEditors = [];
        vm.error = '';

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