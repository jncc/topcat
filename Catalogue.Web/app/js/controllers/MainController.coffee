
angular.module('app.controllers').controller 'MainController', 

    ($scope, $timeout, Account, misc) -> 

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

        # utility function to be moved
        $scope.hashStringToColor = misc.hashStringToColor
        