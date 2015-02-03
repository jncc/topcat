angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http, $timeout) ->
        appTitlePrefix = "Topcat:";
         # initial values

        $scope.keyword = ''
         
        $scope.query = { 
            q: $location.search()['q'] || '', 
            k: $location.search()['k'] || '', 
            p: 0 , 
            n:25 }
            
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
        
        $scope.tagSearch = (keyword) ->
            $scope.query.p = 0;
            $scope.query.k = getPathFromKeyword(keyword)        
        
        # listener for when keywords are entered into the keyword typeahead box        
        $scope.getKeywords = (term) -> $http.get('../api/keywords?q='+term).then (response) -> 
            response.data          
                    
        $scope.doSearch = () ->
            if $scope.query.q 
                # update the url
                $location.url($location.path())
                $location.search('q', $scope.query.q)
                $location.search('k', $scope.query.k)
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
        
        getPathFromKeyword = (keyword) ->
            path = keyword.vocab + keyword.value
            path.replace("http://", "")
            path
            
        $scope.onKeywordSelect = (keyword, model, label) -> 
            $scope.query.k = getPathFromKeyword(keyword)
            
        $scope.decideWhichSearch = () -> #todo: get rid of this if not needed by radio buttons
            
        #  register the three listners
        $scope.$watch 'query', $scope.doSearch, true # could  be either text or keyword
        # $scope.$watch 'model.keyword.value', doKeywordSearch, true # only keyword
        # $scope.$watch 'query.p', changePageNumber, true # coul dbe text or keyword
        # $scope.$watch 'model.searchType', decideWhichSearch, true # coul dbe text or keyword

        # when the querystring changes, update the model query value
        #$scope.$watch(
        #    ()  -> $location.search()['q'] #todo watch and update whole querystring
        #    (q) -> $scope.query.q = q || ''
        #)

