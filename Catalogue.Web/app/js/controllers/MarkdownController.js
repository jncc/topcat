(function() {
  angular.module('app.controllers').controller('MarkdownController', function($scope, markdown) {
    $scope.markdown = markdown;
    $scope.getHtml = function(s) {
      var showdown;
      showdown = new Showdown.converter();
      return showdown.makeHtml(s);
    };
    return $scope.close = function() {
      return $scope.$close($scope.markdown);
    };
  });

}).call(this);

//# sourceMappingURL=MarkdownController.js.map
