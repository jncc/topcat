angular.module('app.controllers').controller 'VocabularyListController',
    ($scope, $http) -> 
        $http.get('../api/VocabularyList').success (data)-> 
            $scope.vocabularies = data          
        
        $scope.encodeId = (id) ->
            encodeURIComponent(id)
