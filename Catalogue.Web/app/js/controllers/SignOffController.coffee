angular.module('app.controllers').controller 'SignOffController',

    ($scope, $http, $location, $timeout, signOffGroup) ->
        
        m =
            publishing:
                summary: {}
                list: []
            signOffStatus: {}
            signOffTimeoutMap: {}
        
        $scope.m = m

        loadData = -> $http.get('../api/publishing/pendingsignoff').success (result) ->
            m.publishing.list = result
            m.signOffStatus[r.id] = "Sign Off" for r in result
                
        loadData()


        $scope.submitSignOff = (recordId) ->
            $scope.signOffRequest = { id: recordId, comment: "" }

            $http.put('../api/publishing/signoff', $scope.signOffRequest)
            .success (result) ->
                m.signOffStatus[recordId] = "Signed Off"
                $scope.status.refresh()
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
                