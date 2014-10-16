angular.module('app.controllers').controller 'VocabularyEditorController',

    ($scope, $http, $routeParams, $location, vocab) -> 
        $scope.reset = -> 
            $scope.form = angular.copy(vocab)
            if (vocab.id)
                $scope.newVocab = false
            else
                $scope.newVocab = true
        
        $scope.removeKeyword = (index) ->
            $scope.form.values.splice index, 1
            
        $scope.addKeyword = ->
            $scope.form.values.push('')
            
        $scope.isClean = -> angular.equals($scope.form, vocab)
        $scope.isSaveHidden = -> $scope.isClean()
        $scope.isCancelHidden = -> $scope.isClean()
        
        # initially set up form
        $scope.reset()
        
        