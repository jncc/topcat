(function() {
  angular.module('app.controllers').controller('SandboxController', function($scope, colourHasher) {
    $scope.hashStringToColor = colourHasher.hashStringToColour;
    return $scope.x = 3;
  });

}).call(this);

//# sourceMappingURL=SandboxController.js.map
