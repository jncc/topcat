(function() {
  angular.module('app.controllers').controller('VocabulatorController', function($scope, $http, colourHasher) {
    var clearCurrentVocab, findKeywords, findVocabs, loadVocab;
    $scope.vocabs = {};
    $scope.vocab = {};
    $scope.find = {};
    $scope.found = {};
    $scope.selected = {};
    $scope.colourHasher = colourHasher;
    clearCurrentVocab = function() {
      $scope.vocab = {};
      return $scope.selected.vocab = {};
    };
    $http.get('../api/vocabularylist').success(function(result) {
      $scope.vocabs.all = result;
      return $scope.vocabs.filtered = result;
    });
    findVocabs = function() {
      var filtered, q, v;
      if ($scope.vocabs.all) {
        q = $scope.find.text.toLowerCase();
        filtered = (function() {
          var _i, _len, _ref, _results;
          _ref = $scope.vocabs.all;
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            v = _ref[_i];
            if (v.name.toLowerCase().indexOf(q) !== -1) {
              _results.push(v);
            }
          }
          return _results;
        })();
        return $scope.vocabs.filtered = filtered;
      }
    };
    findKeywords = function() {
      if ($scope.find.text) {
        return $http.get('../api/keywords?q=' + $scope.find.text).success(function(result) {
          return $scope.found.keywords = result;
        }).error(function(e) {
          return $scope.notifications.add('Oops! ' + e.message);
        });
      } else {
        return $scope.found.keywords = [];
      }
    };
    $scope.doFind = function() {
      $scope.selected.keyword = {
        vocab: '',
        value: $scope.find.text
      };
      clearCurrentVocab();
      findVocabs();
      return findKeywords();
    };
    $scope.$watch('find.text', $scope.doFind, true);
    loadVocab = function(vocab) {
      if (vocab) {
        return $http.get('../api/vocabularies?id=' + encodeURIComponent(vocab.id)).success(function(result) {
          return $scope.vocab = result;
        }).error(function(e) {
          return $scope.notifications.add('Oops! ' + e.message);
        });
      }
    };
    $scope.$watch('selected.vocab', loadVocab);
    $scope.selectKeyword = function(k) {
      return $scope.selected.keyword = k;
    };
    return $scope.close = function() {
      return $scope.$close($scope.selected.keyword);
    };
  });

}).call(this);

//# sourceMappingURL=VocabulatorController.js.map
