angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http) -> 
    
        $scope.keyword =
            vocab: 'http://vocab.jncc.gov.uk/original-seabed-classification-system'
            value: 'MNCR'
            
        $scope.close = ->
            $scope.$close($scope.keyword)
        