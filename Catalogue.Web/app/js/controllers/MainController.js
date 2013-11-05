(function() {

  angular.module('app.controllers').controller('MainController', function($scope, $timeout, Account) {
    $scope.app = {
      starting: true
    };
    $timeout(function() {
      return $scope.app.starting = false;
    }, 500);
    return Account.then(function(user) {
      return $scope.user = user;
    });
  });

}).call(this);
