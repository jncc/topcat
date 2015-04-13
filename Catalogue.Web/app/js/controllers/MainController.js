(function() {
  angular.module('app.controllers', ['ui.bootstrap']).controller('MainController', function($scope, $rootScope, $timeout, $cookies, Account) {
    var browserCheckedForSupport, busyCount, notifications, supported, ua;
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
    notifications = [];
    $scope.notifications = {
      current: notifications,
      add: function(message) {
        var n, remove;
        n = {
          message: message
        };
        notifications.push(n);
        remove = function() {
          return notifications.splice($.inArray(n, notifications));
        };
        return $timeout(remove, 4000);
      }
    };
    Account.then(function(user) {
      return $scope.user = user;
    });
    $rootScope.$on('$locationChangeStart', function() {
      return $('.qtip').qtip('hide');
    });
    browserCheckedForSupport = $cookies.browserCheckedForSupport;
    if (!browserCheckedForSupport) {
      $cookies.browserCheckedForSupport = true;
      ua = navigator.userAgent.toLowerCase();
      supported = ua.indexOf('chrome') > 0;
      if (!supported) {
        return $timeout((function() {
          return $scope.notifications.add('Heads up! Topcat currently works best in Chrome');
        }), 3000);
      }
    }
  });

}).call(this);

//# sourceMappingURL=MainController.js.map
