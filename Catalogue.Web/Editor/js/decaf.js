(function() {
  var module;

  module = angular.module('editor', []);

  module.factory('defaults', function() {
    return {
      name: 'John Smit',
      line1: '123 Main St.',
      city: 'Anytown',
      state: 'AA',
      zip: '12345',
      phone: '1(234) 555-1212'
    };
  });

  module.controller('Controller1', function($scope, defaults) {
    var master;
    master = {
      name: defaults.name,
      address: {
        line1: defaults.line1,
        city: defaults.city,
        state: defaults.state,
        zip: defaults.zip
      },
      contacts: [
        {
          type: 'phone',
          value: defaults.phone
        }
      ]
    };
    $scope.state = /^\w\w$/;
    $scope.zip = /^\d\d\d\d\d$/;
    $scope.cancel = function() {
      return $scope.form = angular.copy(master);
    };
    $scope.save = function() {
      master = $scope.form;
      return $scope.cancel();
    };
    $scope.addContact = function() {
      return $scope.form.contacts.push({
        type: '',
        value: ''
      });
    };
    $scope.removeContact = function(contact) {
      var i;
      i = $scope.form.contacts.indexOf(contact);
      return $scope.form.contacts.splice(i, 1);
    };
    $scope.isCancelDisabled = function() {
      return angular.equals(master, $scope.form);
    };
    $scope.isSaveDisabled = function() {
      return $scope.myForm.$invalid || angular.equals(master, $scope.form);
    };
    $scope.cancel();
  });

}).call(this);
