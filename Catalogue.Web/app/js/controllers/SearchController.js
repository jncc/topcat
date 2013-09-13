(function() {

  angular.module('app.controllers').controller('SearchController', function($scope) {
    $scope.foo = {
      bar: 'hello'
    };
    return $scope.doSearch = function() {};
  });

}).call(this);
