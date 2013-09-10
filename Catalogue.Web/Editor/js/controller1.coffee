



# when this Controller1 is instantiated, this is it
# (not sure if this is the right way to grab the module in a separate file?)
angular.module('editor').controller 'Controller1', ($scope, defaults, $http) -> 

    $scope.lookups = {}

    $http.get('../api/topics').success (result) -> $scope.lookups.topics = result
          
    master = 
        name: defaults.name,
        address: 
            line1: defaults.line1,
            city: defaults.city,
            state: defaults.state,
            zip: defaults.zip,
        contacts: [
          type: 'phone', value: defaults.phone
        ],
        topic: ''

    $scope.prefs = {}
    $scope.prefs.bigLocation = false

    $scope.state = /^\w\w$/
    $scope.zip = /^\d\d\d\d\d$/

    $scope.cancel = () -> $scope.form = angular.copy(master)

    $scope.save = () ->
        master = $scope.form
        $scope.cancel()

    $scope.addContact = () -> $scope.form.contacts.push({ type: '', value: '' })

    $scope.removeContact = (contact) ->
        i = $scope.form.contacts.indexOf contact # indexOf doesn't work in ie7!
        $scope.form.contacts.splice i, 1

    $scope.isCancelDisabled = () -> angular.equals(master, $scope.form)

    $scope.isSaveDisabled = () -> $scope.editorForm.$invalid || angular.equals(master, $scope.form)

    # call scope.cancel() to initially set up form
    $scope.cancel()
    return



