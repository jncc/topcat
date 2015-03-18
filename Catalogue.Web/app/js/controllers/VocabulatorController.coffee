angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http) -> 
        
        $scope.vocabs = {}
        $scope.selected = {}
        $scope.find = {}

        $http.get('../api/vocabularylist').success (result) ->
            $scope.vocabs.all = result
            $scope.vocabs.filtered = result
        
        $scope.doFind = (text) ->
            if $scope.vocabs.all
                #console.log text.toLowerCase()
                q = (v for v in $scope.vocabs.all when v.name.toLowerCase().indexOf(text.toLowerCase()) isnt -1) #when v.name.search(text) > 0
                console.log q
                $scope.vocabs.filtered = q
            
        # when the model search value is updated, do the search
        $scope.$watch 'find.text', $scope.doFind, true
                
        
        $scope.selected.keyword =
            vocab: 'http://vocab.jncc.gov.uk/original-seabed-classification-system'
            value: 'MNCR'
            
        $scope.close = ->
            $scope.$close($scope.selected.keyword)
        