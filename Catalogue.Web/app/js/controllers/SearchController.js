(function() {
  angular.module('app.controllers').controller('SearchController', function($scope, $rootScope, $location, $http, $timeout) {
    var appTitlePrefix, blah, doSearch, ensureEndsWith, getKeywordFromPath, getPathFromKeyword, newQuery;
    appTitlePrefix = "Topcat ";
    $scope.app = {
      starting: true
    };
    $timeout((function() {
      return $scope.app.starting = false;
    }), 500);
    doSearch = function(query) {
      $location.search('q', $scope.query.q);
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
    blah = $.extend({}, newQuery(), $location.search());
    console.log(blah);
    $scope.query = blah;
    $scope.$watch('query', doSearch, true);
    $scope.$watch(function() {
      return $location.search();
    }, function(x) {});
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
    return ensureEndsWith = function(str, suffix) {
      if (str !== '' && !(str.indexOf(suffix, str.length - suffix.length) !== -1)) {
        return str.concat(suffix);
      } else {
        return str;
      }
    };

    /*
    $scope.nextPage = (n) ->
        $scope.query.p = n-1
        $scope.doSearch()
    $scope.range  = (min, max, step) ->
        step = if step is undefined then 1 else step;
        input = [];
        for i in [0..max] by step
            input.push(i);
    $scope.maxPages  = (total, pageLength) ->
        Math.ceil(total/pageLength)-1;
     */
  });

}).call(this);

//# sourceMappingURL=SearchController.js.map
