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

        # implement notifications feature
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
        
        # horrid hack to ensure qtips hide when url (location) changes
        # (tooltips left hanging visible when the element has gone)
        $rootScope.$on '$locationChangeStart', -> $('.qtip').qtip 'hide'

        # global notification for records pending sign off
        $scope.recordsPendingSignOff = false
        $scope.loadRecordsPendingSignOff = -> $http.get('../api/publishing/opendata/pendingsignoff').success (result) -> $scope.recordsPendingSignOff = result.length > 0

        $scope.loadRecordsPendingSignOff()
        
