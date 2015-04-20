module = angular.module 'app.map'

heightForFullHeight = ($window, elem) ->
    viewTop = angular.element($window).innerHeight()
    elemTop = angular.element(elem).offset().top
    (viewTop - elemTop - 50) + 'px' # 50 for a bit of space

module.directive 'tcSearchMap', ($window, $location, $anchorScroll) ->
    link: (scope, elem, attrs) ->
        elem.css 'height', heightForFullHeight $window, elem
        map = L.map elem[0]
        L.tileLayer('https://{s}.tiles.mapbox.com/v4/petmon.lp99j25j/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoicGV0bW9uIiwiYSI6ImdjaXJLTEEifQ.cLlYNK1-bfT0Vv4xUHhDBA',
            maxZoom: 18
            attribution: 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' + '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' + 'Imagery © <a href="http://mapbox.com">Mapbox</a>'
            id: 'petmon.lp99j25j').addTo map
        group = L.layerGroup().addTo map
        query = {}
        normal = color: '#222', fillOpacity: 0.2, weight: 1
        hilite = color: 'rgb(217,38,103)', fillOpacity: 0.6, weight: 1
        scope.$watch 'result.results', (results) ->
            query = for r in results when r.box 
                do (r) ->
                    bounds = [[r.box.south, r.box.west], [r.box.north, r.box.east]]
                    rect = L.rectangle bounds, normal
                    rect.on 'mouseover', -> scope.$apply -> scope.highlighted.id = r.id
                    rect.on 'click', -> scope.$apply ->
                        scope.highlighted.id = r.id
                        $location.hash(r.id);
                        $anchorScroll();    
                    { id: r.id, bounds, rect }
            group.clearLayers()
            group.addLayer x.rect for x in query
            if query.length > 0
                scope.highlighted.id = query[0].id
                map.fitBounds (x.bounds for x in query), padding: [5,5]
        scope.$watch 'highlighted.id', (newer, older) ->
            (x.rect for x in query when x.id is older)[0]?.setStyle normal
            (x.rect for x in query when x.id is newer)[0]?.setStyle hilite


module.directive 'tcBlah', ($window) ->
    link: (scope, elem, attrs) ->
        win = angular.element($window)
        win.bind 'scroll', ->
            # find the results below the top of the viewport and highlight the first one
            query = (el for el in elem.children() when angular.element(el).offset().top > win.scrollTop())
            (scope.$apply -> scope.highlighted.id = query[0].id) if query.length > 0
            
                        
#module.directive 'tcSearchHighlightScroller', ($window) ->
    #link: (scope, elem, attrs) ->
        #scope.$watch 'highlighted.id', (id) ->
            #console.log elem.attr 'id'
            #if id is (elem.attr 'id')
                
        #scope.$watch 'highlighted.scroll', (newer, older) ->
            #if newer isnt older and newer is scope.r
                #y = elem.offset().top
                #$window.scrollTo 0, (y - 20)

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




