
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

        $scope.assessmentWidth = "85%"
        $scope.signOffWidth = "15%"

        $scope.showSignOffInfo = $scope.form.publication != null && $scope.form.publication.openData.completed
        $scope.signOffButtonDisabled = !$scope.user.isIaoUser || ($scope.form.publication != null && $scope.form.publication.openData.signOff != null)

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

        $scope.expandSignOff = ->
            $scope.assessmentWidth = "50%"
            $scope.signOffWidth = "50%"

        $scope.close = () -> $scope.$close $scope.form
            