
angular.module('app.controllers').controller 'AssessmentController',

    ($scope, $http) ->

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
            upload:
                currentClass: {}
                completed: {}
            currentActiveView: {}
            signOffButtonDisabled: !$scope.user.isIaoUser || ($scope.form.publication != null && $scope.form.publication.openData.signOff != null)

        $scope.publishingStatus = publishingStatus

        $scope.refreshPublishingStatus = (activeView) ->
            publishingStatus.currentActiveView = activeView
            publishingStatus.riskAssessment.completed = $scope.form.publication != null && $scope.form.publication.openData.assessment.completed
            publishingStatus.signOff.completed = $scope.form.publication.openData.signOff != null
            publishingStatus.upload.completed = $scope.form.publication.openData.lastSuccess != null
            if activeView == "risk assessment"
                publishingStatus.riskAssessment.currentClass = if publishingStatus.signOff.completed then "visited" else "current"
                publishingStatus.signOff.currentClass = if publishingStatus.signOff.completed then "current" else ""
                publishingStatus.upload.currentClass = ""
                console.log "refreshing publishing status "+activeView+" current class "+publishingStatus.riskAssessment.currentClass
            else if activeView == "sign off"
                console.log "refreshing publishing status "+activeView
                publishingStatus.riskAssessment.currentClass = "visited"
                publishingStatus.signOff.currentClass = "current"
                publishingStatus.upload.currentClass = ""
            else
                publishingStatus.riskAssessment.currentClass = "visited"
                publishingStatus.signOff.currentClass = "visited"
                publishingStatus.upload.currentClass = "current"

        $scope.refreshPublishingStatus("risk assessment")

        refreshAssessmentButton = () ->
            if ($scope.form.publication != null && $scope.form.publication.openData.assessment.completed)
                if $scope.form.publication.openData.assessment.initialAssessmentWasDoneOnSpreadsheet
                    $scope.assessmentButtonText = "Initial assessment completed on spreadsheet"
                else
                    $scope.assessmentButtonText = "Completed by " + $scope.form.publication.openData.assessment.completedByUser.displayName +
                        " on " + formatDate(new Date($scope.form.publication.openData.assessment.completedOnUtc))
            else
                $scope.assessmentButtonText = "I AGREE"

        refreshAssessmentButton()

        if $scope.form.publication != null && $scope.form.publication.openData.signOff != null
            $scope.signOffButtonText = "Signed off by " + $scope.form.publication.openData.signOff.user.displayName +
                " on " + formatDate(new Date($scope.form.publication.openData.signOff.dateUtc))
        else if ($scope.user.isIaoUser)
            $scope.signOffButtonText = "SIGN OFF"
        else
            $scope.signOffButtonText = "Pending sign off"

        $scope.assessClick = ->
            $http.put('../api/publishing/opendata/assess', $scope.assessmentRequest)
            .success (result) ->
                $scope.status.refresh()
                $scope.expandSignOff()
                $scope.form = result.record
                refreshAssessmentButton()
            .catch (error) ->
                $scope.notifications.add error.data.exceptionMessage
                $scope.$dismiss()

        $scope.nextStep = () ->
            if publishingStatus.riskAssessment.currentClass == "current"
                $scope.refreshPublishingStatus("sign off")
            else if publishingStatus.signOff.currentClass == "current"
                scope.refreshPublishingStatus("upload")
            console.log "Show assessment: "+$scope.showAssessmentView+", show sign off: "+$scope.showSignOffView

        $scope.close = () -> $scope.$close $scope.form
