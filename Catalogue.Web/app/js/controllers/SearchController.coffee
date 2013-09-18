

angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http) ->

        search = (q) ->
            $location.search('q', q) # update the url
            if q
                $rootScope.busy = { value: true }
                $http.get('../api/search?q=' + q).success (results) ->
                    $rootScope.busy = { value: false }
                    if results.query == $scope.q.value # don't overwrite faster queries!
                        $scope.results = results
            else
                $scope.results = {}

        $scope.q = { value: '' }

        # when the model query value is updated, do the search
        $scope.$watch 'q.value', search 

        # when the url query value is updated, update the model query value
        $scope.$watch(
            ()  -> $location.search()['q']
            (q) -> $scope.q.value = q || ''
        )

