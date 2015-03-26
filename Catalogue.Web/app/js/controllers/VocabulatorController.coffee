angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http, colourHasher) -> 
        
        $scope.vocabulator = {} if !$scope.vocabulator
        $scope.vocabulator.vocabs = {} if !$scope.vocabulator.vocabs
        $scope.vocabulator.vocab  = {} if !$scope.vocabulator.vocab
        $scope.vocabulator.found  = {} if !$scope.vocabulator.found
                    
        $scope.colourHasher = colourHasher
        
        clearCurrentVocab = ->
            $scope.vocabulator.vocab = {}
            $scope.vocabulator.selectedVocab = {}
            
        # load all the vocabs if not already loaded - we'll filter them client-side
        if !$scope.vocabulator.vocabs.all
            $http.get('../api/vocabularylist').success (result) ->
                $scope.vocabulator.vocabs.all = result
                $scope.vocabulator.vocabs.filtered = result
        
        findVocabs = ->
            if $scope.vocabulator.vocabs.all # check they've all loaded
                q = $scope.vocabulator.q.toLowerCase()
                filtered = (v for v in $scope.vocabulator.vocabs.all when v.name.toLowerCase().indexOf(q) isnt -1)
                $scope.vocabulator.vocabs.filtered = filtered
        
        findKeywords = ->
            if $scope.vocabulator.q
                $http.get '../api/keywords?q=' + $scope.vocabulator.q
                    .success (result) -> $scope.vocabulator.found.keywords = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
            else
                $scope.vocabulator.found.keywords = []

        $scope.doFind = (q, older) ->
            if q isnt older # do nothing initially
                # support adding vocabless keywords - just use what's been typed in!
                $scope.vocabulator.selectedKeyword = { vocab: '', value: $scope.vocabulator.q }
                clearCurrentVocab()
                findVocabs()
                findKeywords()            
                
        # when the model search value is updated, do the search
        $scope.$watch 'vocabulator.q', $scope.doFind
        
        loadVocab = (vocab, old) ->
            if vocab and vocab isnt old
                $http.get '../api/vocabularies?id=' + encodeURIComponent vocab.id
                    .success (result) -> $scope.vocabulator.vocab = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
                
        # when a vocab is selected, load the full document (to show all its keywords)
        $scope.$watch 'vocabulator.selectedVocab', loadVocab
                
        $scope.selectKeyword = (k) -> $scope.vocabulator.selectedKeyword = k
                        
        $scope.close = -> $scope.$close($scope.vocabulator.selectedKeyword)
        