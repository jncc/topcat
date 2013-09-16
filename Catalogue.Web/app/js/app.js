﻿(function() {
  var module;

  module = angular.module('app', ['ngRoute', 'app.utilities', 'app.directives', 'app.services', 'app.controllers']);

  angular.module('app.utilities', []);

  angular.module('app.directives', []);

  angular.module('app.services', ['ngResource']);

  angular.module('app.controllers', []);

  module.config([
    '$routeProvider', function($routeProvider) {
      return $routeProvider.when('/', {
        controller: 'SearchController',
        templateUrl: 'views/search/search.html'
      }).when('/editor/:recordId', {
        controller: 'EditorController',
        templateUrl: 'views/editor/editor.html',
        resolve: {
          'record': function(RecordLoader) {
            return RecordLoader();
          }
        }
      }).otherwise({
        redirectTo: '/'
      });
    }
  ]);

}).call(this);
