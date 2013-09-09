(function() {
  var module;

  module = angular.module('editor', []);

  module.factory('defaults', function() {
    return {
      name: 'John Smit',
      line1: '123 Main St.',
      city: 'Anytown',
      state: 'AA',
      zip: '12345',
      phone: '1(234) 555-1212'
    };
  });

  module.directive('placeholder', function() {
    return {
      restrict: 'A',
      link: function(scope, element, attrs) {
        return $(element).placeholder();
      }
    };
  });

  module.directive('autosize', function() {
    return {
      restrict: 'A',
      link: function(scope, element, attrs) {
        return $(element).autosize();
      }
    };
  });

  module.directive('selectpicker', function() {
    return {
      restrict: 'A',
      link: function(scope, element, attrs) {
        return $(element).selectpicker();
      }
    };
  });

}).call(this);
