angular.module('app.controllers').controller 'VocabularyEditorController',

    ($scope, $http, $routeParams, $location, vocab) -> 
        $scope.reset = -> 
            $scope.form = angular.copy(vocab)
            if (vocab.id)
                $scope.newVocab = false
            else
                $scope.newVocab = true
        
        $scope.removeKeyword = (index) ->
            $scope.form.keywords.splice index, 1
            
        $scope.addKeyword = ->
            $scope.form.keywords.push({id: '', value:''})
            
        $scope.isClean = -> angular.equals($scope.form, vocab)
        $scope.isSaveHidden = -> $scope.isClean()
        $scope.isCancelHidden = -> $scope.isClean()
        
        $scope.save = ->
            processResult = (response) ->
                if response.data.success
                    vocab = response.data.vocab 
                    $scope.form = angular.copy(vocab)
                    $scope.validation = {}
                    $scope.reset()
                    $scope.notifications.add 'Edits saved'
                    $location.path('/vocabularies/editor/' + vocab.id)
                else
                    $scope.validation = response.data.validation
                    # tell the form that fields are invalid
                    errors = response.data.validation.errors
                    if errors.length > 0
                        $scope.notifications.add 'There were errors'
#                        for e in errors
#                            for field in e.fields
#                                $scope.theForm[field].$setValidity('server', false)
                                
                $scope.busy.stop()

            $scope.busy.start()
            if $routeParams.vocabId isnt '0'
                $http.put('../api/vocabularies?id=' + vocab.id, $scope.form).then processResult
            else
                $http.post('../api/vocabularies', $scope.form).then processResult
        
        # initially set up form
        $scope.reset()
        
        