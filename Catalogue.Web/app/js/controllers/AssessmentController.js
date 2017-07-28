﻿// Generated by IcedCoffeeScript 108.0.11
(function() {
  angular.module('app.controllers').controller('AssessmentController', function($scope, $http) {
    $scope.signOffRequest = '{"id": "' + $scope.form.id + '","comment": "Test sign off"}';
    return $scope.close = function() {
      $http.put('../api/publishing/opendata/signoff', $scope.signOffRequest)["catch"](function(error) {
        return $scope.notifications.add('Error updating risk assessment');
      });
      return $scope.$close($scope.assessment);
    };
  });

}).call(this);

//# sourceMappingURL=AssessmentController.js.map
