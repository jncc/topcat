angular.module('app.controllers').controller 'OpenDataPublishingController',

    ($scope, $http, $location, $timeout, signOffGroup) ->
        
        m =
            tab: 5 # default to fifth UI tab
            openData:
                summary: {}
                list: []
            signOffStatus: {}
            signOffTimeoutMap: {}
        
        $scope.m = m

        loadSummary = -> $http.get('../api/publishing/opendata/summary').success (result) -> m.openData.summary = result
        
        loadTab1Data = -> $http.get('../api/publishing/opendata/publishedsincelastupdated').success (result) -> m.openData.list = result
        loadTab2Data = -> $http.get('../api/publishing/opendata/notpublishedsincelastupdated').success (result) -> m.openData.list = result
        loadTab3Data = -> $http.get('../api/publishing/opendata/publicationneverattempted').success (result) -> m.openData.list = result
        loadTab4Data = -> $http.get('../api/publishing/opendata/lastpublicationattemptwasunsuccessful').success (result) -> m.openData.list = result
        loadTab5Data = -> $http.get('../api/publishing/opendata/pendingsignoff').success (result) ->
            m.openData.list = result
            m.signOffStatus[r.id] = "Sign Off" for r in result
        
        loadList = ->
            switch m.tab
                when 1 then loadTab1Data()
                when 2 then loadTab2Data()
                when 3 then loadTab3Data()
                when 4 then loadTab4Data()
                when 5 then loadTab5Data()
                
        $scope.$watch 'm.tab', loadList

        loadSummary()
        loadTab5Data()


        $scope.submitSignOff = (recordId) ->
            $scope.signOffRequest = { id: recordId, comment: "" }

            $http.put('../api/publishing/opendata/signoff', $scope.signOffRequest)
            .success (result) ->
                m.signOffStatus[recordId] = "Signed Off"
                loadSummary()
                $scope.loadRecordsPendingSignOff()
                $scope.notifications.add "Successfully signed off"
            .catch (error) ->
                if (error.status == 401)
                    $scope.notifications.add "Unauthorised - not in valid sign off group"
                else
                    $scope.notifications.add error.data.exceptionMessage

                m.signOffStatus[recordId] = "Retry?"
                delete m.signOffTimeoutMap[recordId]

        $scope.allowGraceTime = (recordId) ->
            if (m.signOffTimeoutMap[recordId] > 0)
                m.signOffStatus[recordId] = "Cancel " + ("0" + m.signOffTimeoutMap[recordId]--).slice(-2)
                $timeout $scope.allowGraceTime.bind(null, recordId), 1000
            else if (recordId of m.signOffTimeoutMap)
                $scope.submitSignOff(recordId)

        $scope.cancelSignOff = (recordId) ->
            delete m.signOffTimeoutMap[recordId]
            m.signOffStatus[recordId] = "Sign Off"

        $scope.signOffButtonClick = (recordId) ->
            if (recordId not of m.signOffTimeoutMap)
                m.signOffTimeoutMap[recordId] = 10; # seconds
                $scope.allowGraceTime(recordId)
            else
                $scope.cancelSignOff recordId
                
