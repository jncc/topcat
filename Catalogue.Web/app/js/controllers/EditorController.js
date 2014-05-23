(function() {
  var fakeValidationData, getSecurityText;

  angular.module('app.controllers').controller('EditorController', function($scope, $http, $routeParams, $location, record, Record) {
    $scope.lookups = {};
    $http.get('../api/topics').success(function(result) {
      return $scope.lookups.topics = result;
    });
    $scope.getSecurityText = getSecurityText;
    $scope.cancel = function() {
      $scope.reset();
      return $scope.notifications.add('Edits cancelled');
    };
    $scope.save = function() {
      var processResult;
      processResult = function(response) {
        var e, errors, field, _i, _j, _len, _len1, _ref;
        if (response.data.success) {
          record = response.data.record;
          $scope.validation = {};
          $scope.reset();
          $scope.notifications.add('Edits saved');
          $location.path('/editor/' + record.id);
        } else {
          $scope.validation = response.data.validation;
          errors = response.data.validation.errors;
          if (errors.length > 0) {
            $scope.notifications.add('There were errors');
          }
          for (_i = 0, _len = errors.length; _i < _len; _i++) {
            e = errors[_i];
            _ref = e.fields;
            for (_j = 0, _len1 = _ref.length; _j < _len1; _j++) {
              field = _ref[_j];
              $scope.theForm[field].$setValidity('server', false);
            }
          }
        }
        return $scope.busy.stop();
      };
      $scope.busy.start();
      if ($routeParams.recordId !== '00000000-0000-0000-0000-000000000000') {
        return $http.put('../api/records/' + record.id, $scope.form).then(processResult);
      } else {
        return $http.post('../api/records', $scope.form).then(processResult);
      }
    };
    $scope.reset = function() {
      return $scope.form = angular.copy(record);
    };
    $scope.isClean = function() {
      return angular.equals($scope.form, record);
    };
    $scope.isSaveHidden = function() {
      return $scope.isClean() || record.readOnly;
    };
    $scope.isCancelHidden = function() {
      return $scope.isClean();
    };
    $scope.isSaveDisabled = function() {
      return $scope.isClean();
    };
    $scope.removeKeyword = function(keyword) {
      return $scope.form.gemini.keywords.splice($.inArray(keyword, $scope.form.gemini.keywords), 1);
    };
    $scope.addKeyword = function() {
      return $scope.form.gemini.keywords.push({
        vocab: '',
        value: ''
      });
    };
    $scope.reset();
  });

  getSecurityText = function(n) {
    switch (n) {
      case 0:
        return 'Open';
      case 1:
        return 'Restricted';
      case 2:
        return 'Classified';
    }
  };

  fakeValidationData = {
    errors: [
      {
        message: 'There was an error'
      }, {
        message: 'There was another error'
      }
    ]
  };

}).call(this);
