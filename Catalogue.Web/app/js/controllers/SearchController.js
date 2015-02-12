(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, ensureEndsWith, getKeywordFromPath, getPathFromKeyword;
    $scope.searchType = {
      fulltext: 'fulltext',
      keyword: 'keyword'
    };
    $scope.activeSearchType = $scope.searchType.fulltext;
    $scope.keyword = '';
    $scope.query = {
      q: $location.search()['q'] || '',
      k: [$location.search()['k'] || ''],
      p: 0,
      n: 25
    };
    getPathFromKeyword = function(keyword) {
      var path;
      path = ensureEndsWith(keyword.vocab, '/') + keyword.value;
      return path.replace("http://", "");
    };
    getKeywordFromPath = function(path) {
      var elements, i, value, vocab, _i, _ref;
      elements = path.split('/');
      value = elements[elements.length - 1];
      vocab = "http://";
      for (i = _i = 0, _ref = elements.length - 2; _i <= _ref; i = _i += 1) {
        vocab = vocab.concat(elements[i].concat('/'));
      }
      return {
        value: value,
        vocab: vocab
      };
    };
    appTitlePrefix = "Topcat:";
    ensureEndsWith = function(str, suffix) {
      if (!(str.indexOf(suffix, str.length - suffix.length) !== -1)) {
        return str.concat(suffix);
      } else {
        return str;
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
    $scope.tagSearch = function(keyword) {
      $scope.keyword = keyword;
      $scope.activeSearchType = $scope.searchType.keyword;
      $scope.query.q = '';
      $scope.query.k = [getPathFromKeyword(keyword)];
      return $scope.doSearch();
    };
    $scope.getKeywords = function(term) {
      return $http.get('../api/keywords?q=' + term).then(function(response) {
        return response.data;
      });
    };
    $scope.doSearch = function() {
      var url;
      if ($scope.query.q || $scope.query.k[0]) {
        $location.url($location.path());
        $location.search('q', $scope.query.q);
        $location.search('k', $scope.query.k[0]);
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
    $scope.onKeywordSelect = function(keyword, model, label) {
      $scope.query.k = [getPathFromKeyword(keyword)];
      return $scope.doSearch();
    };
    $scope.switchSearchType = function() {
      if ($scope.activeSearchType === $scope.searchType.keyword) {
        return $scope.query.q = '';
      } else if ($scope.activeSearchType === $scope.searchType.fulltext) {
        return $scope.query.k = [''];
      }
    };
    $scope.nextPage = function(n) {
      $scope.query.p = n - 1;
      return $scope.doSearch();
    };
    if ($scope.query.k[0] !== '') {
      $scope.activeSearchType = $scope.searchType.keyword;
      $scope.keyword = getKeywordFromPath($scope.query.k[0]);
    }
    return $scope.doSearch();
  });

}).call(this);
