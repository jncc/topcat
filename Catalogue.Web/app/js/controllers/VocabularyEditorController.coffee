angular.module('app.controllers').controller 'VocabularyEditorController',

    ($scope, $http, $routeParams, $location, vocab) -> 
        console.log("weee")
        
        $scope.reset = -> 
            $scope.form = angular.copy(vocab)
        
        # initially set up form
        $scope.reset()