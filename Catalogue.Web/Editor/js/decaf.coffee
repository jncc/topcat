

# a "module" is essentially a configuration for Angular's dependency injector
# which allows you to group a set of controllers, directives, filters, etc. under one name
module = angular.module('editor', []);

# when i ask for defaults, provide this object
module.factory 'defaults', () ->
    name: 'John Smit',
    line1: '123 Main St.',
    city: 'Anytown',
    state: 'AA',
    zip: '12345',
    phone: '1(234) 555-1212',

# when this Controller1 is instantiated, this is it
module.controller 'Controller1', ($scope, defaults) -> 

    master = 
        name: defaults.name,
        address: 
            line1: defaults.line1,
            city: defaults.city,
            state: defaults.state,
            zip: defaults.zip,
        contacts: [
          type: 'phone', value: defaults.phone
        ]

    $scope.state = /^\w\w$/
    $scope.zip = /^\d\d\d\d\d$/

    $scope.cancel = () -> $scope.form = angular.copy(master)

    $scope.save = () ->
        master = $scope.form
        $scope.cancel()

    $scope.addContact = () -> $scope.form.contacts.push({ type: '', value: '' })

    $scope.removeContact = (contact) ->
        i = $scope.form.contacts.indexOf contact
        $scope.form.contacts.splice i, 1

    $scope.isCancelDisabled = () -> angular.equals(master, $scope.form)

    $scope.isSaveDisabled = () -> $scope.myForm.$invalid || angular.equals(master, $scope.form)

    $scope.cancel()
    return


