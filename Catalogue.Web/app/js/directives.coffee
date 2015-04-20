module = angular.module 'app.directives'

# use jquery placeholder plugin (for old IE)
# in angular, we can use a custom directive with the same name as the html5 attribute!
module.directive 'placeholder', () -> 
    link: (scope, elem, attrs) -> $(elem).placeholder()

# make autofocus work for old IE
module.directive 'autofocus', () ->
    link: (scope, elem, attrs) -> elem[0].focus() # call focus on the raw dom object

# specific autofocus directive (for modal dialogues)
module.directive 'tcFocus', ($timeout) ->
    link: (scope, elem, attrs) ->
        $timeout (-> elem[0].focus()), 100 # timeout makes it work :-(

# eat the click (used on the search page) for old IE
module.directive 'tcEatClick', () ->
    link: (scope, elem, attrs) ->
        $(elem).click (e) -> e.preventDefault()

# default qtip options
qtipDefaults = 
    position:
        my: 'bottom center',
        at: 'top center',
        viewport: $(window) # but keep in the viewport if poss
    show:
        event: 'click mouseenter',
        solo: true,
        delay: 0,
    hide:
        fixed: true,
        delay: 0,
    style:
        classes: 'qtip-dark qtip-rounded tip'

# basic tip
module.directive 'tcTip', () ->
    link: (scope, elem, attrs) ->
        if (attrs.tcTip == 'bottom') # optionally accept a position argument
            $(elem).qtip $.extend {}, qtipDefaults, position:
                my: 'top center',
                at: 'bottom center',
        else
            $(elem).qtip qtipDefaults
            

# focus tip (used for editor fields)
module.directive 'tcFocusTip', () ->
    link: (scope, elem, attrs) ->
        $(elem).qtip $.extend {}, qtipDefaults,
            show: event: 'focus'
            hide: event: 'blur'

module.directive 'tcQtipTitle', () ->
    link: (scope, elem, attrs) ->                     
        scope.$watch 'lookups.currentDataFormat.text', ->
            text = scope.lookups.currentDataFormat.text
            if scope.lookups.currentDataFormat.code != undefined && scope.lookups.currentDataFormat.code != ''
                text = text + ' (' + scope.lookups.currentDataFormat.code + ')'
            $(elem).qtip $.extend {}, qtipDefaults, 
                overwrite: true
                content:
                    text: text
                show:
                    event: 'mouseenter'
                position:
                    my: 'top center',
                    at: 'bottom center'        


# widget for tags (keywords)
module.directive 'tcTag', (colourHasher) ->
    link: (scope, elem, attrs) ->
        elem.addClass 'tag'
        # prepend a coloured edging strip to represent the vocab
        edge = if scope.k.vocab
            colour = colourHasher.hashStringToColour scope.k.vocab
            angular.element '<span class="vocabful-colour-edge" style="background-color:' + colour + '">&nbsp;</span>'
        else
            angular.element '<span class="vocabless-colour-edge"></span>'
        elem.prepend edge
        # call qtip with options constructed from the defaults
        $(elem).qtip $.extend {}, qtipDefaults,
            content: text: scope.k.vocab || "No vocabulary"
            position:
                my: 'top center'
                at: 'bottom center'

# use jquery autosize plugin to auto-expand textareas
module.directive 'tcAutosize', ($timeout) -> 
    link: (scope, elem, attrs) ->
        # use $timeout(fn, 0) to get the box to resize properly on load
        $timeout -> $(elem).autosize()

module.directive 'tcBackButton', ($window) ->
    link: (scope, elem, attrs) ->
        elem.on 'click', () -> $window.history.back()

module.directive 'tcSpinner', ($rootScope) ->
    link: (scope, elem, attrs) ->
        elem.addClass 'ng-hide'
        $rootScope.$on '$routeChangeStart', () ->
            elem.removeClass 'ng-hide'
        $rootScope.$on '$routeChangeSuccess', () ->
            elem.addClass 'ng-hide'

