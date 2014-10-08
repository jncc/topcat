(function() {

  angular.module('app.controllers').controller('VocabulariesController', function($scope, $http) {
    return $http.get('../api/vocabularies?q=all').success(function(data) {
      return $scope.vocabularies = data;
    });
  });

}).call(this);
