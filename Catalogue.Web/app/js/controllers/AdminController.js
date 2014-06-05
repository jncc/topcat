(function() {

  angular.module('app.controllers').controller('AdminController', function($scope, $http) {
    var data, header;
    $scope.myBool = false;
    $scope.getBool = function() {
      return $http.get('../api/admin/bool').success(function(result) {
        return $scope.myBool = result;
      });
    };
    data = "=C:\\topcat\\import_data\\Human_Activities_Metadata_Catalogue.csv";
    header = {
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/x-www-form-urlencoded'
      }
    };
    return $scope["import"] = function() {
      return $http.post('../api/admin/import', data, header).success(function(result) {
        return $scope.myBool = result;
      });
    };
  });

}).call(this);
