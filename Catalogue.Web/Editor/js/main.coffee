﻿
# a "module" is essentially a configuration for Angular's dependency injector
# which allows you to group a set of controllers, directives, filters, etc. under one name

module = angular.module('editor', ['$strap.directives']); #strap.directives is for angular-strap

# when i ask for defaults, provide this object
module.factory 'defaults', () ->
    name: 'John Smit',
    line1: '123 Main St.',
    city: 'Anytown',
    state: 'AA',
    zip: '12345',
    phone: '1(234) 555-1212',

# use jquery placeholder plugin for old IE
# in angular, we can use a custom directive with the same name as the html5 attribute!
module.directive 'placeholder', () -> 
    restrict: 'A', # attribute
    link: (scope, element, attrs) -> $(element).placeholder()

# use jquery autosize plugin to auto-expand textareas
module.directive 'autosize', () -> 
    restrict: 'A', # attribute
    link: (scope, element, attrs) -> $(element).autosize()

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
