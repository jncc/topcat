(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http) {
    var search;
    search = function(q) {
      $location.search('q', q);
      if (q) {
        $rootScope.busy = {
          value: true
        };
        return $http.get('../api/search?q=' + q).success(function(results) {
          $rootScope.busy = {
            value: false
          };
          if (results.query === $scope.q.value) {
            return $scope.results = results;
          }
        });
      } else {
        return $scope.results = {};
      }
    };
    $scope.q = {
      value: ''
    };
    $scope.$watch('q.value', search);
    return $scope.$watch(function() {
      return $location.search()['q'];
    }, function(q) {
      return $scope.q.value = q || '';
    });
  });

}).call(this);
