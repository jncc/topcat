(function() {

  angular.module('app.controllers').controller('MainController', function($scope, $timeout, Account, misc) {
    var busyCount;
    $scope.hashStringToColor = misc.hashStringToColor;
    busyCount = 0;
    $scope.busy = {
      start: function() {
        return busyCount = busyCount + 1;
      },
      stop: function() {
        return busyCount = busyCount - 1;
      },
      value: function() {
        return busyCount > 0;
      }
    };
    $scope.notifications = [];
    return Account.then(function(user) {
      return $scope.user = user;
    });
  });

}).call(this);
