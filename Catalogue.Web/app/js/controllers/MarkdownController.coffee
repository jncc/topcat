angular.module('app.controllers').controller 'MarkdownController',

    ($scope, markdown) -> 

        $scope.markdown = markdown

        $scope.getHtml = (s) ->
            showdown = new Showdown.converter()
            showdown.makeHtml s
        
        $scope.close = -> $scope.$close($scope.markdown)