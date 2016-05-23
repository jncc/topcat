angular.module('app.controllers').controller 'PublishingController',

    ($scope, $http) ->
        
        $scope.opendata = {}
        $http.get('../api/publishing/opendata/summary').success (result) -> $scope.opendata.summary = result
        $http.get('../api/publishing/opendata/pending').success (result) -> $scope.opendata.pending = result

        
