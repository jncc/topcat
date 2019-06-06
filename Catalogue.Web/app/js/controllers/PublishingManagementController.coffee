angular.module('app.controllers').controller 'PublishingManagementController',

    ($scope, $http, $location) ->
        
        m =
            tab: 2 # default to second UI tab
            publishing:
                summary: {}
                list: []
        
        $scope.m = m

        loadSummary = -> $http.get('../api/publishing/summary').success (result) -> m.publishing.summary = result
        
        loadTab1Data = -> $http.get('../api/publishing/publishedsincelastupdated').success (result) -> m.publishing.list = result
        loadTab2Data = -> $http.get('../api/publishing/notpublishedsincelastupdated').success (result) -> m.publishing.list = result
        loadTab3Data = -> $http.get('../api/publishing/publicationneverattempted').success (result) -> m.publishing.list = result
        loadTab4Data = -> $http.get('../api/publishing/pendingsignoff').success (result) ->
            m.openData.list = result
        
        loadList = ->
            switch m.tab
                when 1 then loadTab1Data()
                when 2 then loadTab2Data()
                when 3 then loadTab3Data()
                when 4 then loadTab4Data()
                
        $scope.$watch 'm.tab', loadList

        loadSummary()
        loadTab2Data()