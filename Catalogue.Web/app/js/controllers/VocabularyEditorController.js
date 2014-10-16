(function() {

  angular.module('app.controllers').controller('VocabularyEditorController', function($scope, $http, $routeParams, $location, vocab) {
    $scope.reset = function() {
      $scope.form = angular.copy(vocab);
      if (vocab.id) {
        return $scope.newVocab = false;
      } else {
        return $scope.newVocab = true;
      }
    };
    $scope.removeKeyword = function(index) {
      return $scope.form.values.splice(index, 1);
    };
    $scope.addKeyword = function() {
      return $scope.form.values.push('');
    };
    $scope.isClean = function() {
      return angular.equals($scope.form, vocab);
    };
    $scope.isSaveHidden = function() {
      return $scope.isClean();
    };
    $scope.isCancelHidden = function() {
      return $scope.isClean();
    };
    return $scope.reset();
  });

}).call(this);
