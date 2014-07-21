(function() {

  angular.module('app.controllers').controller('ImportController', function($scope, $http) {
    var data, header;
    $scope.myBool = {};
    $scope.myBool["import"] = false;
    $scope.myBool.seedMesh = false;
    $scope.getBool = function() {
      return $http.get('../api/admin/bool').success(function(result) {
        return $scope.myBool = result;
      });
    };
    data = {
      "path": "C:\\topcat\\import_data\\Human_Activities_Metadata_Catalogue.csv"
    };
    header = {
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    };
    $scope["import"] = function() {
      return $http.post('../api/admin', data, header).success(function(result) {
        return $scope.myBool["import"] = result;
      });
    };
    return $scope.seedMesh = function() {
      return $http.put('../api/admin').success(function(result) {
        return $scope.myBool.seedMesh = result;
      });
    };
  });

}).call(this);
