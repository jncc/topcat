
angular.module('app.controllers').controller 'AssessmentController',

    ($scope, $http) ->
        $scope.assessmentRequest = '{"id": "' + $scope.form.id + '"}'
        if $scope.form.publication != null && $scope.form.publication.openData.assessment.completed
            formatDate = (date) ->
                year = date.getFullYear()
                month = forceTwoDigits(date.getMonth()+1)
                day = forceTwoDigits(date.getDate())
                return "#{day}/#{month}/#{year}"
            forceTwoDigits = (val) ->
                if val < 10
                    return "0#{val}"
                return val
            $scope.publishingButtonText = "Completed by " + $scope.form.publication.openData.assessment.completedBy + " on " + formatDate(new Date($scope.form.publication.openData.assessment.completedOnUtc))
        else
            $scope.publishingButtonText = "I AGREE"
        $scope.close = ->
            $http.put('../api/publishing/opendata/assess', $scope.assessmentRequest)
            .success (result) ->
                $scope.$close result.record
            .catch (error) ->
                $scope.notifications.add error.data.exceptionMessage
                $scope.$dismiss()