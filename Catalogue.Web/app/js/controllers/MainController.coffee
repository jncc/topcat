
angular.module('app.controllers').controller 'MainController', 

    ($scope) -> 
        $scope.user = 
            displayName: 'Pete Montgomery'
            email: 'pete.montgomery@jncc.gov.uk'