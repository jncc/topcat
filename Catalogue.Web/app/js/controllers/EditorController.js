(function() {

  angular.module('app.controllers').controller('EditorController', function($scope, defaults, $http) {
    var master;
    $scope.lookups = {};
    $http.get('../api/topics').success(function(result) {
      return $scope.lookups.topics = result;
    });
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
      ],
      topic: ''
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
      return angular.equals(master, $scope.form);
    };
    $scope.cancel();
  });

}).call(this);
