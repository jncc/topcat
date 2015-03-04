angular.module('app.controllers').controller 'SearchController',
    
    ($scope, $rootScope, $location, $http, $timeout) ->
        
        # slightly hacky way of triggering animations on startup to work around
        # angular skipping the initial animation - set app.starting to true for 500ms
        $scope.app = { starting: true };
        $timeout (-> $scope.app.starting = false), 500
        
        # default results view style
        $scope.resultsView = 'list'

        # note: $location.search is the angular api for the querystring value

        updateUrl = (query) -> 
            # update the url querystring to match the query object
            $location.search 'q', query.q || null
            $location.search 'k', query.k || null
            #$location.search('p', $scope.query.p) 
            #$location.search('n', $scope.query.n)
        
        queryRecords = (query) ->
            $scope.busy.start()
            $http.get('../api/search?' + $.param query)
                .success (result) ->
                    console.log query
                    console.log result.query
                    # don't overwrite with earlier but slower queries!
                    if angular.equals result.query, query
                        #if result.total is 0 # without this the browser crashes due to an unsafe object check by angular, when results are zero
                            #$scope.result = {}
                        $scope.result = result
                .error (e) -> $scope.notifications.add 'Oops! ' + e.message
                .finally   -> $scope.busy.stop()
        
        queryKeywords = (query) ->
            if query.q
                $scope.busy.start()
                $http.get('../api/keywords?q=' + query.q)
                    .success (result) ->
                        $scope.keywordSuggestions = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
                    .finally   -> $scope.busy.stop()
             else
                $scope.keywordSuggestions = {}
        
        # called whenever the $scope.query object changes
        # also called explicitly from search button
        $scope.doSearch = (query) ->
            updateUrl query            
            queryKeywords query
            queryRecords query
                
        # when the model query value is updated, do the search
        $scope.$watch 'query', $scope.doSearch, true

        newQuery = ->
            q: '',
            k: null,
            p: 0, 
            n: 25

        # initialise the query to whatever is in the querystring
        $scope.query = $.extend {}, newQuery(), $location.search() 
        
        # when the querystring changes, update the model query value
        # todo this doesn't work very well; let's see if we can do without it
        #$scope.$watch(
        #    ()  -> $location.search()
        #    (x) ->
        #        #console.log $.extend {}, newQuery(), x
        #        #$scope.query = $.extend {}, newQuery(), x
        #)
        
        $scope.queryByKeyword = (keyword) ->
            $scope.query = $.extend {}, newQuery(), { 'k': getPathFromKeyword(keyword) }
            
        $scope.removeKeywordFromQuery = (keyword) ->
            $scope.query.k = null
        
        # function to get the current querystring in the view (for constructing export url)
        $scope.querystring = -> $.param $scope.query

        # keyword helper functions            
        getPathFromKeyword = (keyword) ->
            path = ensureEndsWith(keyword.vocab, '/') + keyword.value
            path.replace("http://", "")
        getKeywordFromPath = (path) -> 
            if path.indexOf('/') == -1
                return {value: path, vocab: ''}
            else
                elements = path.split('/')
                value = elements[elements.length - 1]
                vocab = "http://"
                for i in [0..elements.length - 2] by 1
                    vocab = vocab.concat(elements[i].concat('/'))
                return {value: value, vocab: vocab}
        ensureEndsWith = (str, suffix) ->
            if str != '' && !(str.indexOf(suffix, str.length - suffix.length) != -1)  
                return str.concat(suffix)
            else
                return str
    
       

        # paging helper functions                
        $scope.setPage = (n) ->
            $scope.query.p = n-1
        $scope.range  = (min, max, step) ->
            step = if step is undefined then 1 else step;
            input = [];
            for i in [0..max] by step
                input.push(i);
        $scope.maxPages  = (total, pageLength) ->
            Math.ceil(total/pageLength)-1;
            
           
