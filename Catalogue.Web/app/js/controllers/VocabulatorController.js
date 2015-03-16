(function() {
  angular.module('app.controllers').controller('VocabulatorController', function($scope, $http) {
    $scope.keyword = {
      vocab: 'http://vocab.jncc.gov.uk/original-seabed-classification-system',
      value: 'MNCR'
    };
    return $scope.close = function() {
      return $scope.$close($scope.keyword);
    };
  });

}).call(this);

//# sourceMappingURL=VocabulatorController.js.map
