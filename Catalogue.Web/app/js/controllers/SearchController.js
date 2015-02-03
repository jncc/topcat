(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, getPathFromKeyword;
    appTitlePrefix = "Topcat:";
    $scope.keyword = '';
    $scope.query = {
      q: $location.search()['q'] || '',
      k: $location.search()['k'] || '',
      p: 0,
      n: 25
    };
    $scope.app = {
      starting: true
    };
    $timeout((function() {
      return $scope.app.starting = false;
    }), 500);
    $scope.range = function(min, max, step) {
      var i, input, _i, _results;
      step = step === void 0 ? 1 : step;
      input = [];
      _results = [];
      for (i = _i = 0; 0 <= max ? _i <= max : _i >= max; i = _i += step) {
        _results.push(input.push(i));
      }
      return _results;
    };
    $scope.maxPages = function(total, pageLength) {
      return Math.ceil(total / pageLength) - 1;
    };
    $rootScope.page = {
      title: appTitlePrefix
    };
    $scope.tagSearch = function(keyword) {
      $scope.query.p = 0;
      return $scope.query.k = getPathFromKeyword(keyword);
    };
    $scope.getKeywords = function(term) {
      return $http.get('../api/keywords?q=' + term).then(function(response) {
        return response.data;
      });
    };
    $scope.doSearch = function() {
      var url;
      if ($scope.query.q) {
        $location.url($location.path());
        $location.search('q', $scope.query.q);
        $location.search('k', $scope.query.k);
        $location.search('p', $scope.query.p);
        $location.search('n', $scope.query.n);
        $rootScope.page = {
          title: appTitlePrefix + $scope.query.q
        };
        $scope.busy.start();
        url = '../api/search?' + $.param($scope.query);
        return $http.get(url).success(function(result) {
          if (angular.equals(result.query, $scope.query)) {
            if (result.total === 0) {
              return $scope.result = {};
            } else {
              return $scope.result = result;
            }
          }
        })["finally"](function() {
          return $scope.busy.stop();
        });
      }
    };
    getPathFromKeyword = function(keyword) {
      var path;
      path = keyword.vocab + keyword.value;
      path.replace("http://", "");
      return path;
    };
    $scope.onKeywordSelect = function(keyword, model, label) {
      return $scope.query.k = getPathFromKeyword(keyword);
    };
    $scope.decideWhichSearch = function() {};
    return $scope.$watch('query', $scope.doSearch, true);
  });

}).call(this);
