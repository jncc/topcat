(function() {
  angular.module('app.controllers').controller('VocabulatorController', function($scope, $http, colourHasher) {
    var clearCurrentVocab, findKeywords, findVocabs, loadVocab, m;
    if (!$scope.vocabulator) {
      $scope.vocabulator = {};
    }
    m = $scope.vocabulator;
    if (!m.allVocabs) {
      m.allVocabs = {};
    }
    if (!m.filteredVocabs) {
      m.filteredVocabs = {};
    }
    if (!m.selectedVocab) {
      m.selectedVocab = {};
    }
    if (!m.loadedVocab) {
      m.loadedVocab = null;
    }
    if (!m.foundKewords) {
      m.foundKeywords = [];
    }
    if (!$scope.vocabulator.found) {
      $scope.vocabulator.found = {};
    }
    $scope.colourHasher = colourHasher;
    clearCurrentVocab = function() {
      m.loadedVocab = {};
      return m.selectedVocab = {};
    };
    if (angular.equals(m.allVocabs, {})) {
      $http.get('../api/vocabularylist').success(function(result) {
        m.allVocabs = result;
        return m.filteredVocabs = result;
      });
    }
    findVocabs = function() {
      var filtered, q, v;
      if (!angular.equals(m.allVocabs, {})) {
        q = $scope.vocabulator.q.toLowerCase();
        console.log(q);
        filtered = (function() {
          var _i, _len, _ref, _results;
          _ref = m.allVocabs;
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            v = _ref[_i];
            if (v.name.toLowerCase().indexOf(q) !== -1) {
              _results.push(v);
            }
          }
          return _results;
        })();
        return m.filteredVocabs = filtered;
      }
    };
    findKeywords = function() {
      if ($scope.vocabulator.q) {
        return $http.get('../api/keywords?q=' + $scope.vocabulator.q).success(function(result) {
          return $scope.vocabulator.foundKeywords = result;
        }).error(function(e) {
          return $scope.notifications.add('Oops! ' + e.message);
        });
      } else {
        return $scope.vocabulator.foundKeywords = [];
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
          return m.loadedVocab = result;
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
