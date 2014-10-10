(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, doKeywordSearch, doTextSearch, doVocabSearch;
    appTitlePrefix = "Topcat:";
    $scope.query = {
      q: $location.search()['q'] || '',
      p: 0,
      n: 25
    };
    $scope.model = {
      keyword: {
        value: $location.search()['value'] || '',
        vocab: $location.search()['vocab'] || ''
      }
    };
    $scope.searchType = {
      keyword: 'keyword',
      vocab: 'vocab',
      text: 'text'
    };
    if ($scope.model.keyword.value) {
      console.log("keyword");
      $scope.model.searchType = $scope.searchType.keyword;
      $scope.query.q = $scope.model.keyword.value;
    } else if ($scope.model.keyword.vocab) {
      console.log("vocab");
      $scope.model.searchType = $scope.searchType.vocab;
      $scope.query.q = $scope.model.vocab.value;
    } else {
      console.log("text");
      $scope.model.searchType = $scope.searchType.text;
    }
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
      $scope.query.p = 0;
      $scope.model.searchType = $scope.searchType.keyword;
      return $scope.query.q = keyword.value;
    };
    $scope.changePageNumber = function() {
      if ($scope.model.searchType === $scope.searchType.keyword) {
        return doKeywordSearch();
      } else if ($scope.model.searchType === $scope.searchType.vocab) {
        return doVocabSearch();
      } else {
        return doTextSearch();
      }
    };
    $scope.decideWhichSearch = function() {
      $scope.query.p = 0;
      if ($scope.model.searchType === $scope.searchType.keyword) {
        $scope.model.keyword.value = $scope.query.q;
        return doKeywordSearch();
      } else if ($scope.model.searchType === $scope.searchType.vocab) {
        $scope.model.keyword.vocab = $scope.query.q;
        return doVocabSearch();
      } else {
        return doTextSearch();
      }
    };
    $scope.getKeywords = function(term) {
      return $http.get('../api/keywords?q=' + term).then(function(response) {
        return response.data;
      });
    };
    $scope.getVocabularies = function(term) {
      return $http.get('../api/vocabularytypeahead?q=' + term).then(function(response) {
        return response.data;
      });
    };
    doTextSearch = function() {
      if ($scope.query.q) {
        $location.url($location.path());
        $location.search('q', $scope.query.q);
        $location.search('p', $scope.query.p);
        $location.search('n', $scope.query.n);
        $rootScope.page = {
          title: appTitlePrefix + $scope.query.q
        };
        $scope.busy.start();
        return $http.get('../api/search?' + $.param($scope.query)).success(function(result) {
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
    doKeywordSearch = function() {
      $location.url($location.path());
      $location.search('value', $scope.model.keyword.value);
      $location.search('p', $scope.query.p);
      $location.search('n', $scope.query.n);
      $scope.busy.start();
      return $http.get("../api/keywordSearch?value=" + $scope.model.keyword.value + "&vocab=" + $scope.model.keyword.vocab + "&p=" + $scope.query.p + "&n=" + $scope.query.n).success(function(result) {
        if (angular.equals(result.query, $scope.query)) {
          if (result.total === 0) {
            return $scope.result = {};
          } else {
            return $scope.result = result;
          }
        }
      })["finally"](function() {
        $scope.busy.stop();
        return $rootScope.page = {
          title: appTitlePrefix + $scope.model.keyword.value
        };
      });
    };
    doVocabSearch = function() {
      $location.url($location.path());
      $location.search('vocab', $scope.model.keyword.vocab);
      $location.search('p', $scope.query.p);
      $location.search('n', $scope.query.n);
      $scope.busy.start();
      return $http.get("../api/vocabularySearch?vocab=" + $scope.model.keyword.vocab + "&p=" + $scope.query.p + "&n=" + $scope.query.n).success(function(result) {
        if (angular.equals(result.query, $scope.query)) {
          if (result.total === 0) {
            return $scope.result = {};
          } else {
            return $scope.result = result;
          }
        }
      })["finally"](function() {
        $scope.busy.stop();
        return $rootScope.page = {
          title: appTitlePrefix + $scope.model.keyword.vocab
        };
      });
    };
    return $scope.$watch('query.q', $scope.decideWhichSearch, true);
  });

}).call(this);
