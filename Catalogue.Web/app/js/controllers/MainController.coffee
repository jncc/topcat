
angular.module('app.controllers').controller 'MainController', 

    ($scope, $timeout, Account, misc) -> 

        $scope.hashStringToColor = misc.hashStringToColor

        Account.then (user) -> $scope.user = user
