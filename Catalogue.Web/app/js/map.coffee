module = angular.module 'app.map'

calculateBestHeightForMap = ($window, elem) ->
    viewTop = angular.element($window).innerHeight()
    elemTop = angular.element(elem).offset().top
    (viewTop - elemTop - 10) + 'px'

baseLayer = L.tileLayer 'https://{s}.tiles.mapbox.com/v4/petmon.lp99j25j/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoicGV0bW9uIiwiYSI6ImdjaXJLTEEifQ.cLlYNK1-bfT0Vv4xUHhDBA',
    maxZoom: 18
    attribution: 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' + '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' + 'Imagery © <a href="http://mapbox.com">Mapbox</a>'
    id: 'petmon.lp99j25j'
    
normal = fillOpacity: 0.2, weight: 1, color: '#222'
hilite = fillOpacity: 0.6, weight: 1, color: 'rgb(217,38,103)'

# the records to show on the map, paired with additional map-related
tuples = {}

# make a single element for the query - a tuple of { record, coordinate bounds, leaflet rectangle }
makeTuple = (r, scope) ->
    bounds = [[r.box.south, r.box.west], [r.box.north, r.box.east]]
    rect = L.rectangle bounds, normal
    #rect.on 'mouseover', -> scope.$apply -> scope.highlighted.result = r
    rect.on 'click', -> scope.$apply ->
        scope.highlighted.result = r
        #$location.hash(r.id);
        #$anchorScroll();
    { r, bounds, rect }

module.directive 'tcSearchMap', ($window, $location, $anchorScroll) ->
    link: (scope, elem, attrs) ->
        map = L.map elem[0]
        map.addLayer baseLayer
        group = L.layerGroup().addTo map # a group for the rectangles
        scope.$watch 'result.results', (results) ->
            tuples = for r in results when r.box.north 
                do (r) -> makeTuple r, scope
            group.clearLayers()
            group.addLayer x.rect for x in tuples
            elem.css 'height', calculateBestHeightForMap $window, elem
            console.log tuples.length
            if tuples.length > 0
                scope.highlighted.result = tuples[0].r
        map.on 'zoomend', -> scope.$evalAsync -> scope.highlighted.goto = null
        scope.$watch 'highlighted.result', (newer, older) ->
            scope.highlighted.goto = null
            (map.fitBounds (x.bounds for x in tuples), padding: [5, 5]) if tuples.length
            (x.rect for x in tuples when x.r is older)[0]?.setStyle normal
            (x.rect for x in tuples when x.r is newer)[0]?.setStyle hilite
        scope.$watch 'highlighted.goto', (newer) ->
            rectangle = (x.rect for x in tuples when x.r is newer)[0]
            (map.fitBounds rectangle, padding: [50, 50]) if rectangle


module.directive 'tcSearchResultScrollHighlighter', ($window) ->
    link: (scope, elem, attrs) ->
        win = angular.element($window)
        win.bind 'scroll', ->
            # find the results below the top of the viewport and highlight the first one
            q = (el for el in elem.children() when angular.element(el).offset().top > win.scrollTop())
            result = (x.r for x in tuples when x.r.id is q[0].id)[0]
            (scope.$apply -> scope.highlighted.result = result) if result 

# sticks the element to the top of the viewport when scrolled past
# used for the search map - untested for use elsewhere!
module.directive 'tcStickToTop', ($window, $timeout) ->
    link: (scope, elem, attrs) ->
        win = angular.element($window)
        getPositions = ->
            v: win.scrollTop()
            e: elem.offset().top
            w: elem.width()
        f = ->
            initial = getPositions()
            # stick when the window is below the element
            # unstick when the window is above the original element position
            win.bind 'scroll', ->
                current = getPositions()
                if current.v > current.e
                    elem.addClass 'stick-to-top'
                    elem.css 'width', initial.w # workaround disappearing map
                else if current.v < initial.e
                    elem.removeClass 'stick-to-top'
                    elem.css 'width', ''
        $timeout f, 100 # delay to allow page to render so we can get initial position




