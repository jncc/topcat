(function() {

  angular.module('app.controllers').controller('MarkdownController', function($scope) {
    $scope.text = {
      value: ''
    };
    return $scope.markdown = function(s) {
      var showdown;
      showdown = new Showdown.converter();
      return showdown.makeHtml(s);
    };
  });

}).call(this);
