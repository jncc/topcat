(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, doSearch;
    appTitlePrefix = "Topcat - ";
    $scope.app = {
      starting: true
    };
    $timeout((function() {
      return $scope.app.starting = false;
    }), 500);
    doSearch = function(query) {
      $location.search('q', query.q);
      $location.search('p', query.p);
      $rootScope.page = {
        title: query.q ? appTitlePrefix + query.q : appTitlePrefix
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
    $scope.doKeywordSearch = function(keyword, pageNumber) {
      var searchInputModel;
      searchInputModel = {};
      searchInputModel.keyword = keyword;
      searchInputModel.pageNumber = pageNumber;
      searchInputModel.numberOfRecords = 25;
      $scope.busy.start();
      return $http.post('../api/keywordSearch', searchInputModel).success(function(result) {
        $scope.result = result;
        $scope.busy.stop();
        return $rootScope.page = {
          title: appTitlePrefix + keyword
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
