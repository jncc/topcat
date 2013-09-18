(function() {

  angular.module('app.controllers').controller('SearchController', function($scope, $location) {
    alert('hi');
    $scope.query = {
      q: ''
    };
    $scope.$watch('query.q', function(q) {
      return $location.search('q', q);
    });
    return $scope.$watch(function() {
      return $location.search()['q'];
    }, function() {
      return $scope.query.q = $location.search()['q'] || '';
    });
  });

}).call(this);
