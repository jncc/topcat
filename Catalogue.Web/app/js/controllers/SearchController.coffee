

angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http, $timeout) ->

        # slightly hacky way of triggering animations on startup
        # to work around angular skipping the initial animation
        $scope.app = { starting: true };
        $timeout (-> $scope.app.starting = false), 500

        # note: $location.search is the angular api for the querystring value
        
        doSearch = (query) ->
            $location.search('q', query.q) # update the url
            $rootScope.page = { title: if query.q then ' - ' + query.q else '' } # update the page title
            if query.q
                $rootScope.busy = { value: true }
                # search the server
                $http.get('../api/search?' + $.param query)
                    .success (result) ->
                        $rootScope.busy = { value: false }
                        # don't overwrite with old slow results!
                        if angular.equals result.query, $scope.query
                            $scope.result = result
                    .error -> $rootScope.busy = { value: false }
            else
                $scope.result = {}

        # initial values
        $scope.query = { q: $location.search()['q'] || '', p: 1 }

        # when the model query value is updated, do the search
        $scope.$watch 'query', doSearch, true

        # when the querystring changes, update the model query value
        $scope.$watch(
            ()  -> $location.search()['q'] #todo watch and update whole querystring
            (q) -> $scope.query.q = q || ''
        )

