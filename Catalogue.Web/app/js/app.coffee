module = angular.module 'app', [
    'ngAnimate',
    'ngRoute',
    'ngSanitize',
    'ngCookies',
    'ui.bootstrap',
    'app.map',
    'app.utilities',
    'app.directives',
    'app.services',
    'app.filters',
    'app.controllers',
    'ui.grid',
    'ui.grid.resizeColumns',
    'angularMoment'
    ]
        
angular.module 'app.map', []
angular.module 'app.utilities', []
angular.module 'app.directives', []
angular.module 'app.services', ['ngResource']
angular.module 'app.filters', []
angular.module 'app.controllers', ['ngCookies']

module.config ($routeProvider) ->
    $routeProvider
        .when '/',
            controller:     'SearchController',
            templateUrl:    'views/search/search.html',
            reloadOnSearch: false
        .when '/clone/:recordId',
            controller:     'EditorController',
            templateUrl:    'views/editor/editor.html',
            resolve:        'record': (RecordCloner) -> RecordCloner()
        .when '/editor/:recordId',
            controller:     'EditorController',
            templateUrl:    'views/editor/editor.html',
            resolve:        'record': (RecordLoader) -> RecordLoader()
        .when '/sandbox/colours',
            controller:     'SandboxController',
            templateUrl:    'views/sandbox/colours.html'
        .when '/vocabularies',
            controller:     'VocabularyListController',
            templateUrl:    'views/vocabularies/vocabularies.html'
        .when '/publishing',
            controller:     'PublishingController',
            templateUrl:    'views/publishing/publishing.html'
        .when '/vocabularies/editor/:vocabId*',
            controller:     'VocabularyEditorController',
            templateUrl:    'views/vocabularies/editor.html',
            resolve:        'vocab': (VocabLoader) -> VocabLoader()
        .when '/sandbox/glyphs',
            controller:     'SandboxController',
            templateUrl:    'views/sandbox/glyphs.html'
        .when '/sandbox/dropdown',
            controller:     'SandboxController',
            templateUrl:    'views/sandbox/dropdown.html'
        .when '/sandbox/tooltip',
            controller:     'SandboxController',
            templateUrl:    'views/sandbox/tooltip.html'
        .when '/sandbox/lookups',
            controller:     'SandboxController',
            templateUrl:    'views/sandbox/lookups.html'
        .when '/sandbox/vocabulator', # for developing the vocabulator modal more easily
            controller:     'VocabulatorController',
            templateUrl:    'views/partials/vocabulator.html'
        .when '/sandbox/markdown', # for developing the markdown modal more easily
            controller:     'MarkdownController',
            templateUrl:    'views/partials/markdown.html'
            resolve:        'markdown' : () -> '#test text'
        .when '/content/whytopcat',
            controller:     'ContentController',
            templateUrl:    'views/partials/why-topcat.html'
        .when '/content/whatisdata',
            controller:     'ContentController',
            templateUrl:    'views/partials/what-is-data.html'
        .when '/content/present',
            controller:     'ContentController',
            templateUrl:    'views/partials/presentation.html'
        .otherwise
            redirectTo:     '/'

# todo: move and commment
moment.updateLocale('en', {
    relativeTime : {
        past:   "%s",
        s  : 'a few seconds ago',
        ss : '%d seconds ago',
        m:  "a minute ago",
        mm: "%d minutes ago",
        h:  "an hour ago",
        hh: "%d hours ago",
        d:  "yesterday",
        dd: (number) ->
            if number < 7
                "this past week"
            else
                "this month"
        M:  "last month",
        MM: "this year",
        y:  "last year",
        yy: "%d years ago"
    }
});

# just playing....
module.animation '.my-special-animation', ->
  enter: (element, done) ->
    $(element).slideDown 500
    , done
    (cancelled) ->
      # this (optional) function is called when the animation is complete or cancelled
      $(element).stop() if cancelled

  leave: (element, done) ->
    done()

  move: (element, done) ->
    done()

  addClass: (element, className, done) ->
    done()

  removeClass: (element, className, done) ->
    done()

