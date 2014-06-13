

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
                $scope.busy.start()
                # search the server
                $http.get('../api/search?' + $.param query)
                    .success (result) ->
                        # don't overwrite with old slow results!
                        if angular.equals result.query, $scope.query
                            $scope.result = result
                    .finally -> $scope.busy.stop()
            else
                $scope.result = {}
                
         $scope.doKeywordSearch = (keyword) ->
            $scope.busy.start()
            # $http.get('../api/keywordSearch?value=' + keyword.value+'&vocab='+keyword.value)
            $http.post('../api/keywordSearch', keyword).success (result) ->
                    $scope.result = result; 
                    $scope.busy.stop();
                   # $location.url($location.path());
                   # $location.search('keyword', keyword); # update the url
                    $rootScope.page = { title:keyword}; # update the page title#.finally -> 
                    # don't overwrite with old slow results!
                    #if angular.equals result.query, $scope.query
                     #   $scope.result = result.finally -> 
                      #      $scope.busy.stop()
            
        # initial values
        $scope.query = { q: $location.search()['q'] || '', p: 1 }

        # when the model query value is updated, do the search
        $scope.$watch 'query', doSearch, true

        # when the querystring changes, update the model query value
        $scope.$watch(
            ()  -> $location.search()['q'] #todo watch and update whole querystring
            (q) -> $scope.query.q = q || ''
        )

