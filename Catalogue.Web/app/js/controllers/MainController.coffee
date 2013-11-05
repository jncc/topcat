﻿
angular.module('app.controllers').controller 'MainController', 

    ($scope, $timeout, Account) -> 

        # slightly hacky way of triggering animations on startup
        # to work around angular skipping the initial animation
        $scope.app = { starting: true };
        $timeout(
            -> $scope.app.starting = false,
            500)

        Account.then (user) -> $scope.user = user
