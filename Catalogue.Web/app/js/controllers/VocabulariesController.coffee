angular.module('app.controllers').controller 'VocabulariesController',
    ($scope, $http) -> 
        $http.get('../api/vocabulariestypeahead?q=all').success (data)-> 
            $scope.vocabularies = data          
        
