
angular.module('app.controllers').controller 'ImagePickerController',

    ($scope, $http, recordImage) ->
        $scope.recordImage = angular.copy(recordImage)
        $scope.images = []
        $scope.rowsPerPage = 3
        $scope.imagesPerRow = 3
        $scope.noOfPages = 0
        $scope.currentPage = 0
        $scope.page = []

        ###$scope.images = 
        [
            {
                Url: "",
                Width: 2048,
                Height: 1360,
                FileType: "jpg",
                EditorName: "1",
                LastEdited: "2018-12-17T14:55:29",
                SizeInKB: 351.013671875,
                Crops: {
                    ListingThumbnail: ""
                }
            }
        ]###


        $scope.getImages = () ->
            $http.get('')
                .then (response) ->
                    $scope.images = response.data

                    # looks like they're already sorted, but just in case
                    $scope.images = $scope.images.sort((first, second) -> new Date(second.LastEdited) - new Date(first.LastEdited))

                    console.log $scope.images
                    $scope.noOfPages = Math.ceil $scope.images.length/($scope.rowsPerPage*$scope.imagesPerRow)
                    $scope.setPage($scope.currentPage)
        

        $scope.setPage = (pageNo) ->
            if pageNo < 0 || pageNo == $scope.noOfPages
                return # do nothing

            noOfImagesPerPage = $scope.rowsPerPage*$scope.imagesPerRow

            if $scope.images.length <= noOfImagesPerPage
                $scope.page = $scope.makePageFromImages $scope.images
            else
                $scope.page = $scope.makePageFromImages $scope.images.slice(noOfImagesPerPage*pageNo, noOfImagesPerPage*pageNo+noOfImagesPerPage)

            $scope.currentPage = pageNo
                
        $scope.makePageFromImages = (images) ->
            newPage = []

            if images.length <= $scope.imagesPerRow
                newPage.push images
            else
                rows = Math.ceil(images.length/$scope.imagesPerRow)
                for i in [0...rows]
                    row = images.slice(i*$scope.imagesPerRow, i*$scope.imagesPerRow+$scope.imagesPerRow)
                    newPage.push row

            return newPage

        $scope.saveImage = (image) ->

            if !$scope.recordImage
                $scope.recordImage =
                    url: null
                    height: 0
                    width: 0
                    crops:
                        squareUrl: null
                        thumbnailUrl: null

            $scope.recordImage.url = image.Url
            $scope.recordImage.width = image.Width
            $scope.recordImage.height = image.Height
            $scope.recordImage.crops.squareUrl = image.Crops.Square
            $scope.recordImage.crops.thumbnailUrl = image.Crops.ListingThumbnail

        $scope.ok = () ->
            $scope.$close $scope.recordImage

        $scope.getImages()


    