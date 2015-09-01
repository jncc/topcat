angular.module('app.controllers').controller 'IngestController',

    ($scope, $http) -> 
        $scope.import = {id: 0, fileName: '', skipBadRecords: false}
        $scope.imports = [
            {id: 0, name: 'Topcat'},
            {id: 1, name: 'Activities'},
            {id: 2, name: 'Mesh'},
            {id: 3, name: 'Publications'}
            {id: 4, name: 'Meow'}
            {id: 5, name: 'SeabedSurvey'}]
                
                
                
        $scope.runImport = ->
            processResult = (response) ->
                if response.data.success
                    $scope.notifications.add 'Import run successfully'
                else
                    $scope.notifications.add 'Import failed'
                    $scope.notifications.add response.data.exception
                                
                $scope.busy.stop()

            $scope.busy.start()
            $http.post('../api/ingest', $scope.import).then processResult
        
        
