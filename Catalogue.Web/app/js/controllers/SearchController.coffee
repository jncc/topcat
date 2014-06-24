﻿angular.module('app.controllers').controller 'SearchController',
    ($scope, $rootScope, $location, $http, $timeout) ->
        appTitlePrefix = "Topcat:";
         # initial values
        $scope.query = { q: $location.search()['q'] || '', p: 0 , n:25}
        $scope.model = { keyword : { value:'', vocab:''};}
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
        
        # page number changed, don't fire any listeners jsut call seach with new model (only p will have changed)
        $scope.changeKeywordResetPageNumber = (keyword) ->
            $scope.model.keyword = keyword
            $scope.query.p = 0;
            # no need to call keyword search directly as listner registerd
        
        changePageNumber = () ->
            if $scope.query.q.substr(0,8) is 'keyword:' 
                doKeywordSearch();
            else
                doTextSearch();              
        
        # listener for when text entered into search box, either calls textSearch, or updates the model keyword
        decideWhichSearch = () ->
            if $scope.query.q and $scope.query.q.substr(0,8) is 'keyword:' 
                # this updates the model keyword, which then fires the actual search                
                keywordLength = q.length;
                $scope.model.keyword.value = q.substr(8, keywordLength);
                # vocab ignored server side, might have to change this
                $scope.model.keyword.vocab = "not used yet, a user not expected to type in url";
                # no need to call doKeywordSearch as it will done by the listener
            else
                doTextSearch();              
        
        # not called by a listener, invoked by from changePageNumber or decideWhichSearch
        doTextSearch = () ->
            if $scope.query.q 
                # update the url
                $location.url($location.path())
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

        # listner for model Keyword changed, also invoked by changePageNumber and decideWhichsearch
        doKeywordSearch = () ->
            if $scope.model.keyword.value
                $location.url($location.path())
                $location.search('value', $scope.model.keyword.value) 
                $location.search('vocab', $scope.model.keyword.vocab) 
                $location.search('p', $scope.query.p)
                $scope.query.q = "keyword:"+$scope.model.keyword.value
                $location.search('n', $scope.query.n)                       
                $scope.busy.start()
                $http.get("../api/keywordSearch?value="+ $scope.model.keyword.value+"&vocab="+$scope.model.keyword.vocab+"&p="+$scope.query.p+"&n="+$scope.query.n)          
                    .success (result) ->
                        $scope.result = result; 
                        $rootScope.page = { title:appTitlePrefix+$scope.model.keyword.value}; # update the page title#.finally -> 
                    .finally -> $scope.busy.stop()
       
        #  register the three listners
        $scope.$watch 'query.q', decideWhichSearch, true # coul dbe either text or keyword
        $scope.$watch 'model.keyword.value', doKeywordSearch, true # only keyword
        $scope.$watch 'query.p', changePageNumber, true # coul dbe text or keyword

        # when the querystring changes, update the model query value
        # $scope.$watch(
        #    ()  -> $location.search()['q'] #todo watch and update whole querystring
        #    (q) -> $scope.query.q = q || ''
        #)

