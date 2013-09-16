
module = angular.module 'app.directives'

# use jquery placeholder plugin (for old IE)
# in angular, we can use a custom directive with the same name as the html5 attribute!
module.directive 'placeholder', () -> 
    link: (scope, elem, attrs) -> $(elem).placeholder()

# make autofocus work for old IE
module.directive 'autofocus', () ->
    link: (scope, elem, attrs) -> elem[0].focus() # call focus on the raw dom object

module.directive 'tcAutofocusIfBlank', () ->
    link: (scope, elem, attrs) -> elem[0].focus() if !elem.val()

# use jquery autosize plugin to auto-expand textareas
module.directive 'tcAutosize', () -> 
    link: (scope, elem, attrs) -> $(elem).autosize()

module.directive 'tcBackButton', [ '$window', ($window) ->
    link: (scope, elem, attrs) ->
        elem.on 'click', () -> $window.history.back() ]

module.directive 'tcSpinner', [ '$rootScope', ($rootScope) ->
    link: (scope, elem, attrs) ->
        elem.addClass 'hide'
        $rootScope.$on '$routeChangeStart', () ->
            elem.removeClass 'hide'
        $rootScope.$on '$routeChangeSuccess', () ->
            elem.addClass 'hide' ]


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

