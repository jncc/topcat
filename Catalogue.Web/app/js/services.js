(function() {
  var module;

  module = angular.module('app.services');

  module.factory('Record', [
    '$resource', function($resource) {
      return $resource('../api/records/:id', {
        id: '@id'
      });
    }
  ]);

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
