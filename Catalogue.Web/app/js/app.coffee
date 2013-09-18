

module = angular.module 'app', [
    'ngRoute',
    'app.utilities',
    'app.directives',
    'app.services',
    'app.controllers',
    ]

angular.module 'app.utilities', []
angular.module 'app.directives', []
angular.module 'app.services', ['ngResource']
angular.module 'app.controllers', []

module.config ['$routeProvider', ($routeProvider) ->
    $routeProvider
        .when '/'
            controller: 'SearchController',
            templateUrl: 'views/search/search.html',
            reloadOnSearch: false,
        .when '/editor/:recordId',
            controller: 'EditorController',
            templateUrl: 'views/editor/editor.html',
            resolve: 'record': (RecordLoader) -> RecordLoader() 
        .otherwise
            redirectTo: '/'
    ]
