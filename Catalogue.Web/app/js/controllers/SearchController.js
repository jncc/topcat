﻿(function() {
  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, ensureEndsWith, getKeywordFromPath, getPathFromKeyword, newQuery, queryKeywords, queryRecords, updateUrl;
    appTitlePrefix = "Topcat ";
    $scope.app = {
      starting: true
    };
    $timeout((function() {
      return $scope.app.starting = false;
    }), 500);
    $scope.resultsView = 'list';
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
      }).error(function() {
        return $scope.keywordSuggestions = {};
      })["finally"](function() {
        return $scope.busy.stop();
      });
    };
    $scope.doSearch = function(query) {
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
    $scope.$watch('query', $scope.doSearch, true);
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
    $scope.querystring = function() {
      return $.param($scope.query);
    };
    getPathFromKeyword = function(keyword) {
      var path;
      path = ensureEndsWith(keyword.vocab, '/') + keyword.value;
      return path.replace("http://", "");
    };
    getKeywordFromPath = function(path) {
      var elements, i, value, vocab, _i, _ref;
      if (path.indexOf('/') === -1) {
        return {
          value: path,
          vocab: ''
        };
      } else {
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
