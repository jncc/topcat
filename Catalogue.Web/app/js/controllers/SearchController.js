(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, doDefaultSearch, doSearch;
    appTitlePrefix = "Topcat - ";
    $scope.query = {
      q: $location.search()['q'] || '',
      p: 0,
      n: 25
    };
    $scope.keyword = {};
    $scope.keyword.flag = false;
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
    doDefaultSearch = function(q) {
      var keywordLength;
      $scope.query.p = 0;
      if (q.substr(0, 7) === 'keyword:') {
        $scope.keywordFlag = true;
        keywordLength = q.length();
        $scope.keyword.value = q.substr(8, keywordLength);
        $scope.keyword.vocab = "not used yet, a user not expected to type in url";
        return $scope.doKeywordSearch($scope.keyword, $scope.query.p);
      } else {
        $scope.keywordFlag = false;
        $scope.query.q = q;
        return doSearch();
      }
    };
    doSearch = function() {
      if (!$scope.keyword.flag) {
        if ($scope.query.q) {
          $location.search('q', $scope.query.q);
          $location.search('p', $scope.query.p);
          $location.search('n', $scope.query.n);
          $rootScope.page = {
            title: $scope.query.q ? appTitlePrefix + $scope.query.q : appTitlePrefix
          };
          $scope.busy.start();
          return $http.get('../api/search?' + $.param($scope.query)).success(function(result) {
            if (angular.equals(result.query, $scope.query)) {
              return $scope.result = result;
            }
          })["finally"](function() {
            return $scope.busy.stop();
          });
        }
      } else {
        return $scope.doKeywordSearch($scope.keyword, $scope.query.p);
      }
    };
    $scope.doKeywordSearch = function(keyword, pageNumber) {
      $location.url($location.path());
      $location.search('value', keyword.value);
      $location.search('vocab', keyword.vocab);
      $location.search('p', $scope.query.p);
      $location.search('n', $scope.query.n);
      $scope.query.q = "keyword:" + keyword.value;
      $scope.keyword = keyword;
      $scope.keyword.flag = true;
      $scope.busy.start();
      return $http.get("../api/keywordSearch?value=" + keyword.value + "&vocab=" + keyword.vocab + "&p=" + pageNumber + "&n=" + $scope.query.n).success(function(result) {
        $scope.result = result;
        return $rootScope.page = {
          title: appTitlePrefix + keyword.value
        };
      })["finally"](function() {
        return $scope.busy.stop();
      });
    };
    $scope.$watch('query.q', doDefaultSearch, true);
    $scope.$watch('query.p', doSearch, true);
    return $scope.$watch(function() {
      return $location.search()['q'];
    }, function(q) {
      return $scope.query.q = q || '';
    });
  });

}).call(this);
