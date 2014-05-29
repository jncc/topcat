
angular.module('app.controllers').controller 'EditorController', 

    ($scope, $http, $routeParams, $location, record, Record) -> 

        # todo lookups should be injected
        $scope.lookups = {}
        $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
    
        # helper functions for UI
        $scope.getSecurityText = getSecurityText

        $scope.cancel = ->
            $scope.reset()
            $scope.notifications.add 'Edits cancelled'
        
        $scope.open = ($event, elem) ->
            $event.preventDefault();
            $event.stopPropagation();
            $scope[elem] = true;
            return
    
        $scope.save = ->
            processResult = (response) ->
                    if response.data.success
                        record = response.data.record # oo-er, is updating a param a good idea?
                        $scope.validation = {}
                        $scope.reset()
                        $scope.notifications.add 'Edits saved'
                        $location.path('/editor/' + record.id)
                    else
                        $scope.validation = response.data.validation
                        # tell the form that fields are invalid
                        errors = response.data.validation.errors
                        if errors.length > 0
                            $scope.notifications.add 'There were errors'
                        for e in errors
                            for field in e.fields
                                $scope.theForm[field].$setValidity('server', false)
                    $scope.busy.stop()

            $scope.busy.start()
            if $routeParams.recordId isnt '00000000-0000-0000-0000-000000000000'
                # todo use resource Record.update ??
                $http.put('../api/records/' + record.id, $scope.form).then processResult
            else
                $http.post('../api/records', $scope.form).then processResult

        $scope.reset = -> $scope.form = angular.copy(record)
        $scope.isClean = -> angular.equals($scope.form, record)
        $scope.isSaveHidden = -> $scope.isClean() or record.readOnly
        $scope.isCancelHidden = -> $scope.isClean()
        $scope.isSaveDisabled = -> $scope.isClean() # || $scope.theForm.$invalid 

        #$scope.keywordEditorOpen = true
        $scope.removeKeyword = (keyword) ->
            $scope.form.gemini.keywords.splice ($.inArray keyword, $scope.form.gemini.keywords), 1
        $scope.addKeyword = ->
            $scope.form.gemini.keywords.push({ vocab: '', value: '' })
                
        # initially set up form
        $scope.reset()

        #$scope.validation = fakeValidationData
        return



getSecurityText = (n) -> switch n
    when 0 then 'Open'
    when 1 then 'Restricted'
    when 2 then 'Classified'

fakeValidationData = errors: [
    { message: 'There was an error' }
    { message: 'There was another error' } ]
        
