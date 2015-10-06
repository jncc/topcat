angular.module('app.controllers').controller 'SearchController',
    
    ($scope, $rootScope, $location, $http, $timeout, $q, $modal) ->
        
        # trigger startup fade-in animation on startup to work around
        # angular skipping the initial animation - set app.starting to true for 500ms
        $scope.app = starting: true
        $timeout (-> $scope.app.starting = false), 500
        
        $scope.result = results: {}     # the search results
        $scope.current = {}             # the currently selected, zoomed and top result
        $scope.pageSize = 15            # the paging size (todo: why is there on the scope?)
        $scope.vocabulator = {}         # vocabulator scope to save state between modal instances
        $scope.resultsView = 'list'     # results view style (list|grid)

        updateUrl = (query) ->
            blank = blankQuery()
            # update the url querystring to match the query object
            $location.search 'q', query.q || null
            $location.search 'k', query.k # angular does the right thing here
            $location.search 'p', query.p || null
            $location.search 'd', query.d || null
            #$location.search('n', $scope.query.n)
        
        queryRecords = (query) ->
            $http.get('../api/search?' + $.param query, true)
                .success (result) ->
                    # don't overwrite with earlier but slower queries!
                    if angular.equals result.query, query
                        $scope.result = result
                .error (e) -> $scope.notifications.add 'Oops! ' + e.message
        
        queryKeywords = (query) ->
            if query.q # don't want to query server with empty query
                $http.get('../api/keywords?q=' + query.q)
                    .success (result) -> $scope.keywordSuggestions = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
            else
                $q.defer() # return an empty promise
        
        # called whenever the $scope.query object changes
        # (also called explicitly from search button)
        $scope.doSearch = (query) ->
            updateUrl query
            if query.q or query.k[0]
                $scope.busy.start()
                keywordsPromise = queryKeywords query
                recordsPromise  = queryRecords query
                # when all queries complete
                $q.all [keywordsPromise, recordsPromise]
                    .finally ->
                        $scope.busy.stop()
                        if !$scope.result.query.q
                            $scope.keywordSuggestions = {}
            else
                $scope.keywordSuggestions = {}
                $scope.result = {}
                
        blankQuery = ->
            q: '',
            k: [],
            p: 0,
            n: $scope.pageSize,
            d: null

        parseQuerystring = ->
            o = $location.search() # angular api for getting the querystring as an object
            # when there is exactly one keyword, angular's $location.search does not return an array
            # so fix it up (make k an array of one keyword)
            o.k = [o.k] if o.k and not $.isArray o.k
            o.p = o.p * 1 if o.p
            $.extend {}, blankQuery(), o

        # initialise the query to whatever is in the querystring
        $scope.query = parseQuerystring()
        
        # when the model query value is updated, do the search
        $scope.$watch 'query', $scope.doSearch, true

        # when the querystring changes, update the model query value
        # todo this doesn't work very well; let's see if we can do without it
        #$scope.$watch(
        #    ()  -> $location.search()
        #    (x) ->
        #        #console.log $.extend {}, blankQuery(), x
        #        #$scope.query = $.extend {}, blankQuery(), x
        #)
        
        # function to get the current querystring in the view (for constructing export url)
        $scope.querystring = -> $.param $scope.query, true # true means traditional serialization (no square brackets for arrays)

        $scope.addKeywordsToQuery = (keywords) ->
            [keywordsAlreadyInQuery, keywordsToAddToQuery] = _(keywords)
                .map $scope.keywordToString
                .partition (k) -> k in $scope.query.k
                .value()
            # add keywords not already in the query
            $scope.query.k.push k for k in keywordsToAddToQuery
            $scope.notifications.add "Your query already contains the '#{ $scope.keywordFromString(k).value }' keyword" for k in keywordsAlreadyInQuery
            # for usability, when keywords are added, clear the rest of the current query
            if keywordsToAddToQuery.length
                $scope.query = angular.extend {}, blankQuery(), { 'k': $scope.query.k }
                
        $scope.removeKeywordFromQuery = (keyword) ->
            s = $scope.keywordToString keyword
            $scope.query.k.splice ($.inArray s, $scope.query.k), 1
       
        # keyword helper functions            
        $scope.keywordToString = (k) ->
            s = if k.vocab then k.vocab + '/' + k.value else k.value
            s.replace 'http://', ''
        $scope.keywordFromString = (s) -> 
            if (s.indexOf '/') == -1
                vocab: '', value: s
            else
                slash = s.lastIndexOf '/'
                vocab: 'http://' + (s.substring 0, slash),
                value: s.substring (slash + 1)
                
        # map
        #$scope.selectResultOnMap = (r) ->
        #    $scope.map.selected = r
        #    $scope.map.highlighted = r
        
        # vocabulator
        $scope.openVocabulator = ->
            modal = $modal.open
                controller:  'VocabulatorController'
                templateUrl: 'views/partials/vocabulator.html?' + new Date().getTime() # stop iis express caching the html
                size:        'lg'
                scope:       $scope
            modal.result
                .then (keywords) -> $scope.addKeywordsToQuery keywords
    
        # paging helper functions 
                           
        $scope.setPage = (n) ->
            if n > 0 and n <= ($scope.maxPages($scope.result.total, $scope.pageSize) + 1)
                $scope.query.p = n-1
            
        $scope.range  = (min, max, step) ->
            step = if step is undefined then 1 else step;
            input = [];
            for i in [0..max] by step
                input.push(i);
                
        $scope.maxPages  = (total, pageLength) ->
            Math.ceil(total/pageLength)-1;
            
        $scope.blah = (i) ->
            left = i * i * i;
            left + 'px'

