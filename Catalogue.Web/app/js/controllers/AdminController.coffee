angular.module('app.controllers').controller 'AdminController',

    ($scope, $http) -> 
        $scope.myBool = {};
        $scope.myBool.import = false;
        $scope.myBool.seedMesh = false;
        
        $scope.getBool = -> 
            $http.get('../api/admin/bool').success (result) -> 
                $scope.myBool = result
         
        #data = "=C:\\topcat\\import_data\\Human_Activities_Metadata_Catalogue.csv";
        data = {"path":"C:\\topcat\\import_data\\Human_Activities_Metadata_Catalogue.csv"};
        
        header =  {headers: {
            'Accept':'application/json', 
            'Content-Type':'application/json'
           #'Content-Type':'application/x-www-form-urlencoded'
            }}; 
        
        $scope.import = -> 
            $http.post('../api/admin',data,header).success (result) -> 
                $scope.myBool.import = result
            
        $scope.seedMesh = -> 
            $http.put('../api/admin').success (result) -> 
                $scope.myBool.seedMesh = result
        

        
        
