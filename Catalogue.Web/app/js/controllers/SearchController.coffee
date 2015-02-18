angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http, $timeout) ->
        $scope.searchType = {
            fulltext: 'fulltext',
            keyword:  'keyword'
        }
    
        $scope.activeSearchType = $scope.searchType.fulltext
        
        $scope.keyword = ''
        
        $scope.query = { 
            q: $location.search()['q'] || '', 
            k: [$location.search()['k'] || ''], 
            p: 0 , 
            n:25}
    
        getPathFromKeyword = (keyword) ->
            path = ensureEndsWith(keyword.vocab, '/') + keyword.value
            path.replace("http://", "")
 
        getKeywordFromPath = (path) -> 
            elements = path.split('/')
            value = elements[elements.length - 1]
            vocab = "http://"
            for i in [0..elements.length - 2] by 1
                vocab = vocab.concat(elements[i].concat('/'))
            return {value: value, vocab: vocab}
    
        appTitlePrefix = "Topcat:";
         # initial values
         
        ensureEndsWith = (str, suffix) ->
            if str != '' && !(str.indexOf(suffix, str.length - suffix.length) != -1)  
                return str.concat(suffix)
            else
                return str

            
        # slightly hacky way of triggering animations on startup
        # to work around angular skipping the initial animation
        $scope.app = { starting: true };
        $timeout (-> $scope.app.starting = false), 500
        $scope.range  = (min, max, step) ->
            step = if step is undefined then 1 else step;
            input = [];
            for i in [0..max] by step
                input.push(i);
                
        $scope.maxPages  = (total, pageLength) ->
            Math.ceil(total/pageLength)-1;
            
        $rootScope.page = { title:appTitlePrefix } 
        
        $scope.tagSearch = (keyword) ->
            $scope.keyword = keyword
            $scope.activeSearchType = $scope.searchType.keyword
            $scope.query.q = '';
            $scope.query.k = [getPathFromKeyword(keyword)]
            $scope.doSearch()        
        
        # listener for when keywords are entered into the keyword typeahead box        
        $scope.getKeywords = (term) -> $http.get('../api/keywords?q='+term).then (response) -> 
            response.data          
                    
        $scope.doSearch = () ->
            if $scope.query.q || $scope.query.k[0]
                # update the url
                $location.url($location.path())
                $location.search('q', $scope.query.q)
                $location.search('k', $scope.query.k[0])
                $location.search('p', $scope.query.p) 
                $location.search('n', $scope.query.n)
                
                $rootScope.page = {title: appTitlePrefix + $scope.query.q} # update the page title
                $scope.busy.start()

                url = '../api/search?' + $.param $scope.query
                    
                #search server
                $http.get(url)
                    .success (result) ->
                        # don;t overwrite with slwo results
                        if angular.equals result.query, $scope.query
                            if result.total is 0 # without this the browser crashes due to an unsafe object check by angular, when results are zero
                                $scope.result = {} 
                            else
                                $scope.result = result;    
                    .finally -> $scope.busy.stop()        
        

            
        $scope.onKeywordSelect = (keyword, model, label) -> 
            $scope.keyword = keyword
            $scope.query.k = [getPathFromKeyword(keyword)]
            $scope.doSearch()
            
        $scope.switchSearchType = () -> #todo: get rid of this if not needed by radio buttons
            if $scope.activeSearchType == $scope.searchType.keyword 
                $scope.query.q = ''
            else if $scope.activeSearchType == $scope.searchType.fulltext
                $scope.query.k = [''] 
                
        $scope.nextPage = (n) ->
            $scope.query.p = n-1
            $scope.doSearch()
            
        # Work out starting search type
        #fuggle keyword if initialised via qs
        
        if ($scope.query.k[0] != '') 
            $scope.activeSearchType = $scope.searchType.keyword
            $scope.keyword = getKeywordFromPath($scope.query.k[0])
        
        $scope.doSearch()

