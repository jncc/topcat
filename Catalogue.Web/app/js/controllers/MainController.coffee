angular.module('app.controllers',['ui.bootstrap']).controller 'MainController',

    ($scope, $rootScope, $timeout, $cookies, Account) ->

        # page title
        $scope.page = title: 'Topcat'
        
        # implement busy spinner feature
        busyCount = 0
        $scope.busy =
            start: -> busyCount = busyCount + 1
            stop : -> busyCount = busyCount - 1
            value: -> busyCount > 0

        # implement notifications feature
        notifications = []
        $scope.notifications =
            current: notifications
            add: (message) ->
                n = message: message
                notifications.push n
                remove = -> notifications.splice ($.inArray n, notifications)
                $timeout remove, 4000

        # every page needs a user
        Account.then (user) -> $scope.user = user
        
        # horrid hack to ensure qtips hide when url (location) changes
        # (tooltips left hanging visible when the element has gone)
        $rootScope.$on '$locationChangeStart', -> $('.qtip').qtip 'hide'
        
