(function() {
  angular.module('app.controllers').controller('VocabulatorController', function($scope, $http, colourHasher) {
    var clearCurrentVocab, findKeywords, findVocabs, loadVocab;
    if (!$scope.vocabulator) {
      $scope.vocabulator = {
        vocabs: {},
        vocab: {},
        find: {},
        found: {},
        selected: {}
      };
    }
    $scope.colourHasher = colourHasher;
    clearCurrentVocab = function() {
      $scope.vocabulator.vocab = {};
      return $scope.vocabulator.selected.vocab = {};
    };
    $http.get('../api/vocabularylist').success(function(result) {
      $scope.vocabulator.vocabs.all = result;
      return $scope.vocabulator.vocabs.filtered = result;
    });
    findVocabs = function() {
      var filtered, q, v;
      if ($scope.vocabulator.vocabs.all) {
        q = $scope.vocabulator.find.text.toLowerCase();
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
      if ($scope.vocabulator.find.text) {
        return $http.get('../api/keywords?q=' + $scope.vocabulator.find.text).success(function(result) {
          return $scope.vocabulator.found.keywords = result;
        }).error(function(e) {
          return $scope.notifications.add('Oops! ' + e.message);
        });
      } else {
        return $scope.vocabulator.found.keywords = [];
      }
    };
    $scope.doFind = function() {
      $scope.vocabulator.selected.keyword = {
        vocab: '',
        value: $scope.vocabulator.find.text
      };
      clearCurrentVocab();
      findVocabs();
      return findKeywords();
    };
    $scope.$watch('vocabulator.find.text', $scope.doFind, true);
    loadVocab = function(vocab) {
      if (vocab) {
        return $http.get('../api/vocabularies?id=' + encodeURIComponent(vocab.id)).success(function(result) {
          return $scope.vocabulator.vocab = result;
        }).error(function(e) {
          return $scope.notifications.add('Oops! ' + e.message);
        });
      }
    };
    $scope.$watch('vocabulator.selected.vocab', loadVocab);
    $scope.selectKeyword = function(k) {
      return $scope.vocabulator.selected.keyword = k;
    };
    return $scope.close = function() {
      return $scope.$close($scope.vocabulator.selected.keyword);
    };
  });

}).call(this);

//# sourceMappingURL=VocabulatorController.js.map
