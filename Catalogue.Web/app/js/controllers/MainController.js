(function() {
  angular.module('app.controllers', ['ui.bootstrap']).controller('MainController', function($scope, $rootScope, $timeout, $cookies, Account) {
    var busyCount, notifications, probablyChrome;
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
    probablyChrome = navigator.userAgent.toLowerCase().indexOf('chrome') !== -1;
    if (!probablyChrome) {
      return $scope.notifications.add('Topcat works best on Chrome!');
    }
  });

  angular.module('filters').filter('highlight', function($sce) {
    return function(text, q) {
      var regex;
      regex = new RegExp('(' + q + ')', 'gi');
      if (q) {
        text = text.replace(regex, '<b>$1</b>');
      }
      return $sce.trustAsHtml(text);
    };
  });

}).call(this);

//# sourceMappingURL=MainController.js.map
