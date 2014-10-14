(function() {

  angular.module('app.controllers').controller('VocabularyEditorController', function($scope, $http, $routeParams, $location, vocab) {
    console.log("weee");
    $scope.reset = function() {
      return $scope.form = angular.copy(vocab);
    };
    return $scope.reset();
  });

}).call(this);
