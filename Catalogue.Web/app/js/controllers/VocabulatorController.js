(function() {
  angular.module('app.controllers').controller('VocabulatorController', function($scope, $http, colourHasher) {
    var clearCurrentVocab, findKeywords, findVocabs, loadVocab;
    if (!$scope.vocabulator) {
      $scope.vocabulator = {};
    }
    if (!$scope.vocabulator.vocabs) {
      $scope.vocabulator.vocabs = {};
    }
    if (!$scope.vocabulator.vocab) {
      $scope.vocabulator.vocab = {};
    }
    if (!$scope.vocabulator.found) {
      $scope.vocabulator.found = {};
    }
    $scope.colourHasher = colourHasher;
    clearCurrentVocab = function() {
      $scope.vocabulator.vocab = {};
      return $scope.vocabulator.selectedVocab = {};
    };
    if (!$scope.vocabulator.vocabs.all) {
      $http.get('../api/vocabularylist').success(function(result) {
        $scope.vocabulator.vocabs.all = result;
        return $scope.vocabulator.vocabs.filtered = result;
      });
    }
    findVocabs = function() {
      var filtered, q, v;
      if ($scope.vocabulator.vocabs.all) {
        q = $scope.vocabulator.q.toLowerCase();
        filtered = (function() {
          var _i, _len, _ref, _results;
          _ref = $scope.vocabulator.vocabs.all;
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            v = _ref[_i];
            if (v.name.toLowerCase().indexOf(q) !== -1) {
              _results.push(v);
            }
          }
          return _results;
        })();
        return $scope.vocabulator.vocabs.filtered = filtered;
      }
    };
    findKeywords = function() {
      if ($scope.vocabulator.q) {
        return $http.get('../api/keywords?q=' + $scope.vocabulator.q).success(function(result) {
          return $scope.vocabulator.found.keywords = result;
        }).error(function(e) {
          return $scope.notifications.add('Oops! ' + e.message);
        });
      } else {
        return $scope.vocabulator.found.keywords = [];
      }
    };
    $scope.doFind = function(q, older) {
      if (q !== older) {
        $scope.vocabulator.selectedKeyword = {
          vocab: '',
          value: $scope.vocabulator.q
        };
        clearCurrentVocab();
        findVocabs();
        return findKeywords();
      }
    };
    $scope.$watch('vocabulator.q', $scope.doFind);
    loadVocab = function(vocab, old) {
      if (vocab && vocab !== old) {
        return $http.get('../api/vocabularies?id=' + encodeURIComponent(vocab.id)).success(function(result) {
          return $scope.vocabulator.vocab = result;
        }).error(function(e) {
          return $scope.notifications.add('Oops! ' + e.message);
        });
      }
    };
    $scope.$watch('vocabulator.selectedVocab', loadVocab);
    $scope.selectKeyword = function(k) {
      return $scope.vocabulator.selectedKeyword = k;
    };
    return $scope.close = function() {
      return $scope.$close($scope.vocabulator.selectedKeyword);
    };
  });

}).call(this);

//# sourceMappingURL=VocabulatorController.js.map
