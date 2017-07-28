
angular.module('app.controllers').controller 'AssessmentController',

    ($scope, $http) ->
        $scope.signOffRequest = '{"id": "' + $scope.form.id + '","comment": "Test sign off"}'
        $scope.close = ->
            # Need risk assessment call here instead
            $http.put('../api/publishing/opendata/signoff', $scope.signOffRequest)
            .catch (error) ->
                $scope.notifications.add 'Error updating risk assessment'
                
            $scope.$close($scope.assessment)