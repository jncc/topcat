(function() {

  angular.module('app.controllers').controller('MarkdownController', function($scope, foo) {
    console.log(foo);
    $scope.md = {
      text: foo
    };
    $scope.markdown = function(s) {};
    return $scope.close = function() {
      return $scope.$close($scope.md.text);
    };
  });

}).call(this);
