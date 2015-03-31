angular.module('app.controllers').controller 'MarkdownController',

    ($scope) -> 

        $scope.text = { value: '' }

        $scope.markdown = (s) ->
            showdown = new Showdown.converter()
            showdown.makeHtml s

