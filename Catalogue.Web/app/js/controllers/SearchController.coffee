

angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http) ->

        search = (q) ->
            if q
                $location.search('q', q) # update the url
                $rootScope.busy = { value: true }
                $http.get('../api/search?q=' + q).success (results) -> 
                    $scope.results = results
                    $rootScope.busy = { value: false }
            else
                $scope.results = {}

        $scope.q = { value: '' }

        # when the model query value is updated, do the search
        $scope.$watch 'q.value', search 

        # when the url query value is updated, update the model query value
        $scope.$watch(
            ()  -> $location.search()['q']
            (q) -> $scope.query.q = q || ''
        )

