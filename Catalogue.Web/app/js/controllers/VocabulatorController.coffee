angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http) -> 
        
        $scope.vocabs = {}
        $scope.selected = {}
        $scope.find = {}

        $http.get('../api/vocabularylist').success (result) ->
            $scope.vocabs.all = result
            $scope.vocabs.filtered = result
        
        $scope.doFind = (text) ->
            #$scope.selected.vocab = null # ?
            $scope.vocabs.filtered = v for v in $scope.vocabs.all when v.name.search(text) isnt -1
            
        # when the model search value is updated, do the search
        $scope.$watch 'find.text', $scope.doFind, true
                
        
        $scope.selected.keyword =
            vocab: 'http://vocab.jncc.gov.uk/original-seabed-classification-system'
            value: 'MNCR'
            
        $scope.close = ->
            $scope.$close($scope.selected.keyword)
        