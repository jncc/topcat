(function() {

  angular.module('app.controllers').controller('EditorController', function($scope, defaults, $http, record) {
    $scope.lookups = {};
    $http.get('../api/topics').success(function(result) {
      return $scope.lookups.topics = result;
    });
    $scope.state = /^\w\w$/;
    $scope.zip = /^\d\d\d\d\d$/;
    $scope.reset = function() {
      return $scope.form = angular.copy(record);
    };
    $scope.save = function() {
      record = $scope.record;
      return $scope.reset();
    };
    $scope.isResetDisabled = function() {
      return angular.equals($scope.form, record);
    };
    $scope.isSaveDisabled = function() {
      return angular.equals($scope.form, record);
    };
    $scope.reset();
  });

}).call(this);
