angular.module('app.controllers').controller 'SandboxController',

    ($scope, colourHasher) ->  #, Formats

        $scope.hashStringToColor = colourHasher.hashStringToColour
        $scope.x = 3;
#        Formats.then (formats) -> $scope.formats = formats

