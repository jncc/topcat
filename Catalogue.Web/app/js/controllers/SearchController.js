(function() {
  var __indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout, $q) {
    var blankQuery, parseQuerystring, queryKeywords, queryRecords, updateUrl;
    $scope.app = {
      starting: true
    };
    $timeout((function() {
      return $scope.app.starting = false;
    }), 500);
    $scope.resultsView = 'list';
    updateUrl = function(query) {
      var blank;
      blank = blankQuery();
      $location.search('q', query.q || null);
      return $location.search('k', query.k);
    };
    queryRecords = function(query) {
      return $http.get('../api/search?' + $.param(query, true)).success(function(result) {
        if (angular.equals(result.query, query)) {
          return $scope.result = result;
        }
      }).error(function(e) {
        return $scope.notifications.add('Oops! ' + e.message);
      });
    };
    queryKeywords = function(query) {
      if (query.q) {
        return $http.get('../api/keywords?q=' + query.q).success(function(result) {
          return $scope.keywordSuggestions = result;
        }).error(function(e) {
          return $scope.notifications.add('Oops! ' + e.message);
        });
      } else {
        return $q.defer();
      }
    };
    $scope.doSearch = function(query) {
      var keywordsPromise, recordsPromise;
      updateUrl(query);
      if (query.q || query.k[0]) {
        $scope.busy.start();
        keywordsPromise = queryKeywords(query);
        recordsPromise = queryRecords(query);
        return $q.all([keywordsPromise, recordsPromise])["finally"](function() {
          $scope.busy.stop();
          if (!$scope.result.query.q) {
            return $scope.keywordSuggestions = {};
          }
        });
      } else {
        $scope.keywordSuggestions = {};
        return $scope.result = {};
      }
    };
    blankQuery = function() {
      return {
        q: '',
        k: [],
        p: 0,
        n: 25
      };
    };
    parseQuerystring = function() {
      var o;
      o = $location.search();
      if (o.k && !$.isArray(o.k)) {
        o.k = [o.k];
      }
      return $.extend({}, blankQuery(), o);
    };
    $scope.query = parseQuerystring();
    $scope.$watch('query', $scope.doSearch, true);
    $scope.querystring = function() {
      return $.param($scope.query, true);
    };
    $scope.addKeywordToQuery = function(keyword) {
      var k, s;
      s = $scope.keywordToString(keyword);
      if (__indexOf.call($scope.query.k, s) >= 0) {
        return $scope.notifications.add('Your query already contains this keyword');
      } else {
        k = $scope.query.k;
        k.push(s);
        return $scope.query = $.extend({}, blankQuery(), {
          'k': k
        });
      }
    };
    $scope.removeKeywordFromQuery = function(keyword) {
      return $scope.query.k.splice($.inArray(keyword, $scope.query.k), 1);
    };
    $scope.keywordToString = function(k) {
      var s;
      s = k.vocab ? k.vocab + '/' + k.value : k.value;
      return s.replace('http://', '');
    };
    $scope.keywordFromString = function(s) {
      var slash;
      if ((s.indexOf('/')) === -1) {
        return {
          vocab: '',
          value: s
        };
      } else {
        slash = s.lastIndexOf('/');
        return {
          vocab: 'http://' + (s.substring(0, slash)),
          value: s.substring(slash + 1)
        };
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
    $scope.maxPages = function(total, pageLength) {
      return Math.ceil(total / pageLength) - 1;
    };
    return $scope.gridOptions = {
      data: 'result.results'
    };
  });

}).call(this);

//# sourceMappingURL=SearchController.js.map
