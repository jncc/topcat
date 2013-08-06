
// doing some dependency injection!
var editorModule = angular.module("editor", []);

editorModule.factory("defaults", function () { // angular uses parameter name cos no types in js!
    return {
        name: 'John Smit',
        line1: '123 Main St.',
        city: 'Anytown',
        state: 'AA',
        zip: '12345',
        phone: '1(234) 555-1212'
    };
});

function UserForm($scope, defaults) {
    var master = {
        name: defaults.name,
        address: {
            line1: defaults.line1,
            city: defaults.city,
            state: defaults.state,
            zip: defaults.zip
        },
        contacts: [
          { type: 'phone', value: defaults.phone }
        ]
    };

    $scope.state = /^\w\w$/;
    $scope.zip = /^\d\d\d\d\d$/;

    $scope.cancel = function () {
        $scope.form = angular.copy(master);
    };

    $scope.save = function () {
        master = $scope.form;
        $scope.cancel();
    };

    $scope.addContact = function () {
        $scope.form.contacts.push({ type: '', value: '' });
    };

    $scope.removeContact = function (contact) {
        var contacts = $scope.form.contacts;
        for (var i = 0, ii = contacts.length; i < ii; i++) {
            if (contact === contacts[i]) {
                contacts.splice(i, 1);
            }
        }
    };

    $scope.isCancelDisabled = function () {
        return angular.equals(master, $scope.form);
    };

    $scope.isSaveDisabled = function () {
        return $scope.myForm.$invalid || angular.equals(master, $scope.form);
    };

    $scope.cancel();
}