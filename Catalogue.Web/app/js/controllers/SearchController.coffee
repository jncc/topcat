angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http, $timeout) ->
        appTitlePrefix = "Topcat - ";
         # initial values
        $scope.query = { q: $location.search()['q'] || '', p: 0 , n:25}
        $scope.keyword = {};
        $scope.keyword.flag = false;
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
        
        # search term changed, reset to page 1
        doDefaultSearch = (q) ->
            $scope.query.p = 0; 
            if q.substr(0,8) is 'keyword:' 
                if $scope.keyword.value isnt q.substr(8, keywordLength)  # infinite loop without this check
                    $scope.keywordFlag = true;     
                    keywordLength = q.length;
                    $scope.keyword.value = q.substr(8, keywordLength);
                    # vocab ignored server side, might have to change this
                    $scope.keyword.vocab = "not used yet, a user not expected to type in url";
                    $scope.doKeywordSearch($scope.keyword,  $scope.query.p)
            else
                $scope.keywordFlag = false;  
                $scope.query.q = q;
                doSearch();              
                                   
        doSearch = () ->
            if not $scope.keyword.flag 
                if $scope.query.q 
                    # update the url
                    $location.search('q', $scope.query.q) 
                    $location.search('p', $scope.query.p) 
                    $location.search('n', $scope.query.n)
                    $rootScope.page = { title: if $scope.query.q then appTitlePrefix + $scope.query.q else appTitlePrefix } # update the page title
                    $scope.busy.start()
                    # search the server
                    $http.get('../api/search?' + $.param $scope.query)
                        .success (result) ->
                            # don't overwrite with old slow results!
                            if angular.equals result.query, $scope.query
                                $scope.result = result
                        .finally -> $scope.busy.stop()
            else 
                $scope.doKeywordSearch($scope.keyword, $scope.query.p);
            
        $scope.doKeywordSearch = (keyword, pageNumber) ->
            $location.url($location.path())
            $location.search('value', keyword.value) 
            $location.search('vocab', keyword.vocab) 
            $location.search('p', $scope.query.p)
            $location.search('n', $scope.query.n)           
            $scope.keyword = keyword
            $scope.keyword.flag = true
            $scope.busy.start()
            $http.get("../api/keywordSearch?value="+ keyword.value+"&vocab="+keyword.vocab+"&p="+pageNumber+"&n="+$scope.query.n)          
                .success (result) ->
                    $scope.result = result; 
                    $rootScope.page = { title:appTitlePrefix+keyword.value}; # update the page title#.finally -> 
                .finally -> $scope.busy.stop()
       
        # when the model query value is updated, do the search, and reset page to zero
        $scope.$watch 'query.q', doDefaultSearch, true
        
        # when the model page value is updated just do search with existing scope params
        $scope.$watch 'query.p', doSearch, true

        # when the querystring changes, update the model query value
        # $scope.$watch(
        #    ()  -> $location.search()['q'] #todo watch and update whole querystring
        #    (q) -> $scope.query.q = q || ''
        #)

