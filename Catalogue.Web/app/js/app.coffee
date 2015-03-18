module = angular.module 'app', [
    'ngAnimate',
    'ngRoute',
    'ngSanitize',
    'ngCookies',
    'app.utilities',
    'app.directives',
    'app.services',
    'app.controllers',
    'filters',
    'ui.grid',
    'ui.grid.resizeColumns'
    ]
        
angular.module 'app.utilities', []
angular.module 'app.directives', []
angular.module 'app.services', ['ngResource']
angular.module 'app.controllers', ['ngCookies']
#angular.module 'app.components', []


module.config ($routeProvider) ->
    $routeProvider
        .when '/',
            controller:     'SearchController',
            templateUrl:    'views/search/search.html',
            reloadOnSearch: false
        .when '/editor/:recordId',
            controller:     'EditorController',
            templateUrl:    'views/editor/editor.html',
            resolve:        'record': (RecordLoader) -> RecordLoader()
        .when '/sandbox/colours',
            controller:     'SandboxController',
            templateUrl:    'views/sandbox/colours.html'
        .when '/sandbox/glyphs',
            controller:     'SandboxController',
            templateUrl:    'views/sandbox/glyphs.html'
        .when '/sandbox/dropdown',
            controller:     'SandboxController',
            templateUrl:    'views/sandbox/dropdown.html'
        .when '/vocabularies',
            controller:     'VocabularyListController',
            templateUrl:    'views/vocabularies/vocabularies.html'
        .when '/ingest',
            controller:     'IngestController',
            templateUrl:    'views/ingest/ingest.html'
        .when '/vocabularies/editor/:vocabId*',
            controller:     'VocabularyEditorController',
            templateUrl:    'views/vocabularies/editor.html',
            resolve:        'vocab': (VocabLoader) -> VocabLoader()
        .otherwise
            redirectTo:     '/'

# todo erm, move or remove this
angular.module('filters', []).filter('camelCaseFilter', () -> 
    (input) -> 
        input.charAt(0).toUpperCase() + input.substr(1).replace(/[A-Z]/g, ' $&')
)

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
