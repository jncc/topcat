angular.module('app.controllers').controller 'PublishingController',

    ($scope, $http) ->
        
        
        m =
            tab: 2 # default to second UI tab
            openData:
                summary: {}
                list: []
        
        $scope.m = m

        $http.get('../api/publishing/opendata/summary').success (result) -> m.openData.summary = result
        
        
        load1 = -> $http.get('../api/publishing/opendata/publishedsincelastupdated').success (result) -> m.openData.list = result
        load2 = -> $http.get('../api/publishing/opendata/notpublishedsincelastupdated').success (result) -> m.openData.list = result
        load3 = -> $http.get('../api/publishing/opendata/publicationneverattempted').success (result) -> m.openData.list = result
        load4 = -> $http.get('../api/publishing/opendata/lastpublicationattemptwasunsuccessful').success (result) -> m.openData.list = result
        
        loadList = -> 
            switch m.tab
                when 1 then load1()
                when 2 then load2()
                when 3 then load3()
                when 4 then load4()
                
        $scope.$watch 'm.tab', loadList

        load2()
        
        

        