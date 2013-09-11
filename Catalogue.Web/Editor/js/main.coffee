
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
            scope.$apply () -> $http(
                method: 'POST',
                url: '../api/validator',
                data: "value": elem.val())
                .success (data, status, headers, config) ->
                    console.log data
                    ctrl.$setValidity('myErrorKey', data.valid)
