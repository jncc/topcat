angular.module('app.controllers').controller 'SandboxController',

    ($scope, $http, colourHasher) ->  #, Formats
    
        $scope.hashStringToColor = colourHasher.hashStringToColour
        $scope.x = 3;
        
        $scope.lookups =  {}
        $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
        $http.get('../api/formats?q=').success (result) -> $scope.lookups.formats = result
        
        #Formats.then (formats) -> $scope.formats = formats

