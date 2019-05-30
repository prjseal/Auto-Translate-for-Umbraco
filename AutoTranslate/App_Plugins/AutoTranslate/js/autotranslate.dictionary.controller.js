(function () {
    'use strict';

    function AutoTranslateDictionary($scope, $http, editorState, $routeParams, $window, navigationService) {
        var apiUrl;
        var vm = this;

        function init() {
            var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
            apiUrl = Umbraco.Sys.ServerVariables["AutoTranslate"]["TextTranslateApiUrl"];
        }

        $scope.getTranslatedText = function () {
            var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
            vm.loading = true;
            $http({
                method: 'POST',
                url: apiUrl + 'TranslateDictionaryItems/',
                data: JSON.stringify({
                    CurrentCulture: culture,
                    NodeId: editorState.current.id,
                    OverwriteExistingValues: true,
                    IncludeDescendants: true,
                    FallbackToKey: true
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

        init();
    }

    angular.module('umbraco').controller('AutoTranslateDictionary', AutoTranslateDictionary);

})();