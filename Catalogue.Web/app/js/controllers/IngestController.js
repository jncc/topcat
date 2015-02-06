(function() {

  angular.module('app.controllers').controller('IngestController', function($scope, $http) {
    $scope["import"] = {
      id: 0,
      fileName: '',
      skipBadRecords: false
    };
    $scope.imports = [
      {
        id: 0,
        name: 'Activities'
      }, {
        id: 1,
        name: 'Mesh'
      }, {
        id: 2,
        name: 'Publication Catalogue'
      }
    ];
    return $scope.runImport = function() {
      var processResult;
      processResult = function(response) {
        if (response.data.success) {
          $scope.notifications.add('Import run successfully');
        } else {
          $scope.notifications.add('Import failed');
        }
        return $scope.busy.stop();
      };
      $scope.busy.start();
      return $http.post('../api/ingest', $scope["import"]).then(processResult);
    };
  });

}).call(this);
