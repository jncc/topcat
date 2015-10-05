(function() {
  var __indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  angular.module('app.controllers').controller('VocabulatorController', function($scope, $http, colourHasher) {
    var blankModel, getSelectedVocabfulKeywords, loadVocab, m;
    blankModel = function() {
      return {
        q: '',
        allVocabs: [],
        filteredVocabs: [],
        selectedVocab: {},
        loadedVocab: {},
        foundKeywords: [],
        selectedKeywords: [],
        newUncontrolledKeyword: ''
      };
    };
    if (!$scope.vocabulator) {
      $scope.vocabulator = {};
    }
    if (angular.equals({}, $scope.vocabulator)) {
      angular.extend($scope.vocabulator, blankModel());
    }
    m = $scope.vocabulator;
    $scope.colourHasher = colourHasher;
    if (!m.allVocabs.length) {
      $http.get('../api/vocabularylist').success(function(result) {
        m.allVocabs = result;
        return m.filteredVocabs = result;
      });
    }
    getSelectedVocabfulKeywords = function() {
      return _.filter(m.selectedKeywords, function(k) {
        return k.vocab !== '';
      });
    };
    $scope.doFind = function(q, older) {
      var clearCurrentVocab, findKeywords, findVocabs, suggestKeywordsFromSearchString, updateSelectedKeywords;
      suggestKeywordsFromSearchString = function(s) {
        return _(m.q.split(/[,;]+/)).map(function(s) {
          return {
            vocab: '',
            value: s.trim()
          };
        }).filter(function(k) {
          return k.value !== '';
        }).value();
      };
      updateSelectedKeywords = function() {
        if (!_.some(getSelectedVocabfulKeywords())) {
          if (q === '' && older !== '') {
            return m.selectedKeywords = [];
          } else if (q !== '') {
            return m.selectedKeywords = suggestKeywordsFromSearchString(q);
          }
        }
      };
      clearCurrentVocab = function() {
        m.loadedVocab = {};
        return m.selectedVocab = {};
      };
      findVocabs = function() {
        var filtered, v;
        q = m.q.toLowerCase();
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
      };
      findKeywords = function() {
        if (m.q === '') {
          return m.foundKeywords = [];
        } else {
          return $http.get('../api/keywords?q=' + m.q).success(function(result) {
            return m.foundKeywords = result;
          }).error(function(e) {
            return $scope.notifications.add('Oops! ' + e.message);
          });
        }
      };
      updateSelectedKeywords();
      if (q !== older) {
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
    $scope.$watch('vocabulator.newUncontrolledKeyword', function(keyword, old) {
      if (keyword !== '' && keyword !== old) {
        if (!_.some(getSelectedVocabfulKeywords())) {
          return m.selectedKeywords.length = 0;
        }
      }
    });
    $scope.selectKeyword = function(k) {
      _.remove(m.selectedKeywords, function(k) {
        return k.vocab === '';
      });
      if (__indexOf.call(m.selectedKeywords, k) < 0) {
        return m.selectedKeywords.push(k);
      }
    };
    $scope.unselectKeyword = function(k) {
      return m.selectedKeywords.splice(m.selectedKeywords.indexOf(k), 1);
    };
    return $scope.close = function() {
      var selectedKeywords;
      selectedKeywords = m.selectedKeywords;
      angular.extend($scope.vocabulator, blankModel());
      return $scope.$close(selectedKeywords);
    };
  });

}).call(this);

//# sourceMappingURL=VocabulatorController.js.map
