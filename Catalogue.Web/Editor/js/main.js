(function() {
  var module;

  $(document).ready(function() {
    return $('input, textarea').placeholder();
  });

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

}).call(this);
