(function () {
    'use strict';

    function AutoTranslate($scope, $http, editorState, $routeParams, $window) {
        var apiUrl;

        function init() {
            var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
            apiUrl = Umbraco.Sys.ServerVariables["AutoTranslate"]["TextTranslateApiUrl"];
        }

        $scope.getTranslatedText = function () {
            var culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;

            $http({
                method: 'POST',
                url: apiUrl + 'GetTranslatedText/',
                data: JSON.stringify({
                    CurrentCulture: culture,
                    NodeId: editorState.current.id,
                    OverwriteExistingValues: true,
                    IncludeDescendants: false
                }),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function (response) {
                $window.location.reload(true);
            });
        };

        init();
    }

    angular.module('umbraco').controller('AutoTranslate', AutoTranslate);

})();