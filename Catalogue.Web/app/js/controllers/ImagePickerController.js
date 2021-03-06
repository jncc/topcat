﻿// Generated by IcedCoffeeScript 108.0.11
(function() {
  angular.module('app.controllers').controller('ImagePickerController', function($scope, $http, imagePickerUrl, recordImage) {
    $scope.recordImage = angular.copy(recordImage);
    $scope.images = [];
    $scope.rowsPerPage = 2;
    $scope.imagesPerRow = 4;
    $scope.noOfPages = 0;
    $scope.currentPage = 0;
    $scope.page = [];
    $scope.selectedImageUrl = "";
    $scope.selectedImage = {};
    $scope.getImages = function() {
      if ($scope.recordImage) {
        $scope.selectedImageUrl = $scope.recordImage.url;
      }
      if (imagePickerUrl) {
        $http.get(imagePickerUrl).then(function(response) {
          $scope.images = response.data;
          return $scope.refreshPages();
        });
        return $scope.images = [];
      }
    };
    $scope.refreshPages = function() {
      $scope.images = $scope.images.sort(function(first, second) {
        return new Date(second.LastEdited) - new Date(first.LastEdited);
      });
      $scope.noOfPages = Math.ceil($scope.images.length / ($scope.rowsPerPage * $scope.imagesPerRow));
      return $scope.setPage($scope.currentPage);
    };
    $scope.setPage = function(pageNo) {
      var noOfImagesPerPage;
      if (pageNo < 0 || pageNo === $scope.noOfPages) {
        return;
      }
      noOfImagesPerPage = $scope.rowsPerPage * $scope.imagesPerRow;
      if ($scope.images.length <= noOfImagesPerPage) {
        $scope.page = $scope.makePageFromImages($scope.images);
      } else {
        $scope.page = $scope.makePageFromImages($scope.images.slice(noOfImagesPerPage * pageNo, noOfImagesPerPage * pageNo + noOfImagesPerPage));
      }
      return $scope.currentPage = pageNo;
    };
    $scope.makePageFromImages = function(images) {
      var i, newPage, row, rows, _i;
      newPage = [];
      if (images.length <= $scope.imagesPerRow) {
        newPage.push(images);
      } else {
        rows = Math.ceil(images.length / $scope.imagesPerRow);
        for (i = _i = 0; 0 <= rows ? _i < rows : _i > rows; i = 0 <= rows ? ++_i : --_i) {
          row = images.slice(i * $scope.imagesPerRow, i * $scope.imagesPerRow + $scope.imagesPerRow);
          newPage.push(row);
        }
      }
      return newPage;
    };
    $scope.saveImage = function(image) {
      $scope.setBlankImage();
      if (image.Url === $scope.selectedImageUrl) {
        $scope.recordImage.url = image.Url;
        $scope.recordImage.width = image.Width;
        $scope.recordImage.height = image.Height;
        $scope.recordImage.crops.squareUrl = image.Crops.Square;
        $scope.recordImage.crops.thumbnailUrl = image.Crops.ListingThumbnail;
      } else if ($scope.selectedImageUrl) {
        $scope.recordImage.url = $scope.selectedImageUrl;
      } else {
        $scope.recordImage = null;
      }
      return $scope.$close($scope.recordImage);
    };
    $scope.setBlankImage = function() {
      return $scope.recordImage = {
        url: null,
        height: 0,
        width: 0,
        crops: {
          squareUrl: null,
          thumbnailUrl: null
        }
      };
    };
    $scope.setSelectedImage = function(image) {
      $scope.selectedImage = image;
      return $scope.selectedImageUrl = image.Url;
    };
    return $scope.getImages();
  });

}).call(this);

//# sourceMappingURL=ImagePickerController.js.map
