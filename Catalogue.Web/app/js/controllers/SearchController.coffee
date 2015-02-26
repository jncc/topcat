angular.module('app.controllers').controller 'SearchController',
    
    ($scope, $rootScope, $location, $http, $timeout) ->
        
        appTitlePrefix = "Topcat ";
         
        # slightly hacky way of triggering animations on startup to work around
        # angular skipping the initial animation - set app.starting to true for 500ms
        $scope.app = { starting: true };
        $timeout (-> $scope.app.starting = false), 500

        # note: $location.search is the angular api for the querystring value

        updateUrl = (query) -> 
            # update the url querystring to match the query object
            #$location.url($location.path()) wtf?
            $location.search 'q', query.q
            #$location.search('k', $scope.query.k[0])
            #$location.search('p', $scope.query.p) 
            #$location.search('n', $scope.query.n)
        
        suggestKeywords = (query) ->
            if query.q
                $scope.busy.start()
                $http.get('../api/keywords?q=' + query.q)
                    .success (result) ->
                        $scope.keywordSuggestions = result
                    .finally -> $scope.busy.stop()
            else
                $scope.keywordSuggestions = {}
            
        doSearch = (query) ->
            # update the page title  
            #$rootScope.page = {title: appTitlePrefix + $scope.query.q}
            
            updateUrl query
            
            suggestKeywords query
            
            if query.q or query.k[0]
                $scope.busy.start()
                # search server
                $http.get('../api/search?' + $.param query)
                    .success (result) ->
                        # don't overwrite with earlier but slower queries!
                        if angular.equals result.query, query
                            if result.total is 0 # without this the browser crashes due to an unsafe object check by angular, when results are zero
                                $scope.result = {} 
                            else
                                $scope.result = result;    
                    .finally -> $scope.busy.stop()
            else
                $scope.result = {}
                
        newQuery = ->
            q: '',
            k: [null],
            p: 0, 
            n: 25

        # when the model query value is updated, do the search
        $scope.$watch 'query', doSearch, true

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
        
        
        
        $rootScope.page = { title:appTitlePrefix } 
        



        # listener for when keywords are entered into the keyword typeahead box        
        $scope.getKeywords = (term) -> $http.get('../api/keywords?q='+term).then (response) -> 
            response.data                              



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
            
           
