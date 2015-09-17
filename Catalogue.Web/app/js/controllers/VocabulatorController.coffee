
angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http, colourHasher) -> 
        
        # define the model
        $scope.vocabulator = {} if !$scope.vocabulator
        # we extend (don't overwrite) the object reference from the parent page
        # because we want to save the state of the vocabulator dialog when closed
        if angular.equals {}, $scope.vocabulator
            angular.extend $scope.vocabulator,
                q:                ''  # the vocabulator search string
                allVocabs:        []  # list of all vocabs, loaded at the start
                filteredVocabs:   []  # filtered vocabs after searching
                selectedVocab:    {}  # the currently selected vocab object from the allVocabs list
                loadedVocab:      {}  # the curently loaded full vocab entity
                foundKeywords:    []  # keywords found after searching
                selectedKeywords: []  # the currently selected keyword(s)
        m = $scope.vocabulator
        
        $scope.colourHasher = colourHasher # make available to the view
            
        # load all the vocabs if not already loaded - we'll filter them client-side
        if !m.allVocabs.length
            $http.get('../api/vocabularylist').success (result) ->
                m.allVocabs = result
                m.filteredVocabs = result

        $scope.doFind = (q, older) ->
            clearCurrentVocab = ->
                m.loadedVocab = {}
                m.selectedVocab = {}
            findVocabs = ->
                q = m.q.toLowerCase()
                filtered = (v for v in m.allVocabs when v.name.toLowerCase().indexOf(q) isnt -1)
                m.filteredVocabs = filtered
            findKeywords = ->
                if m.q is ''
                    m.foundKeywords = []
                else
                    $http.get '../api/keywords?q=' + m.q
                        .success (result) -> m.foundKeywords = result
                        .error (e) -> $scope.notifications.add 'Oops! ' + e.message
            if q isnt older # do nothing initially
                # support adding vocabless keywords - just use what's been typed in!
                #m.selectedKeyword = { vocab: '', value: m.q }
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
                
        $scope.selectKeyword = (k) ->
            m.selectedKeywords.push k unless k in m.selectedKeywords
        $scope.unselectKeyword = (k) ->
            m.selectedKeywords.splice (m.selectedKeywords.indexOf k), 1
                        
        $scope.close = ->
            selectedKeywords = m.selectedKeywords
            m.selectedKeywords = [] # clear the selected keywords for next time
            $scope.$close(selectedKeywords)
        