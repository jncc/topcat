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


        $scope.isSaveAndResetHidden = ->
            angular.equals($scope.form, record) || $record.readOnly
        $scope.isSaveDisabled = ->
            angular.equals($scope.form, record) # || $scope.theForm.$invalid 

        # call reset() to initially set up form
        $scope.reset()
        return
    

