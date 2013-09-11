(function() {
  var module;

  module = angular.module('editor', ['$strap.directives']);

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

  module.directive('servervalidation', function($http) {
    return {
      require: 'ngModel',
      link: function(scope, elem, attrs, ctrl) {
        return elem.on('blur', function(e) {
          return scope.$apply(function() {
            return $http.post('../api/validator', {
              "value": elem.val()
            }).success(function(data) {
              return ctrl.$setValidity('myErrorKey', data.valid);
            });
          });
        });
      }
    };
  });

}).call(this);
