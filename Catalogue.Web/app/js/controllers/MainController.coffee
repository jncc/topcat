
angular.module('app.controllers').controller 'MainController', 

    ($scope, $timeout, Account) -> 

        # hacky way of delaying things (animations) on startup
        # to work around angular skipping the initial animation
        $scope.app = { starting: true };
        $timeout(
            -> $scope.app.starting = false,
            1000)

        Account.then (user) -> $scope.user = user
