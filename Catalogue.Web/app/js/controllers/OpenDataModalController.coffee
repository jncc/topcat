
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
            if $scope.form.publication != null && $scope.form.publication.gov != null
                publishingStatus.riskAssessment.completed = $scope.form.publication.assessment != null && $scope.form.publication.assessment.completed
                publishingStatus.signOff.completed = $scope.form.publication.signOff != null
                publishingStatus.upload.completed = $scope.form.publication.gov.lastSuccess != null

            if $scope.recordOutput.recordState.publishingState.assessedAndUpToDate
                publishingStatus.riskAssessment.currentClass = "visited"
            else if publishingStatus.riskAssessment.completed 
                publishingStatus.riskAssessment.currentClass = "current"
            else
                publishingStatus.riskAssessment.currentClass = "current"

            if $scope.recordOutput.recordState.publishingState.signedOffAndUpToDate
                publishingStatus.signOff.currentClass = "visited"
            else if $scope.recordOutput.recordState.publishingState.assessedAndUpToDate
                publishingStatus.signOff.currentClass = "current"
            else
                publishingStatus.signOff.currentClass = "disabled"

            if $scope.recordOutput.recordState.publishingState.publishedToGovAndUpToDate
                publishingStatus.upload.currentClass = "visited"
            else if $scope.recordOutput.recordState.publishingState.signedOffAndUpToDate
                publishingStatus.upload.currentClass = "current"
            else
                publishingStatus.upload.currentClass = "disabled"

        $scope.refreshPublishingStatus()

        # load initial status screen
        if $scope.recordOutput.recordState.publishingState.signedOffAndUpToDate
            publishingStatus.currentActiveView = "upload"
        else if $scope.recordOutput.recordState.publishingState.assessedAndUpToDate
            publishingStatus.currentActiveView = "sign off"
        else
            publishingStatus.currentActiveView = "risk assessment"

        # Refresh text on assess and sign off buttons
        refreshAssessmentInfo = () ->
            if $scope.form.publication != null && $scope.form.publication.assessment != null && $scope.form.publication.assessment.completed
                if $scope.form.publication.assessment.completedByUser == null && $scope.form.publication.assessment.initialAssessmentWasDoneOnSpreadsheet
                    $scope.assessmentCompletedInfo = "Initial assessment completed on spreadsheet"
                else if $scope.recordOutput.recordState.publishingState.assessedAndUpToDate
                    $scope.assessmentCompletedInfo = "Completed by " + $scope.form.publication.assessment.completedByUser.displayName + " on " + moment(new Date($scope.form.publication.assessment.completedOnUtc)).format('DD MMM YYYY h:mm a')
                else
                    $scope.assessmentCompletedInfo = "Last completed by " + $scope.form.publication.assessment.completedByUser.displayName + " on " + moment(new Date($scope.form.publication.assessment.completedOnUtc)).format('DD MMM YYYY h:mm a')

        refreshSignOffInfo = () -> 
            publishingStatus.signOff.showButton = $scope.user.isIaoUser && !$scope.recordOutput.recordState.publishingState.signedOffAndUpToDate

            if $scope.form.publication != null && $scope.form.publication.signOff != null
                if $scope.form.publication.signOff.user == null
                    $scope.signOffCompletedInfo = "Initial sign off completed on spreadsheet"
                else if $scope.recordOutput.recordState.publishingState.signedOffAndUpToDate
                    $scope.signOffCompletedInfo = "Signed off by " + $scope.form.publication.signOff.user.displayName + " on " + moment(new Date($scope.form.publication.signOff.dateUtc)).format('DD MMM YYYY h:mm a')
                else
                    $scope.signOffCompletedInfo = "Last signed off by " + $scope.form.publication.signOff.user.displayName + " on " + moment(new Date($scope.form.publication.signOff.dateUtc)).format('DD MMM YYYY h:mm a')
            
            if $scope.user.isIaoUser
                publishingStatus.signOff.signOffButtonText = "SIGN OFF"
            else
                publishingStatus.signOff.signOffButtonText = "Pending sign off"

        refreshUploadInfo = () ->
            $scope.hubPublishingStatus = () ->
                if $scope.form.publication.hub == null
                    # never attempted
                    return "Pending"
                else if $scope.form.publication.hub.lastSuccess != null && ($scope.recordOutput.recordState.publishingState.publishedToHubAndUpToDate || $scope.recordOutput.recordState.publishingState.publishedToGovAndUpToDate)
                    # published and up to date
                    return "Completed on " + moment(new Date($scope.form.publication.hub.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a')
                else if $scope.form.publication.hub.lastSuccess != null
                    # published but out of date
                    return "Pending - last completed on " + moment(new Date($scope.form.publication.hub.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a')
                else if $scope.form.publication.hub.lastAttempt != null
                    # attempted before
                    return "Pending - last attempted on " + moment(new Date($scope.form.publication.hub.lastAttempt.dateUtc)).format('DD MMM YYYY h:mm a')
                else
                    # anything else?
                    return "Pending"

            $scope.govPublishingStatus = () ->
                if $scope.form.publication.gov == null
                    # never attempted
                    return "Pending"
                else if $scope.form.publication.gov.lastSuccess != null && $scope.recordOutput.recordState.publishingState.publishedToGovAndUpToDate
                    # published and up to date
                    return "Completed on " + moment(new Date($scope.form.publication.gov.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a')
                else if $scope.form.publication.gov.lastSuccess != null
                    # published but out of date
                    return "Pending - last completed on " + moment(new Date($scope.form.publication.gov.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a')
                else if $scope.form.publication.gov.lastAttempt != null
                    # attempted before
                    return "Pending - last failed on " + moment(new Date($scope.form.publication.gov.lastAttempt.dateUtc)).format('DD MMM YYYY h:mm a') +
                        " with error \"" + $scope.form.publication.gov.lastAttempt.message + "\""
                else
                    # anything else?
                    return "Pending"

            if $scope.recordOutput.recordState.publishingState.publishedToGovAndUpToDate
                $scope.uploadStatus = "Publishing completed"
            else
                $scope.uploadStatus = "Publishing in progress..."


        refreshAssessmentInfo()
        refreshSignOffInfo()
        refreshUploadInfo()

        # Assess and sign off button clicks with timeout for sign off
        $scope.assessButtonClick = ->
            $http.put('../api/publishing/assess', $scope.assessmentRequest)
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

            $http.put('../api/publishing/signoff', $scope.signOffRequest)
            .success (result) ->
                $scope.form = result.record
                $scope.recordOutput =
                    record: result.record
                    recordState: result.recordState
                    publishingPolicy: result.publishingPolicy
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

    