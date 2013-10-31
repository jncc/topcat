
angular.module('app.controllers').controller 'MainController', 

    ($scope, Account) -> 
        Account.then (user) -> $scope.user = user
