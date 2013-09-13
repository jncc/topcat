
module = angular.module 'app.directives'

# use jquery placeholder plugin for old IE
# in angular, we can use a custom directive with the same name as the html5 attribute!
module.directive 'placeholder', () -> 
    restrict: 'A', # attribute
    link: (scope, element, attrs) -> $(element).placeholder()

# use jquery autosize plugin to auto-expand textareas
module.directive 'autosize', () -> 
    restrict: 'A', # attribute
    link: (scope, element, attrs) -> $(element).autosize()

module.directive 'tcBackButton', [ '$window', ($window) ->
    restrict: 'A',
    link: (scope, elem, attrs) ->
        elem.on 'click', () -> $window.history.back() ]

module.directive 'spinner', [ '$rootScope', ($rootScope) ->
    restrict: 'A',
    link: (scope, elem, attrs) ->
        elem.addClass 'hide'
        $rootScope.$on '$routeChangeStart', () ->
            elem.removeClass 'hide'
        $rootScope.$on '$routeChangeSuccess', () ->
            elem.addClass 'hide' ]


module.directive 'servervalidation', ($http) ->
    require: 'ngModel',
    link: (scope, elem, attrs, ctrl) ->
        elem.on 'blur', (e) ->
            scope.$apply () -> $http.post('../api/validator', "value": elem.val())
                .success (data) ->
                    ctrl.$setValidity('myErrorKey', data.valid)

module.directive 'tooltip', () ->
    restrict: 'A',
    link: (scope, elem, attrs) -> $(elem).tooltip 
        placement: 'auto',
        delay: show: 500, hide: 100
        

module.directive 'locationclipboard', () ->
    restrict: 'A', #attribute
    link: (scope, elem, attrs) ->
        clip = new ZeroClipboard $(elem)
        clip.on 'complete', (client, args) ->
            l = $('#location')
            l.highlightInputSelectionRange 0, (l.val().length) 
            l.tooltip
                title: 'Copied to clipboard!',
                trigger: 'manual',
                container: 'body'
            l.tooltip 'show'
            setTimeout (() -> l.tooltip 'hide'), 2000 # angular $timeout is not working?!

# from http://programanddesign.com/js/jquery-select-text-range/
$.fn.highlightInputSelectionRange = (start, end) ->
    this.each () ->
        if this.setSelectionRange # non-IE
            this.focus()
            this.setSelectionRange start, end
        else if this.createTextRange # IE
            range = this.createTextRange()
            range.collapse true
            range.moveEnd 'character', end
            range.moveStart 'character', start
            range.select()


