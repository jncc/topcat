
angular.module('app.controllers').controller 'EditorController', 

    ($scope, defaults, $http, record) -> 

        $scope.lookups = {}
        $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
        $scope.state = /^\w\w$/
        $scope.zip = /^\d\d\d\d\d$/

          
        pristine = record

        $scope.reset = () -> $scope.record = angular.copy(pristine)

        $scope.save = () ->
            pristine = $scope.record
            $scope.reset()

        $scope.isResetDisabled = () -> angular.equals(pristine, $scope.record)

        $scope.isSaveDisabled = () -> angular.equals(pristine, $scope.form) # || $scope.theForm.$invalid 

        # call reset() to initially set up form
        $scope.reset()
        return
    

