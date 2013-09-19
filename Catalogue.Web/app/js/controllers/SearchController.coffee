

angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http) ->

        # note: $location.search is the angular api for the querystring value
         
        doSearch = (query) ->
            $location.search('q', query.q) # update the url
            if query.q
                $rootScope.busy = { value: true }
                # search the server
                $http.get('../api/search?q=' + query.q).success (result) ->
                    $rootScope.busy = { value: false }
                    # don't overwrite with old slow results!
                    if angular.equals result.query, $scope.query
                        $scope.result = result
            else
                $scope.result = {}

        $scope.query = { q: '', p: 1 }

        # when the model query value is updated, do the search
        $scope.$watch 'query', doSearch, true

        # when the url query value is updated, update the model query value
        $scope.$watch(
            ()  -> $location.search()['q'] #todo watch and update whole querystring
            (q) -> $scope.query.q = q || ''
        )

