﻿
angular.module('app.controllers').controller 'EditorController', 

    ($scope, $rootScope, $http, record, Record) -> 

        $scope.lookups = {}
        $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
        $scope.state = /^\w\w$/
        $scope.zip = /^\d\d\d\d\d$/

        
        $scope.reset = () -> $scope.form = angular.copy(record)

        $scope.save = () ->
            record = $scope.form # oo-er, is updating the parameter a good idea?
            $scope.reset()
            $rootScope.busy = { value: true }
            $http.put('../api/records/' + record.id, record)
            $rootScope.busy = { value: false }
            ## todo use resource Record.update 


        $scope.isClean = -> angular.equals($scope.form, record)
        $scope.isSaveHidden = -> $scope.isClean() or record.readOnly
        $scope.isResetHidden = -> $scope.isClean()
        $scope.isSaveDisabled = -> $scope.isClean() # || $scope.theForm.$invalid 

        
        # call reset() to initially set up form
        $scope.reset()
        return
    

