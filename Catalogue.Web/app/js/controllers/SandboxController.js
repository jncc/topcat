(function() {
  angular.module('app.controllers').controller('SandboxController', function($scope, $http, colourHasher) {
    $scope.hashStringToColor = colourHasher.hashStringToColour;
    $scope.x = 3;
    $scope.lookups = {};
    $http.get('../api/topics').success(function(result) {
      return $scope.lookups.topics = result;
    });
    return $http.get('../api/formats?q=').success(function(result) {
      return $scope.lookups.formats = result;
    });
  });

}).call(this);

//# sourceMappingURL=SandboxController.js.map
