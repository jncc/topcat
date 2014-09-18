angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http, $timeout) ->
        appTitlePrefix = "Topcat:";
         # initial values
        $scope.query = { q: $location.search()['q'] || '', p: 0 , n:25}
        $scope.model = { keyword : { value:$location.search()['value'] || '', vocab : $location.search()['vocab'] || ''};}

        $scope.searchType = {
            keyword: 'keyword',
            vocab: 'vocab',
            text: 'text'
        }
        
        if ($scope.model.keyword.value)
            console.log("keyword")
            $scope.model.searchType = $scope.searchType.keyword
            $scope.query.q = $scope.model.keyword.value
        else if ($scope.model.keyword.vocab)
            console.log("vocab")
            $scope.model.searchType = $scope.searchType.vocab
            $scope.query.q = $scope.model.vocab.value
        else 
            console.log("text")
            $scope.model.searchType = $scope.searchType.text
            
        # $scope.model.keyword = {};
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
        
        # update flag, then fire listner
        $scope.changeKeywordResetPageNumber = (keyword) ->
            $scope.model.keyword = keyword
            $scope.query.p = 0;
            $scope.model.searchType = $scope.searchType.keyword
            $scope.query.q = keyword.value
            # no need to call keyword search directly as listner registerd for query q change
        
        # page number changed, don't fire any listeners jsut call seach with new model (only p will have changed)
        $scope.changePageNumber = () ->
            if $scope.model.searchType == $scope.searchType.keyword
                doKeywordSearch();
            else if $scope.model.searchType == $scope.searchType.vocab
                doVocabSearch();
            else
                doTextSearch();              
        
        # listener for when text entered into search box, either calls textSearch, or updates the model keyword
        $scope.decideWhichSearch = () ->
            $scope.query.p = 0 # it must be a new search                
            if $scope.model.searchType == $scope.searchType.keyword
                # this updates the model keyword, which then fires the actual search                
                $scope.model.keyword.value = $scope.query.q
                doKeywordSearch()
            else if $scope.model.searchType == $scope.searchType.vocab
                 $scope.model.keyword.vocab =  $scope.query.q
                 doVocabSearch()
            else
                doTextSearch()    
        
        # listener for when keywords are entered into the keyword typeahead box        
        $scope.getKeywords = (term) -> $http.get('../api/keywords?q='+term).then (response) -> 
            response.data          
        
        # listener for when vocabularies are entered into the keyword typeahead box        
        $scope.getVocabularies = (term) -> $http.get('../api/vocabularies?q='+term).then (response) -> 
            response.data          
        
                    
        # not called by a listener, invoked by from changePageNumber or decideWhichSearch
        doTextSearch = () ->
            if $scope.query.q 
                # update the url
                $location.url($location.path())
                $location.search('q', $scope.query.q) 
                $location.search('p', $scope.query.p) 
                $location.search('n', $scope.query.n)
                $rootScope.page = {title: appTitlePrefix + $scope.query.q} # update the page title
                $scope.busy.start()
                # search the server
                $http.get('../api/search?' + $.param $scope.query)
                    .success (result) ->
                        # don;t overwrite with slwo results
                        if angular.equals result.query, $scope.query
                            if result.total is 0 # without this the browser crashes due to an unsafe object check by angular, when results are zero
                                $scope.result = {} 
                            else
                                $scope.result = result;    
                    .finally -> $scope.busy.stop()

        # listner for model Keyword changed, also invoked by changePageNumber and decideWhichsearch
        doKeywordSearch = () ->
            $location.url($location.path())
            $location.search('value', $scope.model.keyword.value) 
            # $location.search('vocab', $scope.model.keyword.vocab) 
            $location.search('p', $scope.query.p)            
            $location.search('n', $scope.query.n)                       
            $scope.busy.start()
            $http.get("../api/keywordSearch?value="+ $scope.model.keyword.value+"&vocab="+$scope.model.keyword.vocab+"&p="+$scope.query.p+"&n="+$scope.query.n)          
                .success (result) ->
                    # don;t overwrite with slwo results
                    if angular.equals result.query, $scope.query
                        if result.total is 0 # without this the browser crashes due to an unsafe object check by angular, when results are zero
                            $scope.result = {} 
                        else
                            $scope.result = result;                 
                .finally -> 
                    $scope.busy.stop()          
                    $rootScope.page = { title:appTitlePrefix+$scope.model.keyword.value} 
        
        #  not called by a listener, invoked by changePageNumber or decideWhichSearch
        doVocabSearch = () ->
            $location.url($location.path())
            $location.search('vocab', $scope.model.keyword.vocab) 
            $location.search('p', $scope.query.p)            
            $location.search('n', $scope.query.n)                       
            $scope.busy.start()
            $http.get("../api/vocabularySearch?vocab="+$scope.model.keyword.vocab+"&p="+$scope.query.p+"&n="+$scope.query.n)          
                .success (result) ->
                    # don't overwrite with slow results
                    if angular.equals result.query, $scope.query
                        if result.total is 0 # without this the browser crashes due to an unsafe object check by angular, when results are zero
                            $scope.result = {} 
                        else
                            $scope.result = result;                 
                .finally -> 
                    $scope.busy.stop()          
                    $rootScope.page = { title:appTitlePrefix+$scope.model.keyword.vocab} 
          
       
        #  register the three listners
        $scope.$watch 'query.q', $scope.decideWhichSearch, true # coul dbe either text or keyword
        # $scope.$watch 'model.keyword.value', doKeywordSearch, true # only keyword
        # $scope.$watch 'query.p', changePageNumber, true # coul dbe text or keyword
        # $scope.$watch 'model.searchType', decideWhichSearch, true # coul dbe text or keyword

        # when the querystring changes, update the model query value
        #$scope.$watch(
        #    ()  -> $location.search()['q'] #todo watch and update whole querystring
        #    (q) -> $scope.query.q = q || ''
        #)

