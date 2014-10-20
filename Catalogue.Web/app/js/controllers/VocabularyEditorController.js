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
    $scope.save = function() {
      var processResult;
      processResult = function(response) {
        var errors;
        if (response.data.success) {
          vocab = response.data.vocab;
          $scope.validation = {};
          $scope.reset();
          $scope.notifications.add('Edits saved');
          $location.path('/vocabularies/editor/' + vocab.id);
        } else {
          $scope.validation = response.data.validation;
          errors = response.data.validation.errors;
          if (errors.length > 0) {
            $scope.notifications.add('There were errors');
          }
        }
        return $scope.busy.stop();
      };
      $scope.busy.start();
      if ($routeParams.vocabId !== '0') {
        return $http.put('../api/vocabularies?id=' + vocab.id, $scope.form).then(processResult);
      } else {
        return $http.post('../api/vocabularies', $scope.form).then(processResult);
      }
    };
    return $scope.reset();
  });

}).call(this);
