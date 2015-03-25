(function() {
  var module;

  module = angular.module('app.services');

  module.factory('Account', function($http, $q) {
    var d;
    d = $q.defer();
    $http.get('../api/account').success(function(data) {
      return d.resolve(data);
    });
    return d.promise;
  });

  module.factory('Vocabulary', function($resource) {
    return $resource('../api/vocabularies/?id=:id', {}, {
      query: {
        method: 'GET',
        params: {
          id: '@id'
        }
      },
      update: {
        method: 'PUT',
        params: {
          id: '@id'
        }
      }
    });
  });

  module.factory('VocabLoader', function(Vocabulary, $route, $q) {
    return function() {
      var d;
      d = $q.defer();
      Vocabulary.get({
        id: $route.current.params.vocabId
      }, function(vocabulary) {
        return d.resolve(vocabulary);
      }, function() {
        return d.reject('Unable to fetch vocabulary ' + $route.current.params.vocabId);
      });
      return d.promise;
    };
  });

  module.factory('Record', function($resource) {
    return $resource('../api/records/:id', {}, {
      query: {
        method: 'GET',
        params: {
          id: '@id'
        }
      },
      update: {
        method: 'PUT',
        params: {
          id: '@id'
        }
      },
      clone: {
        method: 'GET',
        params: {
          id: '@id',
          clone: true
        }
      }
    });
  });

  module.factory('RecordLoader', function(Record, $route, $q) {
    return function() {
      var d;
      d = $q.defer();
      Record.get({
        id: $route.current.params.recordId
      }, function(record) {
        return d.resolve(record);
      }, function() {
        return d.reject('Unable to fetch record ' + $route.current.params.recordId);
      });
      return d.promise;
    };
  });

  module.factory('RecordCloner', function(Record, $route, $q) {
    return function() {
      var d;
      d = $q.defer();
      Record.clone({
        id: $route.current.params.recordId
      }, function(record) {
        return d.resolve(record);
      }, function() {
        return d.reject('Unable to fetch a cloan of record ' + $route.current.params.recordId);
      });
      return d.promise;
    };
  });

  module.factory('Formats', function($http, $q) {
    var d;
    d = $q.defer();
    $http.get('../api/formats').success(function(data) {
      return d.resolve(data);
    });
    return d.promise;
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

//# sourceMappingURL=services.js.map
