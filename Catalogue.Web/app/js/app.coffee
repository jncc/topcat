

module = angular.module 'app', [
    'ngAnimate',
    'ngRoute',
    'ngSanitize',
    'app.utilities',
    'app.directives',
    'app.services',
    'app.controllers',
    ]
        
angular.module 'app.utilities', []
angular.module 'app.directives', ['ui.bootstrap']
angular.module 'app.services', ['ngResource']
angular.module 'app.controllers', []

module.config ['$routeProvider', ($routeProvider) ->
    $routeProvider
        .when '/'
            controller:     'SearchController',
            templateUrl:    'views/search/search.html',
            reloadOnSearch: false,
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
        .when '/admin/minty1$',
            controller:     'AdminController',
            templateUrl:    'views/admin/admin.html'
        .otherwise
            redirectTo:     '/'
    ]

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
