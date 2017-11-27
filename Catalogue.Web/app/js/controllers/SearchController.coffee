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
            $location.search 'f.keywords', query.f.keywords # angular does the right thing here
            $location.search 'p', query.p || null
            $location.search 'o', query.o || null
            $location.search 'f.metadataDate', query.f.metadataDate || null
            $location.search 'f.dataFormats', query.f.dataFormats || null
            $location.search 'f.manager', query.f.manager || null
            #$location.search('n', $scope.query.n)
        
        queryRecords = (query) ->
            $http.get('../api/search?' + $.param query, false)
                .success (result) ->
                    # don't overwrite with earlier but slower queries!
                    if moreOrLessTheSame result.query, query
                        $scope.result = result
                .error (e) -> $scope.notifications.add 'Oops! ' + e.message
        
        queryKeywords = (query) ->
            if query.q # don't want to query server with empty query
                $http.get('../api/keywords?q=' + query.q)
                    .success (result) -> $scope.keywordSuggestions = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
            else
                $q["defer"]() # return an empty promise https://github.com/madskristensen/WebCompiler/issues/202

        moreOrLessTheSame = (resultQuery, query) ->
            # ugly hack to get empty array and null to be the same because on the UI side the array is empty,
            # but it's passed to the API as null
            groomedQuery = {}
            angular.copy query, groomedQuery
            if groomedQuery.f
                if groomedQuery.f.dataFormats and groomedQuery.f.dataFormats.length is 0
                    groomedQuery.f.dataFormats = null
                if groomedQuery.f.keywords and groomedQuery.f.keywords.length is 0
                    groomedQuery.f.keywords = null
            return angular.equals resultQuery, groomedQuery

        # called whenever the $scope.query object changes
        # (also called explicitly from search button)
        $scope.doSearch = (query) ->
            updateUrl query
            if query.q or (query.f and (query.f.keywords and query.f.keywords[0]) or (query.f.dataFormats and query.f.dataFormats[0]) or query.f.manager)
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
            f:
                keywords: [],
                dataFormats: [],
                metadataDate: null,
                manager: null
            p: 0,
            n: $scope.pageSize,
            o: 0

        # all sort options in correct order
        $scope.sortOptions = [ "Most relevant", "Title A-Z", "Title Z-A", "Newest to oldest", "Oldest to newest" ]

        $scope.dataFormatOptions = [ "Database", "Spreadsheet", "Documents", "Geospatial", "Image", "Audio", "Video", "Other" ]

        parseQuerystring = ->
            o = $location.search() # angular api for getting the querystring as an object
            # when there is exactly one keyword, angular's $location.search does not return an array
            # so fix it up (make k an array of one keyword)
            if o.f
                o.f.keywords = [o.f.keywords] if o.f.keywords and not $.isArray o.f.keywords
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
        $scope.querystring = -> $.param $scope.query, false # false means non-traditional serialization (square brackets for arrays)

        # using this redundant array so that the checkboxes in IE work properly
        $scope.dataFormatSelections = []

        $scope.addOrRemoveDataFormat = (dataFormat) ->
            if !$scope.query.f.dataFormats
                $scope.query.f.dataFormats = []

            if $scope.query.f.dataFormats.indexOf(dataFormat) != -1
                $scope.query.f.dataFormats.splice($scope.query.f.dataFormats.indexOf(dataFormat), 1)
                $scope.dataFormatSelections[dataFormat] = false
            else
                $scope.query.f.dataFormats.push(dataFormat)
                $scope.dataFormatSelections[dataFormat] = true

        $scope.removeManager = () -> $scope.query.f.manager = null

        $scope.addKeywordsToQuery = (keywords) ->
            if !$scope.query.f.keywords
                $scope.query.f.keywords = []
            [keywordsAlreadyInQuery, keywordsToAddToQuery] = _(keywords)
                .map $scope.keywordToString
                .partition (k) -> k in $scope.query.f.keywords
                .value()
            # add keywords not already in the query
            $scope.query.f.keywords.push k for k in keywordsToAddToQuery
            $scope.notifications.add "Your query already contains the '#{ $scope.keywordFromString(k).value }' keyword" for k in keywordsAlreadyInQuery
            # for usability, when keywords are added, clear the rest of the current query
            if keywordsToAddToQuery.length
                $scope.query = _.merge {}, blankQuery(), { 'f': $scope.query.f }
                
        $scope.removeKeywordFromQuery = (keyword) ->
            s = $scope.keywordToString keyword
            $scope.query.f.keywords.splice ($.inArray s, $scope.query.f.keywords), 1
       
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
            
        $http.get('../api/collections').success (result) ->
            # chunk the collections into two columns [ [a, b], [c, d] ... ]
            $scope.collections = _.chunk result, 2

        $http.get('../api/usage').success (result) ->
            $scope.recentModifications = result.recentlyModifiedRecords
            
