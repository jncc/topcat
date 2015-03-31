angular.module('app.controllers').controller 'MarkdownController',

    ($scope, foo) -> 

        console.log foo
        $scope.md = { text: foo }

        $scope.markdown = (s) ->
        #    showdown = new Showdown.converter()
        #    showdown.makeHtml s
        
        $scope.close = -> $scope.$close($scope.md.text)