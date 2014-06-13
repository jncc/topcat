(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var doSearch;
    $scope.app = {
      starting: true
    };
    $timeout((function() {
      return $scope.app.starting = false;
    }), 500);
    doSearch = function(query) {
      $location.search('q', query.q);
      $rootScope.page = {
        title: query.q ? ' - ' + query.q : ''
      };
      if (query.q) {
        $scope.busy.start();
        return $http.get('../api/search?' + $.param(query)).success(function(result) {
          if (angular.equals(result.query, $scope.query)) {
            return $scope.result = result;
          }
        })["finally"](function() {
          return $scope.busy.stop();
        });
      } else {
        return $scope.result = {};
      }
    };
    $scope.doKeywordSearch = function(keyword) {
      $scope.busy.start();
      return $http.post('../api/keywordSearch', keyword).success(function(result) {
        $scope.result = result;
        $scope.busy.stop();
        return $rootScope.page = {
          title: keyword
        };
      });
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
