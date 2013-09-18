(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http) {
    var search;
    search = function(q) {
      if (q) {
        $location.search('q', q);
        $rootScope.busy = {
          value: true
        };
        return $http.get('../api/search?q=' + q).success(function(results) {
          $scope.results = results;
          return $rootScope.busy = {
            value: false
          };
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
      return $scope.query.q = q || '';
    });
  });

}).call(this);
