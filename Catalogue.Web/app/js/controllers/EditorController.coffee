angular.module('app.controllers').controller 'EditorController',

    ($scope, $http, $routeParams, $location, record, $modal) -> 
    
        $scope.editing = {}
        $scope.lookups = {}
        $scope.lookups.currentDataFormat = {}
        
        #Allow jQuery calls so we can use some of it's convenient tests, No DOM mods please
        $scope.$ = $
        
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
        
        # todo er, what's this?!
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
                        #console.log errors
                        for e in errors
                            for field in e.fields
                                try
                                    $scope.theForm[field].$setValidity('server', false)
                                catch e 
                                    console.log field
                                    console.log e
                                                                
                                
                $scope.busy.stop()

            $scope.busy.start()

            if $scope.isNew()
                $http.post('../api/records', $scope.form).then processResult
            else
                $http.put('../api/records/' + record.id, $scope.form).then processResult

        $scope.reset = -> 
            $scope.form = angular.copy(record)
            
        $scope.isNew = -> $routeParams.recordId is '00000000-0000-0000-0000-000000000000'
        $scope.isClean = -> angular.equals($scope.form, record)
        $scope.isSaveHidden = -> $scope.isClean() or record.readOnly
        $scope.isCancelHidden = -> $scope.isClean()
        $scope.isSaveDisabled = -> $scope.isClean() # || $scope.theForm.$invalid 

        $scope.hasUsageConstraints = () -> (!!$scope.form.gemini.limitationsOnPublicAccess and $scope.form.gemini.limitationsOnPublicAccess isnt 'no limitations') or (!!$scope.form.gemini.useConstraints and $scope.form.gemini.useConstraints isnt 'no conditions apply')

        # keywords
        $scope.removeKeyword = (k) ->
            $scope.form.gemini.keywords.splice ($.inArray k, $scope.form.gemini.keywords), 1
        $scope.addKeyword = (k) ->
            $scope.form.gemini.keywords.push(k)
        $scope.editKeywords = ->
            # vocabulator
            modal = $modal.open
                controller:  'VocabulatorController'
                templateUrl: 'views/partials/vocabulator.html?' + new Date().getTime() # stop iis express caching the html
                size:        'lg'
                scope:       $scope
            modal.result
                .then (k) -> 
                    $scope.addKeyword k
                .finally -> $scope.editing.keywords = true # doing this now looks better
            
            
        $scope.removeExtent = (extent) ->
            $scope.form.gemini.extent.splice ($.inArray extent, $scope.form.gemini.extent), 1
        $scope.addExtent = ->
            if ($scope.form.gemini.extent == null)
                $scope.form.gemini.extent = []
            $scope.form.gemini.extent.push({ authority: '', code: '' })            
                
        # initially set up form
        $scope.reset()

        #$scope.validation = fakeValidationData
        
        $scope.getKeywords = (term) -> $http.get('../api/keywords?q='+term).then (response) -> 
            response.data
        
        $scope.setKeyword = ($item, keyword) ->
            keyword.vocab = $item.vocab


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
        
