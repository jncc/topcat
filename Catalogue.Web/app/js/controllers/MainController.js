(function() {

  angular.module('app.controllers').controller('MainController', function($scope) {
    return $scope.user = {
      displayName: 'Pete Montgomery',
      email: 'pete.montgomery@jncc.gov.uk'
    };
  });

}).call(this);
