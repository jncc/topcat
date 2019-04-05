angular.module('app.controllers',['ui.bootstrap']).controller 'MainController',

    ($scope, $rootScope, $timeout, $cookies, $http, Account) ->

        # page title
        $scope.page = title: 'Topcat'
        
        # implement busy spinner feature
        busyCount = 0
        $scope.busy =
            start: -> busyCount = busyCount + 1
            stop : -> busyCount = busyCount - 1
            value: -> busyCount > 0

        # notifications (success, info and error messages)
        notifications = []
        $scope.notifications =
            current: notifications
            add: (message) ->
                n = message: message
                notifications.push n
                remove = -> notifications.pop()
                $timeout remove, 4000

        # every page needs a user
        Account.then (user) -> $scope.user = user

        # application-wide "status" info for current user
        $scope.status = 
            pendingSignOffCount: 0
            refresh: ->
                $http.get('../api/publishing/pendingsignoffcountforcurrentuser').success (result) ->
                    $scope.status.pendingSignOffCount = result
        $scope.status.refresh()

        # horrid hack to ensure qtips hide when url (location) changes
        # (tooltips left hanging visible when the element has gone)
        $rootScope.$on '$locationChangeStart', -> $('.qtip').qtip 'hide'       
