
module = angular.module 'app', [
    'ngRoute',
    'app.directives',
    'app.services' ]

module.config ['$routeProvider', ($routeProvider) ->
    $routeProvider
        .when '/'
            controller: 'MyController',
            templateUrl: 'views/editor/blah.html'
        .when '/editor',
            controller: 'EditorController',
            templateUrl: 'views/editor/editor.html'
        .otherwise
            redirectTo: '/'
    ]

module.controller 'MyController', ($scope) ->
    $scope.foo = { bar : 'hello' }


module.controller 'EditorController', ($scope, defaults, $http) -> 

    $scope.lookups = {}

    $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
          
    master = 
        name: defaults.name,
        address: 
            line1: defaults.line1,
            city: defaults.city,
            state: defaults.state,
            zip: defaults.zip,
        contacts: [
          type: 'phone', value: defaults.phone
        ],
        topic: ''

    $scope.state = /^\w\w$/
    $scope.zip = /^\d\d\d\d\d$/

    $scope.cancel = () -> $scope.form = angular.copy(master)

    $scope.save = () ->
        master = $scope.form
        $scope.cancel()

    $scope.addContact = () -> $scope.form.contacts.push({ type: '', value: '' })

    $scope.removeContact = (contact) ->
        i = $scope.form.contacts.indexOf contact # indexOf doesn't work in ie7!
        $scope.form.contacts.splice i, 1

    $scope.isCancelDisabled = () -> angular.equals(master, $scope.form)

    $scope.isSaveDisabled = () -> $scope.theForm.$invalid || angular.equals(master, $scope.form)

    # call scope.cancel() to initially set up form
    $scope.cancel()
    return


