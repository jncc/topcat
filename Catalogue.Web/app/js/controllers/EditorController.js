(function() {

  angular.module('app.controllers').controller('EditorController', function($scope, defaults, $http, record) {
    var pristine;
    $scope.lookups = {};
    $http.get('../api/topics').success(function(result) {
      return $scope.lookups.topics = result;
    });
    $scope.state = /^\w\w$/;
    $scope.zip = /^\d\d\d\d\d$/;
    pristine = record;
    $scope.reset = function() {
      return $scope.record = angular.copy(pristine);
    };
    $scope.save = function() {
      pristine = $scope.record;
      return $scope.reset();
    };
    $scope.isResetDisabled = function() {
      return angular.equals(pristine, $scope.record);
    };
    $scope.isSaveDisabled = function() {
      return angular.equals(pristine, $scope.form);
    };
    $scope.reset();
  });

}).call(this);
