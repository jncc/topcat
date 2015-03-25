angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http, colourHasher) -> 
        
        if !$scope.vocabulator
            $scope.vocabulator = 
                vocabs: {}
                vocab: {}
                find: {}
                found: {}
                selected: {}
                    
        $scope.colourHasher = colourHasher
        
        clearCurrentVocab = ->
            $scope.vocabulator.vocab = {}
            $scope.vocabulator.selected.vocab = {}
            
        # load all the vocabs - we'll filter them client-side
        $http.get('../api/vocabularylist').success (result) ->
            $scope.vocabulator.vocabs.all = result
            $scope.vocabulator.vocabs.filtered = result
        
        findVocabs = ->
            if $scope.vocabulator.vocabs.all # check they've all loaded
                q = $scope.vocabulator.find.text.toLowerCase()
                filtered = (v for v in $scope.vocabulator.vocabs.all when v.name.toLowerCase().indexOf(q) isnt -1)
                $scope.vocabulator.vocabs.filtered = filtered
        
        findKeywords = ->
            if $scope.vocabulator.find.text
                $http.get '../api/keywords?q=' + $scope.vocabulator.find.text
                    .success (result) -> $scope.vocabulator.found.keywords = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
            else
                $scope.vocabulator.found.keywords = []

        $scope.doFind = ->
            # support adding vocabless keywords - just use what's been typed in!
            $scope.vocabulator.selected.keyword = { vocab: '', value: $scope.vocabulator.find.text }
            clearCurrentVocab()
            findVocabs()
            findKeywords()            
                
        # when the model search value is updated, do the search
        $scope.$watch 'vocabulator.find.text', $scope.doFind, true
        
        loadVocab = (vocab) ->
            if vocab
                $http.get '../api/vocabularies?id=' + encodeURIComponent vocab.id
                    .success (result) -> $scope.vocabulator.vocab = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
                
        # when a vocab is selected, load the full document (to show all its keywords)
        $scope.$watch 'vocabulator.selected.vocab', loadVocab
                
        $scope.selectKeyword = (k) -> $scope.vocabulator.selected.keyword = k
                        
        $scope.close = -> $scope.$close($scope.vocabulator.selected.keyword)
        