(function() {
  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, doSearch, ensureEndsWith, getKeywordFromPath, getPathFromKeyword, newQuery, updateUrl;
    appTitlePrefix = "Topcat ";
    $scope.app = {
      starting: true
    };
    $timeout((function() {
      return $scope.app.starting = false;
    }), 500);
    updateUrl = function(query) {
      return $location.search('q', query.q);
    };
    doSearch = function(query) {
      updateUrl(query);
      if (query.q || query.k[0]) {
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
      } else {
        return $scope.result = {};
      }
    };
    newQuery = function() {
      return {
        q: '',
        k: [null],
        p: 0,
        n: 25
      };
    };
    $scope.$watch('query', doSearch, true);
    $scope.query = $.extend({}, newQuery(), $location.search());
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
