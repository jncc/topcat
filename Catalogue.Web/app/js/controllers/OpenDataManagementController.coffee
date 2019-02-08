angular.module('app.controllers').controller 'OpenDataManagementController',

    ($scope, $http, $location, signOffGroup) ->
        
        m =
            tab: 2 # default to second UI tab
            openData:
                summary: {}
                list: []
        
        $scope.m = m

        loadSummary = -> $http.get('../api/publishing/summary').success (result) -> m.openData.summary = result
        
        loadTab1Data = -> $http.get('../api/publishing/publishedsincelastupdated').success (result) -> m.openData.list = result
        loadTab2Data = -> $http.get('../api/publishing/notpublishedsincelastupdated').success (result) -> m.openData.list = result
        loadTab3Data = -> $http.get('../api/publishing/publicationneverattempted').success (result) -> m.openData.list = result
        loadTab4Data = -> $http.get('../api/publishing/lastpublicationattemptwasunsuccessful').success (result) -> m.openData.list = result
        loadTab5Data = -> $http.get('../api/publishing/pendingsignoff').success (result) ->
            m.openData.list = result
        
        loadList = ->
            switch m.tab
                when 1 then loadTab1Data()
                when 2 then loadTab2Data()
                when 3 then loadTab3Data()
                when 4 then loadTab4Data()
                when 5 then loadTab5Data()
                
        $scope.$watch 'm.tab', loadList

        loadSummary()
        loadTab2Data()
