
angular.module('app.controllers').controller 'OpenDataModalController',

    ($scope, $http, $timeout, recordOutput) ->

        formatDate = (date) ->
            year = date.getFullYear()
            month = useTwoDigits(date.getMonth()+1)
            day = useTwoDigits(date.getDate())
            return "#{day}/#{month}/#{year}"
        useTwoDigits = (val) ->
            if val < 10
                return "0#{val}"
            return val

        $scope.assessmentRequest = {}
        $scope.assessmentRequest.id = $scope.form.id
        $scope.recordOutput = recordOutput

        publishingStatus =
            riskAssessment:
                currentClass: {}
                completed: {}
            signOff:
                currentClass: {}
                completed: {}
                timeout: {}
                showButton: {}
            upload:
                currentClass: {}
                completed: {}
            currentActiveView: {}

        $scope.publishingStatus = publishingStatus
        publishingStatus.signOff.timeout = -1

        # change multistep status
        $scope.refreshPublishingStatus = () ->            
            publishingStatus.riskAssessment.completed = $scope.form.publication != null && $scope.form.publication.openData.assessment.completed
            publishingStatus.signOff.completed = $scope.form.publication != null && $scope.form.publication.openData.signOff != null
            publishingStatus.upload.completed = $scope.form.publication != null && $scope.form.publication.openData.lastSuccess != null

            if $scope.recordOutput.recordState.openDataPublishingState.assessedAndUpToDate
                publishingStatus.riskAssessment.currentClass = "visited"
            else if publishingStatus.riskAssessment.completed 
                publishingStatus.riskAssessment.currentClass = "current"
            else
                publishingStatus.riskAssessment.currentClass = "current"

            if $scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate
                publishingStatus.signOff.currentClass = "visited"
            else if $scope.recordOutput.recordState.openDataPublishingState.assessedAndUpToDate
                publishingStatus.signOff.currentClass = "current"
            else
                publishingStatus.signOff.currentClass = "disabled"

            if $scope.recordOutput.recordState.openDataPublishingState.uploadedAndUpToDate
                publishingStatus.upload.currentClass = "visited"
            else if $scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate
                publishingStatus.upload.currentClass = "current"
            else
                publishingStatus.upload.currentClass = "disabled"

        $scope.refreshPublishingStatus()

        # load initial status screen
        if $scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate
            publishingStatus.currentActiveView = "upload"
        else if $scope.recordOutput.recordState.openDataPublishingState.assessedAndUpToDate
            publishingStatus.currentActiveView = "sign off"
        else
            publishingStatus.currentActiveView = "risk assessment"

        # Refresh text on assess and sign off buttons
        refreshAssessmentInfo = () ->
            if $scope.form.publication != null && $scope.form.publication.openData.assessment.completed
                if $scope.form.publication.openData.assessment.completedByUser == null && $scope.form.publication.openData.assessment.initialAssessmentWasDoneOnSpreadsheet
                    $scope.assessmentCompletedInfo = "Initial assessment completed on spreadsheet"
                else if $scope.recordOutput.recordState.openDataPublishingState.assessedAndUpToDate
                    $scope.assessmentCompletedInfo = "Completed by " + $scope.form.publication.openData.assessment.completedByUser.displayName + " on " + moment(new Date($scope.form.publication.openData.assessment.completedOnUtc)).format('DD MMM YYYY h:mm a')
                else
                    $scope.assessmentCompletedInfo = "Last completed by " + $scope.form.publication.openData.assessment.completedByUser.displayName + " on " + moment(new Date($scope.form.publication.openData.assessment.completedOnUtc)).format('DD MMM YYYY h:mm a')

        refreshSignOffInfo = () -> 
            publishingStatus.signOff.showButton = $scope.user.isIaoUser && !$scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate

            if $scope.form.publication != null && $scope.form.publication.openData.signOff != null
                if $scope.form.publication.openData.signOff.user == null
                    $scope.signOffCompletedInfo = "Initial sign off completed on spreadsheet"
                else if $scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate
                    $scope.signOffCompletedInfo = "Signed off by " + $scope.form.publication.openData.signOff.user.displayName + " on " + moment(new Date($scope.form.publication.openData.signOff.dateUtc)).format('DD MMM YYYY h:mm a')
                else
                    $scope.signOffCompletedInfo = "Last signed off by " + $scope.form.publication.openData.signOff.user.displayName + " on " + moment(new Date($scope.form.publication.openData.signOff.dateUtc)).format('DD MMM YYYY h:mm a')
            
            if $scope.user.isIaoUser
                publishingStatus.signOff.signOffButtonText = "SIGN OFF"
            else
                publishingStatus.signOff.signOffButtonText = "Pending sign off"

        refreshUploadInfo = () ->
            if $scope.form.publication != null
                if $scope.form.publication.openData.lastAttempt != null
                    $scope.uploadLastAttempted = moment(new Date($scope.form.publication.openData.lastAttempt.dateUtc)).format('DD MMM YYYY h:mm a')
                if $scope.form.publication.openData.lastSuccess != null
                    $scope.uploadLastSucceeded = moment(new Date($scope.form.publication.openData.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a')

            if $scope.recordOutput.recordState.openDataPublishingState.uploadedAndUpToDate
                $scope.uploadStatus = "Upload completed"
            else
                $scope.uploadStatus = "Pending upload"

        refreshAssessmentInfo()
        refreshSignOffInfo()
        refreshUploadInfo()

        # Assess and sign off button clicks with timeout for sign off
        $scope.assessButtonClick = ->
            $http.put('../api/publishing/opendata/assess', $scope.assessmentRequest)
            .success (result) ->
                $scope.status.refresh()
                $scope.form = result.record
                $scope.recordOutput =
                    record: result.record
                    recordState : result.recordState
                refreshAssessmentInfo()
                $scope.refreshPublishingStatus()
                publishingStatus.currentActiveView = "sign off"
            .catch (error) ->
                $scope.notifications.add error.data.exceptionMessage
                $scope.$dismiss()

        $scope.signOffButtonClick = () ->
            if publishingStatus.signOff.timeout == -1
                publishingStatus.signOff.timeout = 10 # seconds
                $scope.allowGraceTime()
            else
                $scope.cancelSignOff()

        $scope.submitSignOff = () ->
            $scope.signOffRequest = { id: $scope.form.id, comment: "" }

            $http.put('../api/publishing/opendata/signoff', $scope.signOffRequest)
            .success (result) ->
                $scope.form = result.record
                $scope.recordOutput =
                    record: result.record
                    recordState: result.recordState
                $scope.status.refresh()
                refreshSignOffInfo()
                $scope.refreshPublishingStatus()
                publishingStatus.currentActiveView = "upload"
            .catch (error) ->
                if (error.status == 401)
                    $scope.notifications.add "Unauthorised - not in valid sign off group"
                else
                    $scope.notifications.add error.data.exceptionMessage

                publishingStatus.signOff.timeout = -1
                $scope.$dismiss()

        $scope.allowGraceTime = () ->
            if (publishingStatus.signOff.timeout > 0)
                publishingStatus.signOff.signOffButtonText = "Cancel " + ("0" + publishingStatus.signOff.timeout--).slice(-2)
                $timeout $scope.allowGraceTime, 1000
            else if publishingStatus.signOff.timeout == 0
                $timeout.cancel
                $scope.submitSignOff()

        $scope.cancelSignOff = () ->
            $timeout.cancel
            publishingStatus.signOff.timeout = -1
            refreshSignOffInfo()

        $scope.close = () -> $scope.$close $scope.recordOutput

    