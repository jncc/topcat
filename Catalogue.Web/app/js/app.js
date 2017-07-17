﻿// Generated by IcedCoffeeScript 108.0.11
(function() {
  var module;

  module = angular.module('app', ['ngAnimate', 'ngRoute', 'ngSanitize', 'ngCookies', 'ui.bootstrap', 'app.map', 'app.utilities', 'app.directives', 'app.services', 'app.filters', 'app.controllers', 'ui.grid', 'ui.grid.resizeColumns', 'angularMoment']);

  angular.module('app.map', []);

  angular.module('app.utilities', []);

  angular.module('app.directives', []);

  angular.module('app.services', ['ngResource']);

  angular.module('app.filters', []);

  angular.module('app.controllers', ['ngCookies']);

  module.config(function($routeProvider) {
    return $routeProvider.when('/', {
      controller: 'SearchController',
      templateUrl: 'views/search/search.html',
      reloadOnSearch: false
    }).when('/clone/:recordId', {
      controller: 'EditorController',
      templateUrl: 'views/editor/editor.html',
      resolve: {
        'record': function(RecordCloner) {
          return RecordCloner();
        }
      }
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
    }).when('/vocabularies', {
      controller: 'VocabularyListController',
      templateUrl: 'views/vocabularies/vocabularies.html'
    }).when('/publishing', {
      controller: 'PublishingController',
      templateUrl: 'views/publishing/publishing.html'
    }).when('/vocabularies/editor/:vocabId*', {
      controller: 'VocabularyEditorController',
      templateUrl: 'views/vocabularies/editor.html',
      resolve: {
        'vocab': function(VocabLoader) {
          return VocabLoader();
        }
      }
    }).when('/sandbox/glyphs', {
      controller: 'SandboxController',
      templateUrl: 'views/sandbox/glyphs.html'
    }).when('/sandbox/dropdown', {
      controller: 'SandboxController',
      templateUrl: 'views/sandbox/dropdown.html'
    }).when('/sandbox/tooltip', {
      controller: 'SandboxController',
      templateUrl: 'views/sandbox/tooltip.html'
    }).when('/sandbox/lookups', {
      controller: 'SandboxController',
      templateUrl: 'views/sandbox/lookups.html'
    }).when('/sandbox/vocabulator', {
      controller: 'VocabulatorController',
      templateUrl: 'views/partials/vocabulator.html'
    }).when('/sandbox/markdown', {
      controller: 'MarkdownController',
      templateUrl: 'views/partials/markdown.html',
      resolve: {
        'markdown': function() {
          return '#test text';
        }
      }
    }).when('/content/whytopcat', {
      controller: 'ContentController',
      templateUrl: 'views/partials/why-topcat.html'
    }).when('/content/whatisdata', {
      controller: 'ContentController',
      templateUrl: 'views/partials/what-is-data.html'
    }).when('/content/present', {
      controller: 'ContentController',
      templateUrl: 'views/partials/presentation.html'
    }).otherwise({
      redirectTo: '/'
    });
  });

  moment.updateLocale('en', {
    relativeTime: {
      past: "%s",
      s: 'a few seconds ago',
      ss: '%d seconds ago',
      m: "a minute ago",
      mm: "%d minutes ago",
      h: "an hour ago",
      hh: "%d hours ago",
      d: "yesterday",
      dd: function(number) {
        if (number < 7) {
          return "this past week";
        } else {
          return "this month";
        }
      },
      M: "last month",
      MM: "this year",
      y: "last year",
      yy: "%d years ago"
    }
  });

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

//# sourceMappingURL=app.js.map
