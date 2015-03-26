(function() {
  angular.module('app.controllers').controller('VocabularyListController', function($scope, $http) {
    $http.get('../api/VocabularyList').success(function(data) {
      return $scope.vocabularies = data;
    });
    return $scope.encodeId = function(id) {
      return encodeURIComponent(id);
    };
  });

}).call(this);

//# sourceMappingURL=VocabularyListController.js.map
