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
      return $scope.form.keywords.splice(index, 1);
    };
    $scope.addKeyword = function() {
      return $scope.form.keywords.push({
        id: '',
        value: ''
      });
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
        var e, errors, field, _i, _j, _len, _len1, _ref;
        if (response.data.success) {
          vocab = response.data.vocab;
          $scope.form = angular.copy(vocab);
          $scope.validation = {};
          $scope.reset();
          $scope.notifications.add('Edits saved');
          $location.path('/vocabularies/editor/' + vocab.id);
        } else {
          $scope.validation = response.data.validation;
          errors = response.data.validation.errors;
          if (errors.length > 0) {
            $scope.notifications.add('There were errors');
            for (_i = 0, _len = errors.length; _i < _len; _i++) {
              e = errors[_i];
              _ref = e.fields;
              for (_j = 0, _len1 = _ref.length; _j < _len1; _j++) {
                field = _ref[_j];
                $scope.theForm[field].$setValidity('server', false);
              }
            }
          }
        }
        return $scope.busy.stop();
      };
      $scope.busy.start();
      if ($scope.newVocab) {
        return $http.post('../api/vocabularies', $scope.form).then(processResult);
      } else {
        return $http.put('../api/vocabularies?id=' + vocab.id, $scope.form).then(processResult);
      }
    };
    return $scope.reset();
  });

}).call(this);
