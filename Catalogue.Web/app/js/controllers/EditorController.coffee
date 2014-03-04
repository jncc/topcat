﻿
angular.module('app.controllers').controller 'EditorController', 

    ($scope, $http, record, Record) -> 

        $scope.lookups = {}
        $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
        $scope.state = /^\w\w$/
        $scope.zip = /^\d\d\d\d\d$/

        $scope.getSecurityText = (n) ->
            switch n
              when 0 then 'Open'
              when 1 then 'Restricted'
              when 2 then 'Classified'
        
        $scope.reset = () -> $scope.form = angular.copy(record)

        $scope.save = () ->
            $scope.busy.start()
            $scope.validation = {}
            # todo use resource Record.update ?
            $http.put('../api/records/' + record.id, $scope.form).then (response) ->
                if response.data.success
                    record = response.data.record # oo-er, is updating a param a good idea?
                    $scope.reset()
                else
                    $scope.validation = response.data.validation
                    # tell the form that fields are invalid
                    for e in response.data.validation.errors
                        for field in e.fields
                            $scope.theForm[field].$setValidity('server', false)
                $scope.busy.stop()

        $scope.isClean = -> angular.equals($scope.form, record)
        $scope.isSaveHidden = -> $scope.isClean() or record.readOnly
        $scope.isResetHidden = -> $scope.isClean()
        $scope.isSaveDisabled = -> $scope.isClean() # || $scope.theForm.$invalid 

        #$scope.keywordEditorOpen = true
        $scope.removeKeyword = (keyword) ->
            i = $scope.form.gemini.keywords.indexOf keyword # indexOf doesn't work in ie7!
            $scope.form.gemini.keywords.splice i, 1
        $scope.addKeyword = -> $scope.form.gemini.keywords.push({ vocab: '', value: '' })
                
        $scope.reset() # initially set up form

        $scope.validation = fakeValidationData
        return
    
fakeValidationData =
    errors: [
        { message: 'There was an error' }
        { message: 'There was another error' } ]
        
