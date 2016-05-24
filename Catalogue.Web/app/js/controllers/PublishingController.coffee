angular.module('app.controllers').controller 'PublishingController',

    ($scope, $http) ->
        
        
        m =
            openData: {},
            tab: 2
        
        $scope.m = m

        $http.get('../api/publishing/opendata/summary').success (result) -> m.openData.summary = result
        $http.get('../api/publishing/opendata/pending').success (result) -> m.openData.pending = result

        