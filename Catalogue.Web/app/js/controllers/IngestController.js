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
        name: 'Topcat'
      }, {
        id: 1,
        name: 'Activities'
      }, {
        id: 2,
        name: 'Mesh'
      }, {
        id: 3,
        name: 'Publications'
      }
    ];
    return $scope.runImport = function() {
      var processResult;
      processResult = function(response) {
        if (response.data.success) {
          $scope.notifications.add('Import run successfully');
        } else {
          $scope.notifications.add('Import failed');
          $scope.notifications.add(response.data.exception);
        }
        return $scope.busy.stop();
      };
      $scope.busy.start();
      return $http.post('../api/ingest', $scope["import"]).then(processResult);
    };
  });

}).call(this);

//# sourceMappingURL=IngestController.js.map
