
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

        if $scope.form.publication != null && $scope.form.publication.openData.assessment.completed
            if $scope.form.publication.openData.assessment.initialAssessmentWasDoneOnSpreadsheet
                $scope.publishingButtonText = "Initial assessment completed on spreadsheet"
            else
                $scope.publishingButtonText = "Completed by " + $scope.form.publication.openData.assessment.completedByUser.displayName +
                    " on " + formatDate(new Date($scope.form.publication.openData.assessment.completedOnUtc))
        else
            $scope.publishingButtonText = "I AGREE"

        $scope.close = ->
            $http.put('../api/publishing/opendata/assess', $scope.assessmentRequest)
            .success (result) ->
                $scope.$close result.record
            .catch (error) ->
                $scope.notifications.add error.data.exceptionMessage
                $scope.$dismiss()