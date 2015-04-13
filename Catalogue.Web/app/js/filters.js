(function() {
  var module;

  module = angular.module('app.filters');

  module.filter('camelCaseFilter', function() {
    return function(input) {
      return input.charAt(0).toUpperCase() + input.substr(1).replace(/[A-Z]/g, ' $&');
    };
  });

  module.filter('highlight', function($sce) {
    return function(text, q) {
      var regex;
      regex = new RegExp('(' + q + ')', 'gi');
      if (q) {
        text = text.replace(regex, '<b>$1</b>');
      }
      return $sce.trustAsHtml(text);
    };
  });

}).call(this);

//# sourceMappingURL=filters.js.map
