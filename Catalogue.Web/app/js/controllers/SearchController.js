(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http) {
    var doSearch;
    doSearch = function(query) {
      $location.search('q', query.q);
      if (query.q) {
        $rootScope.busy = {
          value: true
        };
        return $http.get('../api/search?' + $.param(query)).success(function(result) {
          $rootScope.busy = {
            value: false
          };
          if (angular.equals(result.query, $scope.query)) {
            return $scope.result = result;
          }
        }).error(function() {
          return $rootScope.busy = {
            value: false
          };
        });
      } else {
        return $scope.result = {};
      }
    };
    $scope.query = {
      q: $location.search()['q'] || '',
      p: 1
    };
    $scope.$watch('query', doSearch, true);
    return $scope.$watch(function() {
      return $location.search()['q'];
    }, function(q) {
      return $scope.query.q = q || '';
    });
  });

}).call(this);
