angular.module('app.controllers').controller 'OpenDataPublishingController',

    ($scope, $http, $location, $timeout) ->
        
        m =
            tab: 5 # default to fifth UI tab
            openData:
                summary: {}
                list: []
        
        $scope.m = m
        $scope.signOffStatus = {}
        $scope.recordTimeoutMap = {}

        loadSummary = -> $http.get('../api/publishing/opendata/summary').success (result) -> m.openData.summary = result
        
        load1 = -> $http.get('../api/publishing/opendata/publishedsincelastupdated').success (result) -> m.openData.list = result
        load2 = -> $http.get('../api/publishing/opendata/notpublishedsincelastupdated').success (result) -> m.openData.list = result
        load3 = -> $http.get('../api/publishing/opendata/publicationneverattempted').success (result) -> m.openData.list = result
        load4 = -> $http.get('../api/publishing/opendata/lastpublicationattemptwasunsuccessful').success (result) -> m.openData.list = result
        load5 = -> $http.get('../api/publishing/opendata/pendingsignoff').success (result) ->
            m.openData.list = result
            $scope.signOffStatus[r.id] = "Sign Off" for r in result
        
        loadList = ->
            switch m.tab
                when 1 then load1()
                when 2 then load2()
                when 3 then load3()
                when 4 then load4()
                when 5 then load5()
                
        $scope.$watch 'm.tab', loadList

        loadSummary()
        load5()


        $scope.submitSignOff = (recordId) ->
            $scope.signOffRequest = {}
            $scope.signOffRequest.id = recordId
            $scope.signOffRequest.comment = ""

            $http.put('../api/publishing/opendata/signoff', $scope.signOffRequest)
            .success (result) ->
                $scope.signOffStatus[recordId] = "Signed Off"
                loadSummary()
                $scope.loadRecordsPendingSignOff()
                $scope.notifications.add "Successfully signed off"
            .catch (error) ->
                if (error.status == 401)
                    $scope.notifications.add "Unauthorised - not in valid sign off group"
                else
                    $scope.notifications.add error.data.exceptionMessage

                $scope.signOffStatus[recordId] = "Retry?"
                delete $scope.recordTimeoutMap[recordId]

        $scope.allowGraceTime = (recordId) ->
            if ($scope.recordTimeoutMap[recordId] > 0)
                $scope.signOffStatus[recordId] = "Cancel " + ("0" + $scope.recordTimeoutMap[recordId]--).slice(-2)
                $timeout $scope.allowGraceTime.bind(null, recordId), 1000
            else if (recordId of $scope.recordTimeoutMap)
                $scope.submitSignOff(recordId)

        $scope.cancelSignOff = (recordId) ->
            delete $scope.recordTimeoutMap[recordId]
            $scope.signOffStatus[recordId] = "Sign Off"

        $scope.signOffButtonClick = (recordId) ->
            if (recordId not of $scope.recordTimeoutMap)
                $scope.recordTimeoutMap[recordId] = 10; # seconds
                $scope.allowGraceTime(recordId)
            else
                $scope.cancelSignOff recordId
                
