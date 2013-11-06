
angular.module('app.controllers').controller 'MainController', 

    ($scope, $timeout, Account) -> 

        Account.then (user) -> $scope.user = user
