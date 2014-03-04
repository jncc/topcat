(function() {
  var fakeValidationData;

  angular.module('app.controllers').controller('EditorController', function($scope, $http, record, Record) {
    $scope.lookups = {};
    $http.get('../api/topics').success(function(result) {
      return $scope.lookups.topics = result;
    });
    $scope.state = /^\w\w$/;
    $scope.zip = /^\d\d\d\d\d$/;
    $scope.getSecurityText = function(n) {
      switch (n) {
        case 0:
          return 'Open';
        case 1:
          return 'Restricted';
        case 2:
          return 'Classified';
      }
    };
    $scope.reset = function() {
      return $scope.form = angular.copy(record);
    };
    $scope.save = function() {
      $scope.busy.start();
      $scope.validation = {};
      return $http.put('../api/records/' + record.id, $scope.form).then(function(response) {
        var e, field, _i, _j, _len, _len1, _ref, _ref1;
        if (response.data.success) {
          record = response.data.record;
          $scope.reset();
        } else {
          $scope.validation = response.data.validation;
          _ref = response.data.validation.errors;
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            e = _ref[_i];
            _ref1 = e.fields;
            for (_j = 0, _len1 = _ref1.length; _j < _len1; _j++) {
              field = _ref1[_j];
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
      var i;
      i = $scope.form.gemini.keywords.indexOf(keyword);
      return $scope.form.gemini.keywords.splice(i, 1);
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
