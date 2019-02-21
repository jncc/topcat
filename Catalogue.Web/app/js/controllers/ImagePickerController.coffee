
angular.module('app.controllers').controller 'ImagePickerController',

    ($scope, $http, recordOutput) ->
        $scope.test = "hello"

        $scope.close = () -> $scope.$close $scope.recordOutput

    