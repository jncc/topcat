
angular.module('app.controllers').controller 'AssessmentController',

    ($scope, $http) ->
        $scope.assessmentRequest = '{"id": "' + $scope.form.id + '"}'
        $scope.assessmentResult = {}
        $scope.close = ->
            $http.put('../api/publishing/opendata/assess', $scope.assessmentRequest)
            .success (result) ->
                $scope.$close result.record
            .catch (error) ->
                $scope.notifications.add error.data.exceptionMessage
                $scope.$dismiss()