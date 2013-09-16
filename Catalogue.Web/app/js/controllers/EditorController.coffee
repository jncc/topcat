
angular.module('app.controllers').controller 'EditorController', 

    ($scope, defaults, $http, record) -> 

        $scope.lookups = {}
        $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
        $scope.state = /^\w\w$/
        $scope.zip = /^\d\d\d\d\d$/

          
        $scope.reset = () -> $scope.form = angular.copy(record)

        $scope.save = () ->
            record = $scope.record
            $scope.reset()

        $scope.isResetDisabled = () -> angular.equals($scope.form, record)

        $scope.isSaveDisabled = () -> angular.equals($scope.form, record) # || $scope.theForm.$invalid 

        # call reset() to initially set up form
        $scope.reset()
        return
    

