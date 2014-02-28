(function() {

  angular.module('app.controllers').controller('EditorController', function($scope, $rootScope, $http, record, Record) {
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
      $rootScope.busy = {
        value: true
      };
      $scope.errors = {};
      return $http.put('../api/records/' + record.id, $scope.form).then(function(response) {
        if (response.data.success) {
          record = response.data.record;
          $scope.reset();
        } else {
          angular.forEach(response.data.validation.errors, function(error) {
            return angular.forEach(error.fields, function(field) {
              return $scope.theForm[field].$setValidity('server', false);
            });
          });
        }
        return $rootScope.busy = {
          value: false
        };
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
  });

}).call(this);
