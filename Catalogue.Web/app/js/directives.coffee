
module = angular.module 'app.directives'

# use jquery placeholder plugin (for old IE)
# in angular, we can use a custom directive with the same name as the html5 attribute!
module.directive 'placeholder', () -> 
    link: (scope, elem, attrs) -> $(elem).placeholder()

# make autofocus work for old IE
module.directive 'autofocus', () ->
    link: (scope, elem, attrs) -> elem[0].focus() # call focus on the raw dom object

# eat the click (used on the search page) for old IE
module.directive 'tcEatClick', () ->
    link: (scope, elem, attrs) ->
        $(elem).click (e) -> e.preventDefault()

# default qtip options
qtipDefaults = 
    position:
        my: 'top center',
        at: 'bottom center',
        viewport: $(window) # but keep in the viewport if poss
    show:
        event: 'click mouseenter',
        solo: true,
    hide:
        fixed: true,
        delay: 100,
    style:
        classes: 'qtip-tipsy tag-tip'

# basic tip
# widget for tags (keywords)
module.directive 'tcTip', () ->
    link: (scope, elem, attrs) -> $(elem).qtip qtipDefaults

# widget for tags (keywords)
module.directive 'tcTag', () ->
    link: (scope, elem, attrs) ->
        $(elem).addClass('tag')
        # call qtip with options constructed from the defaults
        $(elem).qtip $.extend {}, qtipDefaults, content: text: scope.k.vocab

# top-copy icon
module.directive 'tcTopCopyIcon', () ->
    link: (scope, elem, attrs) ->
        $(elem).addClass('dark glyphicon glyphicon-leaf')
        # call qtip with options constructed from the defaults
        $(elem).qtip $.extend {}, qtipDefaults,
            position:
                my: 'left center',
                at: 'right center',


# use jquery autosize plugin to auto-expand textareas
module.directive 'tcAutosize', () -> 
    link: (scope, elem, attrs) -> $(elem).autosize()

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

#module.directive 'tcDatepicker', () ->
#    link: (scope, elem, attrs) ->
#        $(elem).datepicker
#            format: 'yyyy-dd-mm'

module.directive 'tcServerValidation', ($http) ->
    require: 'ngModel',
    link: (scope, elem, attrs, ctrl) ->
        elem.on 'blur', (e) ->
            scope.$apply () -> $http.post('../api/validator', "value": elem.val())
                .success (data) ->
                    ctrl.$setValidity('myErrorKey', data.valid)

module.directive 'tcFocusTip', () ->
    link: (scope, elem, attrs) -> $(elem).tooltip 
        trigger: 'focus',
        placement: 'top',
        delay: show: 0, hide: 100

module.directive 'tcCopyToClipboard', () ->
    link: (scope, elem, attrs) ->
        clip = new ZeroClipboard $(elem)
        clip.on 'complete', (client, args) ->
            t = $('#' + attrs.tcCopyToClipboard)
            console.log t
            t.tooltip 'destroy'
            t.highlightInputSelectionRange 0, (t.val().length) 
            setTimeout (() ->
                t.tooltip
                    html: 'Copied to clipboard!',
                    trigger: 'manual',
                    position: 'bottom',
                    #container: 'body'
                t.tooltip 'show'
                setTimeout (() -> t.tooltip 'hide'), 2000 # angular $timeout is not working?!
                ), 1000

