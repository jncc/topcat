
angular.module('app.controllers').controller 'EditorController', 

    ($scope, $http, $routeParams, $location, record, Record) -> 

        # todo lookups should be injected
        $scope.lookups = {}
        $scope.lookups.currentDataFormat = {}
        $scope.lookups.languages = [ 
            {code:'eng', text:'English'},
            {code:'cym', text:'Welsh'},
            {code:'gle', text:'Gaelic (Irish)'},
            {code:'gla', text:'Gaelic (Scottish)'},
            {code:'cor', text:'Cornish'},
            {code:'sco', text:'Ulster Scots'}
        ];
        
        $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
        $http.get('../api/formats?q=').success (result) -> 
            $scope.lookups.currentDataFormat = getDataFormatObj $scope.form.gemini.dataFormat, result
            $scope.lookups.formats = result
        $http.get('../api/roles?q=').success (result) -> $scope.lookups.roles = result            

        $scope.collapseDataFormatSelector = true    
        $scope.collapseDateFormat = true
                                    
        # helper functions for UI
        $scope.getSecurityText = getSecurityText
        $scope.getDataFormatObj = getDataFormatObj
        $scope.updateDataFormatObj = updateDataFormatObj

        $scope.cancel = ->
            $scope.reset()
            $scope.lookups.currentDataFormat = getDataFormatObj $scope.form.gemini.dataFormat, $scope.lookups.formats
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
    
getDataFormatObj = (name, formats) ->
    if (name != undefined && formats != undefined)
        for format in formats
            for dataType in format.formats
                if dataType.name == name
                    return {
                        type: format.name,
                        text: dataType.name,
                        code: dataType.code,
                        glyph: format.glyph
                    }
        return {
            text: 'None Selected',
            glyph: 'glyphicon-th'
        }
    else 
        return {
            text: "Other",
            glyph: 'glyphicon-th'
        }

updateDataFormatObj = (name, formats, form) ->
    form.gemini.dataFormat = name
    getDataFormatObj(name, formats)

fakeValidationData = errors: [
    { message: 'There was an error' }
    { message: 'There was another error' } ]
        
