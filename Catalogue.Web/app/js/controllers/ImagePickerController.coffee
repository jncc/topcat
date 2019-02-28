
angular.module('app.controllers').controller 'ImagePickerController',

    ($scope, $http, imagePickerUrl, recordImage) ->
        $scope.recordImage = angular.copy(recordImage)
        $scope.images = []
        $scope.rowsPerPage = 2
        $scope.imagesPerRow = 4
        $scope.noOfPages = 0
        $scope.currentPage = 0
        $scope.page = []
        $scope.selectedImageUrl = ""
        $scope.selectedImage = {}

        $scope.getImages = () ->
            $http.get(imagePickerUrl)
                .then (response) ->
                    $scope.images = response.data

            # looks like they're already sorted, but just in case
            $scope.images = $scope.images.sort((first, second) -> new Date(second.LastEdited) - new Date(first.LastEdited))
            
            if $scope.recordImage
                $scope.selectedImageUrl = $scope.recordImage.url

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
            $scope.setBlankImage()
            if image.Url is $scope.selectedImageUrl
                $scope.recordImage.url = image.Url
                $scope.recordImage.width = image.Width
                $scope.recordImage.height = image.Height
                $scope.recordImage.crops.squareUrl = image.Crops.Square
                $scope.recordImage.crops.thumbnailUrl = image.Crops.ListingThumbnail
            else if $scope.selectedImageUrl
                $scope.recordImage.url = $scope.selectedImageUrl
            else
                $scope.recordImage = null

            $scope.$close $scope.recordImage

        $scope.setBlankImage = () ->
            $scope.recordImage =
                url: null
                height: 0
                width: 0
                crops:
                    squareUrl: null
                    thumbnailUrl: null

        $scope.setSelectedImage = (image) ->
            $scope.selectedImage = image
            $scope.selectedImageUrl = image.Url

        $scope.getFilename = (url) ->
            return url.substring(url.lastIndexOf('/')+1);

        $scope.getImages()


    