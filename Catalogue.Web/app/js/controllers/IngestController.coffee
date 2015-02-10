angular.module('app.controllers').controller 'IngestController',

    ($scope, $http) -> 
        $scope.import = {id: 0, fileName: '', skipBadRecords: false, importKeywords: false}
        $scope.imports = [{id: 0, name: 'Activities'},
                {id: 1, name: 'Mesh'},
                {id: 2, name: 'Publication Catalogue'}]
                
                
                
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
        
        
