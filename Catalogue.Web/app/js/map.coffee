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
select = fillOpacity: 0.5, weight: 1, color: 'rgb(217,38,103)'

getArea = (bounds) ->
    [[s, w], [n, e]] = bounds
    x = e - w
    y = n - s
    Math.abs (x * y)

getBestPadding = (tuples) ->
    onlyOneBoxIsVisible = (allBounds) ->
        largest = _.max allBounds, (b) -> getArea b
        isWithin = (bounds, other) ->
            [[s, w], [n, e]] = bounds
            [[sx, wx], [nx, ex]] = other
            s >= sx and n <= nx and w >= wx and e <= ex
        _.every allBounds, (b) -> isWithin b, largest 
    if onlyOneBoxIsVisible(_.map tuples, (t) -> t.bounds)
        padding: [120, 120] # zoom out fairly wide so we can see the surrounding space
    else
        padding: [5, 5] # zoom in to fit - there's likely to be space between the various boxes

# the records to show on the map, paired with additional map-related objects
# triples of { record, coordinate bounds, leaflet rectangle }
tuples = {}

updateTuples = (results, scope) ->
    if results
        tuples = for r in results when r.box isnt null and r.box.north
            do (r) ->
                bounds = [[r.box.south, r.box.west], [r.box.north, r.box.east]]
                rect = L.rectangle bounds, normal
                rect.on 'mouseover', -> rect.setStyle fillOpacity: 0.4
                rect.on 'mouseout', -> rect.setStyle fillOpacity: 0.2
                rect.on 'click', -> scope.$apply ->
                    scope.current.zoomed = r
                    #$location.hash(r.id);
                    #$anchorScroll();
                { r, bounds, rect }
    
module.directive 'tcSearchMap', ($window, $location, $anchorScroll) ->
    link: (scope, elem, attrs) ->
        map = L.map elem[0], { zoomControl: false }
        L.control.zoom { position: 'bottomright'}
            .addTo map
        map.addLayer baseLayer
        group = L.layerGroup().addTo map # a group for the rectangles
        scope.$watch 'result.results', (results) ->
            updateTuples results, scope
            group.clearLayers()
            ordered = _(tuples).sortBy((x) -> getArea x.bounds).reverse().value()
            group.addLayer x.rect for x in ordered
            elem.css 'height', calculateBestHeightForMap $window, elem
            scope.current.zoomed = null
            # select the first result initially
            if tuples.length > 0
                scope.current.selected = tuples[0].r
                map.fitBounds (x.bounds for x in tuples), getBestPadding tuples
        #map.on 'zoomend', -> scope.$apply -> scope.map.current.selected = null
        scope.$watch 'current.topmost', (result) ->
            if not scope.current.zoomed
                scope.current.selected = result
        scope.$watch 'current.selected', (newer, older) ->
            if newer isnt older
                # hilite selected box
                _(tuples).find((x) -> x.r is newer)?.rect.setStyle select
                _(tuples).find((x) -> x.r is older)?.rect.setStyle normal
        scope.$watch 'current.zoomed', (newer, older) ->
            if newer and newer isnt older
                # zoom in and set current.selected
                tuple = _(tuples).find((x) -> x.r is newer)
                map.fitBounds tuple.bounds, padding: [50, 50]
                scope.current.selected = newer

module.directive 'tcSearchResultScrollHighlighter', ($window) ->
    link: (scope, elem, attrs) ->
        win = angular.element($window)
        win.bind 'scroll', ->
            # find the results below the top of the viewport and highlight the first one
            q = (el for el in elem.children() when angular.element(el).offset().top > win.scrollTop())
            result = (x.r for x in tuples when x.r.id is q[0]?.id)[0]
            (scope.$apply -> scope.current.topmost = result) if result 

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




