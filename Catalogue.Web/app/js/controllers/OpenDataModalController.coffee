
angular.module('app.controllers').controller 'OpenDataModalController',

    ($scope, $http, $timeout) ->

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

        publishingStatus =
            riskAssessment:
                currentClass: {}
                completed: {}
            signOff:
                currentClass: {}
                completed: {}
                timeout: {}
                signOffButtonDisabled: {}
                signOffButtonClass: {}
            upload:
                currentClass: {}
                completed: {}
            currentActiveView: {}

        $scope.publishingStatus = publishingStatus
        publishingStatus.signOff.timeout = 0

        $scope.refreshPublishingStatus = () ->            
            publishingStatus.riskAssessment.completed = $scope.form.publication != null && $scope.form.publication.openData.assessment.completed
            publishingStatus.signOff.completed = $scope.form.publication != null && $scope.form.publication.openData.signOff != null
            publishingStatus.upload.completed = $scope.form.publication != null && $scope.form.publication.openData.lastSuccess != null
            
            if publishingStatus.upload.completed
                publishingStatus.riskAssessment.currentClass = "visited"
                publishingStatus.signOff.currentClass = "visited"
                publishingStatus.upload.currentClass = "visited"
            else if publishingStatus.signOff.completed
                publishingStatus.riskAssessment.currentClass = "visited"
                publishingStatus.signOff.currentClass = "visited"
                publishingStatus.upload.currentClass = "current"
            else if publishingStatus.riskAssessment.completed
                publishingStatus.riskAssessment.currentClass = "visited"
                publishingStatus.signOff.currentClass = "current"
                publishingStatus.upload.currentClass = "disabled"
            else
                publishingStatus.riskAssessment.currentClass = "current"
                publishingStatus.signOff.currentClass = "disabled"
                publishingStatus.upload.currentClass = "disabled"

        $scope.refreshPublishingStatus()

        # load initial status screen
        if publishingStatus.upload.completed || publishingStatus.signOff.completed
            publishingStatus.currentActiveView = "upload"
        else if publishingStatus.riskAssessment.completed
            publishingStatus.currentActiveView = "sign off"
        else
            publishingStatus.currentActiveView = "risk assessment"

        refreshAssessmentButton = () ->
            if ($scope.form.publication != null && $scope.form.publication.openData.assessment.completed)
                if $scope.form.publication.openData.assessment.initialAssessmentWasDoneOnSpreadsheet
                    $scope.assessmentButtonText = "Initial assessment completed on spreadsheet"
                else
                    $scope.assessmentButtonText = "Completed by " + $scope.form.publication.openData.assessment.completedByUser.displayName +
                        " on " + formatDate(new Date($scope.form.publication.openData.assessment.completedOnUtc))
            else
                $scope.assessmentButtonText = "I AGREE"

        refreshSignOffButton = () -> 
            publishingStatus.signOff.signOffButtonDisabled = !$scope.user.isIaoUser || ($scope.form.publication != null && $scope.form.publication.openData.signOff != null)

            if $scope.form.publication != null && $scope.form.publication.openData.signOff != null
                publishingStatus.signOff.signOffButtonClass = "btn btn-primary"
                publishingStatus.signOff.signOffButtonText = "Signed off by " + $scope.form.publication.openData.signOff.user.displayName +
                    " on " + formatDate(new Date($scope.form.publication.openData.signOff.dateUtc))
            else if ($scope.user.isIaoUser)
                publishingStatus.signOff.signOffButtonClass = "btn btn-danger sign-off"
                publishingStatus.signOff.signOffButtonText = "SIGN OFF"
            else
                publishingStatus.signOff.signOffButtonClass = "btn btn-primary"
                publishingStatus.signOff.signOffButtonText = "Pending sign off"

        refreshAssessmentButton()
        refreshSignOffButton()

        $scope.assessClick = ->
            $http.put('../api/publishing/opendata/assess', $scope.assessmentRequest)
            .success (result) ->
                $scope.status.refresh()
                $scope.form = result.record
                refreshAssessmentButton()
                $scope.refreshPublishingStatus()
                publishingStatus.currentActiveView = "sign off"
            .catch (error) ->
                $scope.notifications.add error.data.exceptionMessage
                $scope.$dismiss()

        # duplicated from SignOffController, need to refactor this
        $scope.submitSignOff = () ->
            $scope.signOffRequest = { id: $scope.form.id, comment: "" }

            $http.put('../api/publishing/opendata/signoff', $scope.signOffRequest)
            .success (result) ->
                $scope.status.refresh()
                $scope.form = result.record
                refreshSignOffButton()
                $scope.refreshPublishingStatus()
                publishingStatus.currentActiveView = "upload"
            .catch (error) ->
                if (error.status == 401)
                    $scope.notifications.add "Unauthorised - not in valid sign off group"
                else
                    $scope.notifications.add error.data.exceptionMessage

                publishingStatus.signOff.timeout = 0
                $scope.$dismiss()

        $scope.allowGraceTime = () ->
            if (publishingStatus.signOff.timeout > 0)
                publishingStatus.signOff.signOffButtonText = "Cancel " + ("0" + publishingStatus.signOff.timeout--).slice(-2)
                $timeout $scope.allowGraceTime, 1000
            else
                $scope.submitSignOff()

        $scope.cancelSignOff = () ->
            publishingStatus.signOff.timeout = 0
            refreshSignOffButton()

        $scope.signOffButtonClick = () ->
            if  publishingStatus.signOff.timeout == 0
                publishingStatus.signOff.timeout = 10 # seconds
                $scope.allowGraceTime()
            else
                $scope.cancelSignOff()


        $scope.close = () -> $scope.$close $scope.form
