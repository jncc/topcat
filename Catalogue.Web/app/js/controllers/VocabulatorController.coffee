angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http, colourHasher) -> 
        
        $scope.vocabs = {}
        $scope.vocab = {}
        $scope.find = {}
        $scope.found = {}
        $scope.selected = {}
        
        $scope.colourHasher = colourHasher
        
        clearCurrentVocab = ->
            $scope.vocab = {}
            $scope.selected.vocab = {}
            
        # load all the vocabs - we'll filter them client-side
        $http.get('../api/vocabularylist').success (result) ->
            $scope.vocabs.all = result
            $scope.vocabs.filtered = result
        
        findVocabs = ->
            if $scope.vocabs.all # check they've all loaded
                q = $scope.find.text.toLowerCase()
                filtered = (v for v in $scope.vocabs.all when v.name.toLowerCase().indexOf(q) isnt -1)
                $scope.vocabs.filtered = filtered
        
        findKeywords = ->
            if $scope.find.text
                $http.get '../api/keywords?q=' + $scope.find.text
                    .success (result) -> $scope.found.keywords = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
            else
                $scope.found.keywords = []

        $scope.doFind = ->
            clearCurrentVocab()
            findVocabs()
            findKeywords()            
                
        # when the model search value is updated, do the search
        $scope.$watch 'find.text', $scope.doFind, true
        
        loadVocab = (vocab) ->
            if vocab
                $http.get '../api/vocabularies?id=' + encodeURIComponent vocab.id
                    .success (result) -> $scope.vocab = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
                
        # when a vocab is selected, load the full document (to show all its keywords)
        $scope.$watch 'selected.vocab', loadVocab
                
        $scope.selectKeyword = (k) -> $scope.selected.keyword = k
                        
        $scope.close = -> $scope.$close($scope.selected.keyword)
        