angular.module('app.controllers').controller 'VocabulariesController',
    ($scope, $http) -> 
        $http.get('../api/vocabularies?q=all').success (data)-> 
            $scope.vocabularies = data          
        
