(function() {

  angular.module('app.controllers').controller('VocabulariesController', function($scope, $http) {
    return $http.get('../api/vocabulariestypeahead?q=all').success(function(data) {
      return $scope.vocabularies = data;
    });
  });

}).call(this);
