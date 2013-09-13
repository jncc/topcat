(function() {
  var module;

  module = angular.module('app', ['ngRoute', 'app.directives', 'app.services', 'app.controllers']);

  angular.module('app.directives', []);

  angular.module('app.services', ['ngResource']);

  angular.module('app.controllers', []);

  module.config([
    '$routeProvider', function($routeProvider) {
      return $routeProvider.when('/', {
        controller: 'SearchController',
        templateUrl: 'views/search/search.html'
      }).when('/editor', {
        controller: 'EditorController',
        templateUrl: 'views/editor/editor.html'
      }).otherwise({
        redirectTo: '/'
      });
    }
  ]);

}).call(this);
