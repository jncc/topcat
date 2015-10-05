
angular.module('app.controllers').controller 'VocabulatorController',

    ($scope, $http, colourHasher) -> 
        
        blankModel = ->
            q:                      ''  # the vocabulator search string
            allVocabs:              []  # list of all vocabs, loaded at the start
            filteredVocabs:         []  # filtered vocabs after searching
            selectedVocab:          {}  # the currently selected vocab object from the allVocabs list
            loadedVocab:            {}  # the curently loaded full vocab entity
            foundKeywords:          []  # keywords found after searching
            selectedKeywords:       []  # the currently selected keyword(s)
            newUncontrolledKeyword: ''  # a new keyword with the currently selected (uncontrolled) vocab
        
        # we extend (don't overwrite) the object reference from the parent page
        # because we want to save the state of the vocabulator dialog when closed
        $scope.vocabulator = {} if !$scope.vocabulator
        angular.extend $scope.vocabulator, blankModel() if angular.equals {}, $scope.vocabulator
        m = $scope.vocabulator
        
        $scope.colourHasher = colourHasher # make available to the view
        
        # load all the vocabs if not already loaded - we'll filter them client-side
        if !m.allVocabs.length
            $http.get('../api/vocabularylist').success (result) ->
                m.allVocabs = result
                m.filteredVocabs = result

        getSelectedVocabfulKeywords = -> _.filter m.selectedKeywords, (k) -> k.vocab isnt ''
        
        $scope.doFind = (q, older) ->
            suggestKeywordsFromSearchString = (s) ->
                _(m.q.split /[,;]+/) # split on ',' and ';'
                    .map (s) -> { vocab: '', value: s.trim() }
                    .filter (k) -> k.value isnt ''
                    .value()
            updateSelectedKeywords = ->
                # if no vocabful keywords already selected, add vocabless ones from the search string
                if (!_.some getSelectedVocabfulKeywords())
                    # clear selected keywords when search string is deleted
                    if q is '' and older isnt ''
                        m.selectedKeywords = []
                    # parse the search string to suggest vocabless keywords
                    else if q isnt ''
                        m.selectedKeywords = suggestKeywordsFromSearchString q
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
            updateSelectedKeywords()
            if q isnt older # do nothing initially
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
                
        # when a vocab is selected, load the full vocab entity (to show all its keywords)
        $scope.$watch 'vocabulator.selectedVocab', loadVocab
        
        $scope.$watch 'vocabulator.newUncontrolledKeyword', (keyword, old) ->
            if keyword isnt '' and keyword isnt old
                # clear keywords that may have been suggested by a search the query
                if (!_.some getSelectedVocabfulKeywords())
                    m.selectedKeywords.length = 0
                
        $scope.selectKeyword = (k) ->
            # remove vocabless keywords (ie what's been entered in the search box)
            _.remove m.selectedKeywords, (k) -> k.vocab is ''
            m.selectedKeywords.push k unless k in m.selectedKeywords
        $scope.unselectKeyword = (k) ->
            m.selectedKeywords.splice (m.selectedKeywords.indexOf k), 1
                        
        $scope.close = ->
            # clear the model for next time before returning the result of the dialog
            selectedKeywords = m.selectedKeywords
            angular.extend $scope.vocabulator, blankModel()
            $scope.$close(selectedKeywords)
        