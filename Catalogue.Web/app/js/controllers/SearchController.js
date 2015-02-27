(function() {
  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, doSearch, ensureEndsWith, getPathFromKeyword, newQuery, queryKeywords, queryRecords, updateUrl;
    appTitlePrefix = "Topcat ";
    $scope.app = {
      starting: true
    };
    $timeout((function() {
      return $scope.app.starting = false;
    }), 500);
    updateUrl = function(query) {
      $location.search('q', query.q || null);
      return $location.search('k', query.k || null);
    };
    queryRecords = function(query) {
      $scope.busy.start();
      return $http.get('../api/search?' + $.param(query)).success(function(result) {
        if (angular.equals(result.query, query)) {
          if (result.total === 0) {
            return $scope.result = {};
          } else {
            return $scope.result = result;
          }
        }
      }).error(function(e) {
        return $scope.notifications.add('Oops! ' + e.message);
      })["finally"](function() {
        return $scope.busy.stop();
      });
    };
    queryKeywords = function(query) {
      $scope.busy.start();
      return $http.get('../api/keywords?q=' + query.q).success(function(result) {
        return $scope.keywordSuggestions = result;
      })["finally"](function() {
        return $scope.busy.stop();
      });
    };
    doSearch = function(query) {
      updateUrl(query);
      if (query.q) {
        queryKeywords(query);
        return queryRecords(query);
      } else if (query.k) {
        $scope.keywordSuggestions = {};
        return queryRecords(query);
      } else {
        $scope.keywordSuggestions = {};
        return $scope.result = {};
      }
    };
    $scope.$watch('query', doSearch, true);
    newQuery = function() {
      return {
        q: null,
        k: null,
        p: 0,
        n: 25
      };
    };
    $scope.query = $.extend({}, newQuery(), $location.search());
    $scope.queryByKeyword = function(keyword) {
      return $scope.query = $.extend({}, newQuery(), {
        'k': getPathFromKeyword(keyword)
      });
    };
    $rootScope.page = {
      title: appTitlePrefix
    };
    $scope.getKeywords = function(term) {
      return $http.get('../api/keywords?q=' + term).then(function(response) {
        return response.data;
      });
    };
    getPathFromKeyword = function(keyword) {
      var path;
      path = ensureEndsWith(keyword.vocab, '/') + keyword.value;
      return path.replace("http://", "");
    };
    $scope.getKeywordFromPath = function(path) {
      var elements;
      if (path) {
        if (path.indexOf('/') === -1) {
          return {
            value: path,
            vocab: ''
          };
        } else {
          elements = path.split('/');
          console.log(elements);
          return {
            value: '',
            vocab: ''
          };
        }
      } else {
        return {
          value: '',
          vocab: ''
        };
      }
    };
    ensureEndsWith = function(str, suffix) {
      if (str !== '' && !(str.indexOf(suffix, str.length - suffix.length) !== -1)) {
        return str.concat(suffix);
      } else {
        return str;
      }
    };
    $scope.setPage = function(n) {
      return $scope.query.p = n - 1;
    };
    $scope.range = function(min, max, step) {
      var i, input, _i, _results;
      step = step === void 0 ? 1 : step;
      input = [];
      _results = [];
      for (i = _i = 0; step > 0 ? _i <= max : _i >= max; i = _i += step) {
        _results.push(input.push(i));
      }
      return _results;
    };
    return $scope.maxPages = function(total, pageLength) {
      return Math.ceil(total / pageLength) - 1;
    };
  });

}).call(this);

//# sourceMappingURL=SearchController.js.map
