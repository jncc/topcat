(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, changePageNumber, decideWhichSearch, doKeywordSearch, doTextSearch;
    appTitlePrefix = "Topcat:";
    $scope.query = {
      q: $location.search()['q'] || '',
      p: 0,
      n: 25
    };
    $scope.model = {
      keyword: {
        value: '',
        vocab: ''
      }
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
    $scope.changeKeywordResetPageNumber = function(keyword) {
      $scope.model.keyword = keyword;
      return $scope.query.p = 0;
    };
    changePageNumber = function() {
      if ($scope.query.q.substr(0, 8) === 'keyword:') {
        return doKeywordSearch();
      } else {
        return doTextSearch();
      }
    };
    decideWhichSearch = function() {
      var keywordLength;
      if ($scope.query.q && $scope.query.q.substr(0, 8) === 'keyword:') {
        keywordLength = q.length;
        $scope.model.keyword.value = q.substr(8, keywordLength);
        return $scope.model.keyword.vocab = "not used yet, a user not expected to type in url";
      } else {
        return doTextSearch();
      }
    };
    doTextSearch = function() {
      if ($scope.query.q) {
        $location.url($location.path());
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
    };
    doKeywordSearch = function() {
      if ($scope.model.keyword.value) {
        $location.url($location.path());
        $location.search('value', $scope.model.keyword.value);
        $location.search('vocab', $scope.model.keyword.vocab);
        $location.search('p', $scope.query.p);
        $scope.query.q = "keyword:" + $scope.model.keyword.value;
        $location.search('n', $scope.query.n);
        $scope.busy.start();
        return $http.get("../api/keywordSearch?value=" + $scope.model.keyword.value + "&vocab=" + $scope.model.keyword.vocab + "&p=" + $scope.query.p + "&n=" + $scope.query.n).success(function(result) {
          $scope.result = result;
          return $rootScope.page = {
            title: appTitlePrefix + $scope.model.keyword.value
          };
        })["finally"](function() {
          return $scope.busy.stop();
        });
      }
    };
    $scope.$watch('query.q', decideWhichSearch, true);
    $scope.$watch('model.keyword.value', doKeywordSearch, true);
    return $scope.$watch('query.p', changePageNumber, true);
  });

}).call(this);