# (this is clever) binds the scrolltop value of the element to the supplied expression
module.directive 'tcBindScrollPosition', ($parse, $timeout) ->
    link: (scope, elem, attrs) ->
        getter = $parse attrs.tcBindScrollPosition # see docs for $parse
        value = getter scope
        # hack: delay to avoid setting the scroll before it has content
        ($timeout (-> elem[0].scrollTop = value), 0) if value
        elem.bind 'scroll', ->
            setter = getter.assign
            setter scope, elem[0].scrollTop
            scope.$apply()
            
### module.directive 'tcDatepicker', () ->
  link: (scope, elem, attrs) ->
        $(elem).datepicker
            format: 'yyyy-dd-mm'
            ###

module.directive 'tcServerValidation', ($http) ->
    require: 'ngModel',
    link: (scope, elem, attrs, ctrl) ->
        elem.on 'blur', (e) ->
            scope.$apply () -> $http.post('../api/validator', "value": elem.val())
                .success (data) ->
                    ctrl.$setValidity('myErrorKey', data.valid)




module.directive 'tcCopyPathToClipboard', ($timeout) ->
    link: (scope, elem, attrs) ->
        clip = new ZeroClipboard $(elem)
        $(clip.htmlBridge).qtip $.extend {}, qtipDefaults,
            content: text: 'Copy path to clipboard'
            position:
                my: 'top center'
                at: 'bottom center'
        $(clip.htmlBridge).addClass 'hover-fix'
        clip.on 'complete', (client, args) ->
            t = $('#' + attrs.tcCopyPathToClipboard)
            t.highlightInputSelectionRange 0, (t.val().length)
            t.qtip 'disable', true
            wrapper = $('.editor-path')
            wrapper.qtip $.extend {}, qtipDefaults,
                content: text: 'Copied to clipboard!'
            wrapper.qtip 'show'
            $timeout (->
                wrapper.qtip 'hide'
                wrapper.qtip 'disable'
                t.qtip 'disable', false), 2000


# currently not working because i don't know quite how to have multiple zero clipboards
#module.directive 'tcCopyDirectoryToClipboard', ($timeout) ->
#    link: (scope, elem, attrs) ->
#        clip = new ZeroClipboard $(elem)
        #$(clip.htmlBridge).qtip $.extend {}, qtipDefaults, content: text: 'XXCopy directory to clipboard'
#        clip.on 'complete', (client, args) ->
#            t = $('#' + attrs.tcCopyDirectoryToClipboard)
#            t.highlightInputSelectionRange 0, (t.val().length)
#            t.qtip 'disable', true
#            wrapper = $('.editor-path')
#            wrapper.qtip $.extend {}, qtipDefaults,
#                content: text: 'Copied to clipboard!'
#            wrapper.qtip 'show'
#            $timeout (->
#                wrapper.qtip 'hide'
#                wrapper.qtip 'disable'
#                t.qtip 'disable', false), 2000

# this directive simply sets the control's "server" validity key to true so that
# the controls stops showing as invalid as soon as the user starts to correct it
module.directive 'tcServerValidated', ->
    require: 'ngModel'
    link: (scope, elem, attrs, modelCtrl) ->
        modelCtrl.$viewChangeListeners.push ->
            modelCtrl.$setValidity('server', true)

# http://stackoverflow.com/a/20086923/40759
module.directive "tcDebounce", ($timeout) ->
  restrict: "A"
  require: "ngModel"
  priority: 99
  link: (scope, elm, attr, ngModelCtrl) ->
    return  if attr.type is "radio" or attr.type is "checkbox"
    elm.unbind "input"
    debounce = undefined
    elm.bind "input", ->
      $timeout.cancel debounce
      debounce = $timeout(->
        scope.$apply ->
          ngModelCtrl.$setViewValue elm.val()
      , 500)
    elm.bind "blur", ->
      scope.$apply ->
        ngModelCtrl.$setViewValue elm.val()


module.directive 'tcDropdown', ($timeout) ->
    restrict: 'E'
    transclude: true
    replace: true
    template: '<div>
                    <form>
                        <input type="text" ng-model="term" ng-change="query()" autocomplete="off" />
                    </form>
                    <div ng-transclude></div>
                </div>'
    scope:
        search: '&'
        select: '&'
        items:  '='
        term:   '='
    controller: ($scope) ->
        $scope.items = []
        $scope.hide = false
        this.activate = (item) -> $scope.active = item


    