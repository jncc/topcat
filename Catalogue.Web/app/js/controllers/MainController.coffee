
angular.module('app.controllers').controller 'MainController', 

    ($scope, $timeout, Account, misc) -> 

        $scope.hashStringToColor = misc.hashStringToColor
        
        busyCount = 0
        $scope.busy =
            start: -> busyCount = busyCount + 1
            stop : -> busyCount = busyCount - 1
            value: -> busyCount > 0

        $scope.notifications = [] # todo ....

        Account.then (user) -> $scope.user = user
