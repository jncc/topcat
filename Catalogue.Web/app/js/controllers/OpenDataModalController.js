﻿// Generated by IcedCoffeeScript 108.0.11
(function() {
  angular.module('app.controllers').controller('OpenDataModalController', function($scope, $http, $timeout, recordOutput) {
    var formatDate, publishingStatus, refreshAssessmentInfo, refreshSignOffInfo, refreshUploadInfo, useTwoDigits;
    formatDate = function(date) {
      var day, month, year;
      year = date.getFullYear();
      month = useTwoDigits(date.getMonth() + 1);
      day = useTwoDigits(date.getDate());
      return "" + day + "/" + month + "/" + year;
    };
    useTwoDigits = function(val) {
      if (val < 10) {
        return "0" + val;
      }
      return val;
    };
    $scope.assessmentRequest = {};
    $scope.assessmentRequest.id = $scope.form.id;
    $scope.recordOutput = recordOutput;
    publishingStatus = {
      riskAssessment: {
        currentClass: {},
        completed: {}
      },
      signOff: {
        currentClass: {},
        completed: {},
        timeout: {},
        showButton: {}
      },
      upload: {
        currentClass: {},
        completed: {}
      },
      currentActiveView: {}
    };
    $scope.publishingStatus = publishingStatus;
    publishingStatus.signOff.timeout = -1;
    $scope.refreshPublishingStatus = function() {
      if ($scope.form.publication !== null && $scope.form.publication.gov !== null) {
        publishingStatus.riskAssessment.completed = $scope.form.publication.gov.assessment !== null && $scope.form.publication.gov.assessment.completed;
        publishingStatus.signOff.completed = $scope.form.publication.gov.signOff !== null;
        publishingStatus.upload.completed = $scope.form.publication.gov.lastSuccess !== null;
      }
      if ($scope.recordOutput.recordState.openDataPublishingState.assessedAndUpToDate) {
        publishingStatus.riskAssessment.currentClass = "visited";
      } else if (publishingStatus.riskAssessment.completed) {
        publishingStatus.riskAssessment.currentClass = "current";
      } else {
        publishingStatus.riskAssessment.currentClass = "current";
      }
      if ($scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate) {
        publishingStatus.signOff.currentClass = "visited";
      } else if ($scope.recordOutput.recordState.openDataPublishingState.assessedAndUpToDate) {
        publishingStatus.signOff.currentClass = "current";
      } else {
        publishingStatus.signOff.currentClass = "disabled";
      }
      if ($scope.recordOutput.recordState.openDataPublishingState.uploadedAndUpToDate) {
        return publishingStatus.upload.currentClass = "visited";
      } else if ($scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate) {
        return publishingStatus.upload.currentClass = "current";
      } else {
        return publishingStatus.upload.currentClass = "disabled";
      }
    };
    $scope.refreshPublishingStatus();
    if ($scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate) {
      publishingStatus.currentActiveView = "upload";
    } else if ($scope.recordOutput.recordState.openDataPublishingState.assessedAndUpToDate) {
      publishingStatus.currentActiveView = "sign off";
    } else {
      publishingStatus.currentActiveView = "risk assessment";
    }
    refreshAssessmentInfo = function() {
      if ($scope.form.publication !== null && $scope.form.publication.gov !== null && $scope.form.publication.gov.assessment !== null && $scope.form.publication.gov.assessment.completed) {
        if ($scope.form.publication.gov.assessment.completedByUser === null && $scope.form.publication.gov.assessment.initialAssessmentWasDoneOnSpreadsheet) {
          return $scope.assessmentCompletedInfo = "Initial assessment completed on spreadsheet";
        } else if ($scope.recordOutput.recordState.openDataPublishingState.assessedAndUpToDate) {
          return $scope.assessmentCompletedInfo = "Completed by " + $scope.form.publication.gov.assessment.completedByUser.displayName + " on " + moment(new Date($scope.form.publication.gov.assessment.completedOnUtc)).format('DD MMM YYYY h:mm a');
        } else {
          return $scope.assessmentCompletedInfo = "Last completed by " + $scope.form.publication.gov.assessment.completedByUser.displayName + " on " + moment(new Date($scope.form.publication.gov.assessment.completedOnUtc)).format('DD MMM YYYY h:mm a');
        }
      }
    };
    refreshSignOffInfo = function() {
      publishingStatus.signOff.showButton = $scope.user.isIaoUser && !$scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate;
      if ($scope.form.publication !== null && $scope.form.publication.gov.signOff !== null) {
        if ($scope.form.publication.gov.signOff.user === null) {
          $scope.signOffCompletedInfo = "Initial sign off completed on spreadsheet";
        } else if ($scope.recordOutput.recordState.openDataPublishingState.signedOffAndUpToDate) {
          $scope.signOffCompletedInfo = "Signed off by " + $scope.form.publication.gov.signOff.user.displayName + " on " + moment(new Date($scope.form.publication.gov.signOff.dateUtc)).format('DD MMM YYYY h:mm a');
        } else {
          $scope.signOffCompletedInfo = "Last signed off by " + $scope.form.publication.gov.signOff.user.displayName + " on " + moment(new Date($scope.form.publication.gov.signOff.dateUtc)).format('DD MMM YYYY h:mm a');
        }
      }
      if ($scope.user.isIaoUser) {
        return publishingStatus.signOff.signOffButtonText = "SIGN OFF";
      } else {
        return publishingStatus.signOff.signOffButtonText = "Pending sign off";
      }
    };
    refreshUploadInfo = function() {
      $scope.hubPublishingStatus = function() {
        if ($scope.form.publication.hub === null) {
          return "Pending";
        } else if ($scope.form.publication.hub.lastSuccess !== null && $scope.form.gemini.metadataDate <= $scope.form.publication.hub.lastSuccess) {
          return "Completed on " + moment(new Date($scope.form.publication.hub.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a');
        } else if ($scope.form.publication.hub.lastSuccess !== null) {
          return "Pending - last completed on " + moment(new Date($scope.form.publication.hub.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a');
        } else if ($scope.form.publication.hub.lastAttempt !== null) {
          return "Pending - last attempted on " + moment(new Date($scope.form.publication.hub.lastAttempt.dateUtc)).format('DD MMM YYYY h:mm a');
        } else {
          return "Pending";
        }
      };
      $scope.govPublishingStatus = function() {
        if ($scope.form.publication.gov === null) {
          return "Pending";
        }
        if ($scope.form.publication.gov.lastSuccess !== null && $scope.form.gemini.metadataDate <= $scope.form.publication.gov.lastSuccess) {
          return "Completed on " + moment(new Date($scope.form.publication.gov.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a');
        } else if ($scope.form.publication.gov.lastSuccess !== null) {
          return "Pending - last completed on " + moment(new Date($scope.form.publication.gov.lastSuccess.dateUtc)).format('DD MMM YYYY h:mm a');
        } else if ($scope.form.publication.gov.lastAttempt !== null) {
          return "Pending - last failed on " + moment(new Date($scope.form.publication.gov.lastAttempt.dateUtc)).format('DD MMM YYYY h:mm a') + " with error \"" + $scope.form.publication.gov.lastAttempt.message + "\"";
        } else {
          return "Pending";
        }
      };
      if ($scope.recordOutput.recordState.openDataPublishingState.uploadedAndUpToDate) {
        return $scope.uploadStatus = "Publishing completed";
      } else {
        return $scope.uploadStatus = "Publishing in progress...";
      }
    };
    refreshAssessmentInfo();
    refreshSignOffInfo();
    refreshUploadInfo();
    $scope.assessButtonClick = function() {
      return $http.put('../api/publishing/opendata/assess', $scope.assessmentRequest).success(function(result) {
        $scope.status.refresh();
        $scope.form = result.record;
        $scope.recordOutput = {
          record: result.record,
          recordState: result.recordState
        };
        refreshAssessmentInfo();
        $scope.refreshPublishingStatus();
        return publishingStatus.currentActiveView = "sign off";
      })["catch"](function(error) {
        $scope.notifications.add(error.data.exceptionMessage);
        return $scope.$dismiss();
      });
    };
    $scope.signOffButtonClick = function() {
      if (publishingStatus.signOff.timeout === -1) {
        publishingStatus.signOff.timeout = 10;
        return $scope.allowGraceTime();
      } else {
        return $scope.cancelSignOff();
      }
    };
    $scope.submitSignOff = function() {
      $scope.signOffRequest = {
        id: $scope.form.id,
        comment: ""
      };
      return $http.put('../api/publishing/opendata/signoff', $scope.signOffRequest).success(function(result) {
        $scope.form = result.record;
        $scope.recordOutput = {
          record: result.record,
          recordState: result.recordState,
          publishingPolicy: result.publishingPolicy
        };
        $scope.status.refresh();
        refreshSignOffInfo();
        $scope.refreshPublishingStatus();
        return publishingStatus.currentActiveView = "upload";
      })["catch"](function(error) {
        if (error.status === 401) {
          $scope.notifications.add("Unauthorised - not in valid sign off group");
        } else {
          $scope.notifications.add(error.data.exceptionMessage);
        }
        publishingStatus.signOff.timeout = -1;
        return $scope.$dismiss();
      });
    };
    $scope.allowGraceTime = function() {
      if (publishingStatus.signOff.timeout > 0) {
        publishingStatus.signOff.signOffButtonText = "Cancel " + ("0" + publishingStatus.signOff.timeout--).slice(-2);
        return $timeout($scope.allowGraceTime, 1000);
      } else if (publishingStatus.signOff.timeout === 0) {
        $timeout.cancel;
        return $scope.submitSignOff();
      }
    };
    $scope.cancelSignOff = function() {
      $timeout.cancel;
      publishingStatus.signOff.timeout = -1;
      return refreshSignOffInfo();
    };
    return $scope.close = function() {
      return $scope.$close($scope.recordOutput);
    };
  });

}).call(this);

//# sourceMappingURL=OpenDataModalController.js.map
