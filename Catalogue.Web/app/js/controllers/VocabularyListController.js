(function() {

  angular.module('app.controllers').controller('VocabularyListController', function($scope, $http) {
    return $http.get('../api/VocabularyList').success(function(data) {
      return $scope.vocabularies = data;
    });
  });

}).call(this);
