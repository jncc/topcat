
angular.module('app.controllers').controller 'AdminController', 

    ($scope, $http) -> 
    
        $scope.myBool = false;
        
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
            $http.post('../api/admin/import',data,header).success (result) -> 
                $scope.myBool = result
        

        
        
