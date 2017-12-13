angular.module('app.controllers').controller 'EditorController',

    ($scope, $http, $routeParams, $location, record, $modal, Account) -> 

        Account.then (user) -> $scope.user = user

        $scope.editing = {}
        $scope.lookups = {}
        $scope.lookups.currentDataFormat = {}
        $scope.recordOutput = record
        
        # store a vocabulator scope here to save state between modal instances
        $scope.vocabulator = {}
        
        # todo please delete this now
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
        $scope.getPendingSignOff = getPendingSignOff
        $scope.getOpenDataButtonText = getOpenDataButtonText
        $scope.getOpenDataButtonToolTip = getOpenDataButtonToolTip
        
        $scope.cancel = ->
            $scope.reset()
            $scope.lookups.currentDataFormat = getDataFormatObj $scope.form.gemini.dataFormat, $scope.lookups.formats
            $scope.notifications.add 'Edits cancelled'
        
        # todo er, what's this?!?
        $scope.open = ($event, elem) ->
            $event.preventDefault();
            $event.stopPropagation();
            $scope[elem] = true;
            return
    
        $scope.successResponse = (response) ->
            $scope.reloadRecord response
            $scope.notifications.add 'Edits saved'

        $scope.reloadRecord = (response) ->
            $scope.recordOutput =
                record: response.record
                recordState: response.recordState
            $scope.validation = {}
            $scope.reset()
            $scope.status.refresh()
            $location.path('/editor/' + $scope.recordOutput.record.id)

        $scope.save = ->
            processResult = (response) ->
                if response.data.success
                    $scope.successResponse response.data
                else
                    $scope.validation = response.data.validation
                    # tell the form that fields are invalid
                    errors = response.data.validation.errors
                    if errors.length > 0
                        $scope.notifications.add 'There were errors'
                        for e in errors
                            for field in e.fields
                                try
                                    $scope.theForm[field].$setValidity('server', false)
                $scope.busy.stop()

            $scope.busy.start()

            if $scope.isNew()
                $http.post('../api/records', $scope.form).then processResult
            else
                $http.put('../api/records/' + $scope.recordOutput.record.id, $scope.form).then processResult

        $scope.clone = ->
            $location.path( '/clone/' + $scope.form.id )
            
        $scope.reset = -> 
            $scope.form = angular.copy($scope.recordOutput.record)

        $scope.isNew = -> $scope.form.id is '00000000-0000-0000-0000-000000000000'
        $scope.isClean = -> angular.equals($scope.form, $scope.recordOutput.record)
        $scope.isSaveHidden = -> $scope.isClean() or $scope.recordOutput.record.readOnly
        $scope.isCancelHidden = -> $scope.isClean()
        $scope.isSaveDisabled = -> $scope.isClean() # || $scope.theForm.$invalid 
        $scope.isCloneHidden = -> $scope.isNew()
        $scope.isCloneDisabled = -> !$scope.isClean()
        $scope.isHttpPath = (path) -> path and path.toLowerCase().startsWith "http"
        $scope.isPublishingModalButtonEnabled = -> isFilePath($scope.form.path) and $scope.isSaveHidden()
        $scope.isPublishingModalButtonVisible = -> $scope.form.publication && $scope.form.publication.openData && $scope.form.publication.openData.publishable == true
        $scope.hasUsageConstraints = () -> (!!$scope.form.gemini.limitationsOnPublicAccess and $scope.form.gemini.limitationsOnPublicAccess isnt 'no limitations') or (!!$scope.form.gemini.useConstraints and $scope.form.gemini.useConstraints isnt 'no conditions apply')

        # keywords # update arg name and use cs in
        $scope.removeKeyword = (keyword) ->
            $scope.form.gemini.keywords.splice ($.inArray keyword, $scope.form.gemini.keywords), 1
        $scope.addKeywords = (keywords) ->
            $scope.form.gemini.keywords.push k for k in keywords
        $scope.editKeywords = ->
            # vocabulator
            modal = $modal.open
                controller:  'VocabulatorController'
                templateUrl: 'views/partials/vocabulator.html?' + new Date().getTime() # stop iis express caching the html
                size:        'lg'
                scope:       $scope
            modal.result
                .then (keywords) -> 
                    $scope.addKeywords keywords
                .finally -> $scope.editing.keywords = true # doing this now looks better
            
        $scope.editAbstract = ->
            modal = $modal.open
                controller:  'MarkdownController'
                templateUrl: 'views/partials/markdown.html?' + new Date().getTime() # stop iis express caching the html
                size:        'lg'
                resolve:     'markdown': -> $scope.form.gemini.abstract
            modal.result
                .then (s) -> $scope.form.gemini.abstract = s

        $scope.openPublishingModal = ->
            modal = $modal.open
                controller:  'OpenDataModalController'
                templateUrl: 'views/partials/opendatamodal.html?' + new Date().getTime() # stop iis express caching the html
                size:        'lg'
                scope:       $scope
                resolve:     'recordOutput': -> $scope.recordOutput
                backdrop:  'static'
            modal.result
                .then (result) -> $scope.reloadRecord result

        $scope.removeExtent = (extent) ->
            $scope.form.gemini.extent.splice ($.inArray extent, $scope.form.gemini.extent), 1
        $scope.addExtent = ->
            if ($scope.form.gemini.extent == null)
                $scope.form.gemini.extent = []
            $scope.form.gemini.extent.push({ authority: '', code: '' })            
                
        # initially set up form
        $scope.reset()
        
        $scope.getKeywords = (term) -> $http.get('../api/keywords?q='+term).then (response) -> 
            response.data
        
        $scope.setKeyword = ($item, keyword) ->
            keyword.vocab = $item.vocab

        $scope.fillManagerDetails = () ->
            if not $scope.form.manager then $scope.form.manager = {}
            $scope.form.manager.displayName = $scope.user.displayName

        $scope.setPublishable = (value) ->
            if !$scope.form.publication
                $scope.form.publication = {}

            if !$scope.form.publication.openData
                $scope.form.publication.openData = {}

            if (value == true && $scope.form.publication.openData.publishable == true) or (value == false && $scope.form.publication.openData.publishable == false)
                $scope.form.publication.openData.publishable = null
            else
                $scope.form.publication.openData.publishable = value

        $scope.togglePublishable = () ->
            if !$scope.form.publication
                $scope.form.publication = {}

            if !$scope.form.publication.openData
                $scope.form.publication.openData = {}
                $scope.form.publication.openData.publishable = null

            if $scope.form.publication.openData.publishable == null
                $scope.form.publication.openData.publishable = true
            else if $scope.form.publication.openData.publishable == true
                $scope.form.publication.openData.publishable = false
            else
                $scope.form.publication.openData.publishable = null



