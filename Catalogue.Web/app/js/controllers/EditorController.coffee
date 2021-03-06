﻿angular.module('app.controllers').controller 'EditorController',

    ($scope, $http, $routeParams, $location, record, $modal, Account) ->

        Account.then (user) -> $scope.user = user
        
        $scope.editing = {}
        $scope.lookups = {}
        $scope.lookups.currentDataFormat = {}
        $scope.recordOutput = record

        $scope.publishingNotifications = getPublishingNotifications record.record.publication, record.recordState.publishingState
        
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
        $scope.getPublishingText = getPublishingText
        $scope.getPublishButtonTooltip = getPublishButtonTooltip
        $scope.getFormattedDate = getFormattedDate
        $scope.addPublishableResource = addPublishableResource
        $scope.removePublishableResource = removePublishableResource
        $scope.trimDoubleQuotes = trimDoubleQuotes
        $scope.getResourceUrl = getResourceUrl
        
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
            $scope.publishingNotifications = getPublishingNotifications response.record.publication, response.recordState.publishingState
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
        $scope.isPublishDisabled = -> !$scope.isSaveHidden() || $scope.form.validation != 1
        $scope.isPublishHidden = -> !($scope.form.publication && $scope.form.publication.target && ($scope.form.publication.target.hub && $scope.form.publication.target.hub.publishable == true ||
            $scope.form.publication.target.gov && $scope.form.publication.target.gov.publishable == true))
        $scope.isHttpPath = (path) -> path and path.toLowerCase().startsWith "http"
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
                controller:  'PublishingModalController'
                templateUrl: 'views/partials/publishingmodal.html?' + new Date().getTime() # stop iis express caching the html
                size:        'lg'
                scope:       $scope
                resolve:     'recordOutput': -> $scope.recordOutput
                backdrop:  'static'
            modal.result
                .then (result) -> $scope.reloadRecord result

        $scope.openImagePicker = ->
            modal = $modal.open
                controller:  'ImagePickerController'
                templateUrl: 'views/partials/imagepicker.html?' + new Date().getTime() # stop iis express caching the html
                size:        'lg'
                scope:       $scope
                resolve:     'recordImage': -> $scope.form.image
            modal.result
                .then (image) -> $scope.form.image = image

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



isFilePath = (path) -> path and path.match /^([a-z]:|\\\\jncc-corpfile\\)/i

addPublishableResource = (record) ->
    if !record.resources
        record.resources = []
    record.resources.push { path: "" }

getResourceUrl = (resource) ->
    if resource.path.startsWith("http://") || resource.path.startsWith("https://")
        return resource.path
    else if resource.publishedUrl != null
        return resource.publishedUrl
    else
        return null

removePublishableResource = (record, resource) ->
    record.resources.splice ($.inArray resource, record.resources), 1

trimDoubleQuotes = (s) -> # removes double quotes surrounding a string
    if s.match(/^(").*(")$/) then s.substring(1, s.length - 1) else s

getPublishButtonTooltip = (record) ->
    if record.validation == 1
        return "View the publishing workflow"
    else
        return "Validation level must be Gemini"

getPublishingText = (record, publishingState) ->
    previouslyPublishedText = "Never Published"
    publishingStatusText = null

    if record.publication && record.publication.target && (record.publication.target.gov && record.publication.target.gov.lastSuccess || record.publication.target.hub && record.publication.target.hub.lastSuccess)
        previouslyPublishedText = "Published"

    if record.publication && record.publication.target
        if previouslyPublishedText == "Published" && !publishingState.assessedAndUpToDate
            publishingStatusText = "Out Of Date"
        else if publishingState.signedOffAndUpToDate && !publishingState.publishedToHubAndUpToDate && !publishingState.publishedToGovAndUpToDate
            publishingStatusText = "Signed Off"
        else if publishingState.assessedAndUpToDate && !publishingState.publishedToHubAndUpToDate && !publishingState.publishedToGovAndUpToDate
            publishingStatusText = "Assessed"

    if publishingStatusText != null
        return previouslyPublishedText + ", " + publishingStatusText
    else
        return previouslyPublishedText

getFormattedDate = (date) ->
    return moment(new Date(date)).format('DD MMM YYYY')

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

getPublishingNotifications = (publication, publishingState) ->
    if publication && publication.target && publishingState
        notifications = []
        if publishingState.assessedAndUpToDate && !publishingState.signedOffAndUpToDate
            notifications.push {message: 'Awaiting publishing sign off, editing this record will remove it from the approvals list and require resubmission'}
        if (publication.data && publication.data.lastAttempt && publication.data.lastAttempt.message) || (publication.target.hub && publication.target.hub.lastAttempt && publication.target.hub.lastAttempt.message) || (publication.target.gov && publication.target.gov.lastAttempt && publication.target.gov.lastAttempt.message)
            notifications.push {message: 'Failure during a previous publishing attempt, please contact Topcat support'}

        return notifications

    return null

fakeValidationData = errors: [
    { message: 'There was an error' }
    { message: 'There was another error' } ]

        