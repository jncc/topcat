angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http, colourHasher) -> 
        
        $scope.vocabulator = {} if !$scope.vocabulator
        m = $scope.vocabulator

        m.allVocabs = {} if !m.allVocabs            # all vocabs, loaded at the start
        m.filteredVocabs = {} if !m.filteredVocabs  # filtered vocabs after typing / searching
        m.selectedVocab = {} if !m.selectedVocab    # the currently selected vocab object out of allVocabs
        m.loadedVocab = null if !m.loadedVocab      # the curently loaded full vocab entity
        m.foundKeywords = [] if !m.foundKewords     # keywords found after typing / searching
        
        $scope.vocabulator.found  = {} if !$scope.vocabulator.found
        
        # selectedKeyword
        # keywords??
        # scrollPositions
        
        $scope.colourHasher = colourHasher # make available in view
        
        clearCurrentVocab = ->
            m.loadedVocab = {}
            m.selectedVocab = {}
            
        # load all the vocabs if not already loaded - we'll filter them client-side
        if angular.equals m.allVocabs, {}
            $http.get('../api/vocabularylist').success (result) ->
                m.allVocabs = result
                m.filteredVocabs = result
        
        findVocabs = ->
            if !angular.equals m.allVocabs, {} # check they've all loaded
                q = $scope.vocabulator.q.toLowerCase()
                console.log q
                filtered = (v for v in m.allVocabs when v.name.toLowerCase().indexOf(q) isnt -1)
                m.filteredVocabs = filtered
        
        findKeywords = ->
            if $scope.vocabulator.q
                $http.get '../api/keywords?q=' + $scope.vocabulator.q
                    .success (result) -> $scope.vocabulator.foundKeywords = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
            else
                $scope.vocabulator.foundKeywords = []

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
                    .success (result) -> m.loadedVocab = result
                    .error (e) -> $scope.notifications.add 'Oops! ' + e.message
                
        # when a vocab is selected, load the full document (to show all its keywords)
        $scope.$watch 'vocabulator.selectedVocab', loadVocab
                
        $scope.selectKeyword = (k) -> $scope.vocabulator.selectedKeyword = k
                        
        $scope.close = -> $scope.$close($scope.vocabulator.selectedKeyword)
        