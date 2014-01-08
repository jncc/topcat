(function() {

  angular.module('app.controllers').controller('SandboxController', function($scope, Formats) {
    return Formats.then(function(formats) {
      return $scope.formats = formats;
    });
  });

}).call(this);