isFilePath = (path) -> path and path.match /^([a-z]:|\\\\jncc-corpfile\\)/i

getOpenDataButtonToolTip = (record, publishingState) ->
    if !isFilePath(record.path)
        return "Open data publishing not available for non-file resources"
    else if record.publication == null or record.publication.openData == null or record.publication.openData.publishable != true
        return "The open data publication status of the record, editing the record may affect the status."
    else if record.publication.openData.lastSuccess != null && record.publication.openData.lastSuccess != undefined && !publishingState.assessedAndUpToDate
        return "This record has been changed since it was last published, it may need republishing."
    else
        return "The open data publication status of the record, editing the record may affect the status."

getOpenDataButtonText = (record, publishingState) ->
    if record.publication == null || record.publication.openData == null || record.publication.openData.publishable != true
        return "Publishable"
    else if record.publication.openData.lastSuccess != null && record.publication.openData.lastSuccess != undefined && !publishingState.assessedAndUpToDate
        return "Republish"
    else if publishingState.uploadedAndUpToDate
        return "Published"
    else if publishingState.signedOffAndUpToDate
        return "Signed Off"
    else if publishingState.assessedAndUpToDate
        return "Assessed"
    else
        return "Publishable"

getPendingSignOff = (publication) ->
    if (publication != null && publication.openData.assessment.completed && publication.openData.signOff == null)
        return true
    else
        return false

getSecurityText = (n) -> switch n
    when 0 then 'Official'
    when 1 then 'Official-Sensitive'
    when 2 then 'Secret'
    
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

        