

angular.module('app.controllers').controller 'SandboxController', 

    ($scope, Formats) -> 

        Formats.then (formats) -> $scope.formats = formats
