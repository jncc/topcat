(function() {
  var module;

  module = angular.module('app', ['ngAnimate', 'ngRoute', 'ngSanitize', 'app.utilities', 'app.directives', 'app.services', 'app.controllers', 'filters']);

  angular.module('app.utilities', []);

  angular.module('app.directives', []);

  angular.module('app.services', ['ngResource']);

  angular.module('app.controllers', []);

  angular.module('filters', []).filter('camelCaseFilter', function() {
    return function(input) {
      return input.charAt(0).toUpperCase() + input.substr(1).replace(/[A-Z]/g, ' $&');
    };
  });

  module.config([
    '$routeProvider', function($routeProvider) {
      return $routeProvider.when('/', {
        controller: 'SearchController',
        templateUrl: 'views/search/search.html',
        reloadOnSearch: false
      }).when('/editor/:recordId', {
        controller: 'EditorController',
        templateUrl: 'views/editor/editor.html',
        resolve: {
          'record': function(RecordLoader) {
            return RecordLoader();
          }
        }
      }).when('/sandbox/colours', {
        controller: 'SandboxController',
        templateUrl: 'views/sandbox/colours.html'
      }).when('/sandbox/glyphs', {
        controller: 'SandboxController',
        templateUrl: 'views/sandbox/glyphs.html'
      }).when('/sandbox/dropdown', {
        controller: 'SandboxController',
        templateUrl: 'views/sandbox/dropdown.html'
      }).when('/vocabularies', {
        controller: 'VocabularyListController',
        templateUrl: 'views/vocabularies/vocabularies.html'
      }).when('/admin/minty1$', {
        controller: 'AdminController',
        templateUrl: 'views/admin/admin.html'
      }).when('/vocabularies/editor/:vocabId*', {
        controller: 'VocabularyEditorController',
        templateUrl: 'views/vocabularies/editor.html',
        resolve: {
          'vocab': function(VocabLoader) {
            return VocabLoader();
          }
        }
      }).otherwise({
        redirectTo: '/'
      });
    }
  ]);

  module.animation('.my-special-animation', function() {
    return {
      enter: function(element, done) {
        $(element).slideDown(500, done);
        return function(cancelled) {
          if (cancelled) {
            return $(element).stop();
          }
        };
      },
      leave: function(element, done) {
        return done();
      },
      move: function(element, done) {
        return done();
      },
      addClass: function(element, className, done) {
        return done();
      },
      removeClass: function(element, className, done) {
        return done();
      }
    };
  });

}).call(this);
