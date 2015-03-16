angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http) -> 
    
        $scope.keyword =
            vocab: 'http://vocab.jncc.gov.uk/pete'
            value: 'ubertime'
            
        $scope.close = ->
            $scope.$close($scope.keyword)
        
 