(function() {
  var fakeValidationData, getSecurityText;

  angular.module('app.controllers').controller('EditorController', function($scope, $http, record, Record) {
    $scope.lookups = {};
    $http.get('../api/topics').success(function(result) {
      return $scope.lookups.topics = result;
    });
    $scope.getSecurityText = getSecurityText;
    $scope.reset = function() {
      return $scope.form = angular.copy(record);
    };
    $scope.save = function() {
      $scope.busy.start();
      $scope.validation = {};
      return $http.put('../api/records/' + record.id, $scope.form).then(function(response) {
        var e, errors, field, _i, _j, _len, _len1, _ref;
        if (response.data.success) {
          record = response.data.record;
          $scope.reset();
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
      });
    };
    $scope.isClean = function() {
      return angular.equals($scope.form, record);
    };
    $scope.isSaveHidden = function() {
      return $scope.isClean() || record.readOnly;
    };
    $scope.isResetHidden = function() {
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
    $scope.validation = fakeValidationData;
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
