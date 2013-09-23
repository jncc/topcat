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

  module.factory('RecordLoader', function(Record, $route, $q) {
    return function() {
      var d;
      d = $q.defer();
      Record.get({
        id: $route.current.params.recordId
      }, function(record) {
        return d.resolve(record, function() {
          return d.reject('Unable to fetch record ' + $route.current.params.recordId);
        });
      });
      return d.promise;
    };
  });

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
