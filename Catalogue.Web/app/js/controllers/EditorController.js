﻿// Generated by IcedCoffeeScript 108.0.11
(function() {
  var fakeValidationData, getDataFormatObj, getOpenDataButtonText, getPendingSignOff, getSecurityText, updateDataFormatObj;

  angular.module('app.controllers').controller('EditorController', function($scope, $http, $routeParams, $location, record, $modal) {
    $scope.editing = {};
    $scope.lookups = {};
    $scope.lookups.currentDataFormat = {};
    $scope.record = record;
    $scope.vocabulator = {};
    $scope.$ = $;
    $http.get('../api/topics').success(function(result) {
      return $scope.lookups.topics = result;
    });
    $http.get('../api/formats?q=').success(function(result) {
      $scope.lookups.currentDataFormat = getDataFormatObj($scope.form.gemini.dataFormat, result);
      return $scope.lookups.formats = result;
    });
    $http.get('../api/roles?q=').success(function(result) {
      return $scope.lookups.roles = result;
    });
    $scope.collapseDataFormatSelector = true;
    $scope.collapseDateFormat = true;
    $scope.getSecurityText = getSecurityText;
    $scope.getDataFormatObj = getDataFormatObj;
    $scope.updateDataFormatObj = updateDataFormatObj;
    $scope.getPendingSignOff = getPendingSignOff;
    $scope.getOpenDataButtonText = getOpenDataButtonText;
    $scope.cancel = function() {
      $scope.reset();
      $scope.lookups.currentDataFormat = getDataFormatObj($scope.form.gemini.dataFormat, $scope.lookups.formats);
      return $scope.notifications.add('Edits cancelled');
    };
    $scope.open = function($event, elem) {
      $event.preventDefault();
      $event.stopPropagation();
      $scope[elem] = true;
    };
    $scope.successResponse = function(response) {
      $scope.reloadRecord(response);
      return $scope.notifications.add('Edits saved');
    };
    $scope.reloadRecord = function(response) {
      $scope.record = response;
      $scope.validation = {};
      $scope.reset();
      $scope.status.refresh();
      return $location.path('/editor/' + $scope.record.id);
    };
    $scope.save = function() {
      var processResult;
      processResult = function(response) {
        var e, errors, field, _i, _j, _len, _len1, _ref;
        if (response.data.success) {
          $scope.successResponse(response.data.record);
        } else {
          $scope.validation = response.data.validation;
          errors = response.data.validation.errors;
          if (errors.length > 0) {
            $scope.notifications.add('There were errors');
            for (_i = 0, _len = errors.length; _i < _len; _i++) {
              e = errors[_i];
              _ref = e.fields;
              for (_j = 0, _len1 = _ref.length; _j < _len1; _j++) {
                field = _ref[_j];
                try {
                  $scope.theForm[field].$setValidity('server', false);
                } catch (_error) {}
              }
            }
          }
        }
        return $scope.busy.stop();
      };
      $scope.busy.start();
      if ($scope.isNew()) {
        return $http.post('../api/records', $scope.form).then(processResult);
      } else {
        return $http.put('../api/records/' + $scope.record.id, $scope.form).then(processResult);
      }
    };
    $scope.clone = function() {
      return $location.path('/clone/' + $scope.form.id);
    };
    $scope.reset = function() {
      return $scope.form = angular.copy($scope.record);
    };
    $scope.isNew = function() {
      return $scope.form.id === '00000000-0000-0000-0000-000000000000';
    };
    $scope.isClean = function() {
      return angular.equals($scope.form, $scope.record);
    };
    $scope.isSaveHidden = function() {
      return $scope.isClean() || $scope.record.readOnly;
    };
    $scope.isCancelHidden = function() {
      return $scope.isClean();
    };
    $scope.isSaveDisabled = function() {
      return $scope.isClean();
    };
    $scope.isCloneHidden = function() {
      return $scope.isNew();
    };
    $scope.isCloneDisabled = function() {
      return !$scope.isClean();
    };
    $scope.isPublishingModalButtonDisabled = function() {
      return !$scope.isSaveHidden();
    };
    $scope.hasUsageConstraints = function() {
      return (!!$scope.form.gemini.limitationsOnPublicAccess && $scope.form.gemini.limitationsOnPublicAccess !== 'no limitations') || (!!$scope.form.gemini.useConstraints && $scope.form.gemini.useConstraints !== 'no conditions apply');
    };
    $scope.removeKeyword = function(keyword) {
      return $scope.form.gemini.keywords.splice($.inArray(keyword, $scope.form.gemini.keywords), 1);
    };
    $scope.addKeywords = function(keywords) {
      var k, _i, _len, _results;
      _results = [];
      for (_i = 0, _len = keywords.length; _i < _len; _i++) {
        k = keywords[_i];
        _results.push($scope.form.gemini.keywords.push(k));
      }
      return _results;
    };
    $scope.editKeywords = function() {
      var modal;
      modal = $modal.open({
        controller: 'VocabulatorController',
        templateUrl: 'views/partials/vocabulator.html?' + new Date().getTime(),
        size: 'lg',
        scope: $scope
      });
      return modal.result.then(function(keywords) {
        return $scope.addKeywords(keywords);
      })["finally"](function() {
        return $scope.editing.keywords = true;
      });
    };
    $scope.editAbstract = function() {
      var modal;
      modal = $modal.open({
        controller: 'MarkdownController',
        templateUrl: 'views/partials/markdown.html?' + new Date().getTime(),
        size: 'lg',
        resolve: {
          'markdown': function() {
            return $scope.form.gemini.abstract;
          }
        }
      });
      return modal.result.then(function(s) {
        return $scope.form.gemini.abstract = s;
      });
    };
    $scope.openPublishingModal = function() {
      var modal;
      modal = $modal.open({
        controller: 'OpenDataModalController',
        templateUrl: 'views/partials/opendatamodal.html?' + new Date().getTime(),
        size: 'lg',
        scope: $scope,
        backdrop: 'static'
      });
      return modal.result.then(function(result) {
        return $scope.reloadRecord(result);
      });
    };
    $scope.removeExtent = function(extent) {
      return $scope.form.gemini.extent.splice($.inArray(extent, $scope.form.gemini.extent), 1);
    };
    $scope.addExtent = function() {
      if ($scope.form.gemini.extent === null) {
        $scope.form.gemini.extent = [];
      }
      return $scope.form.gemini.extent.push({
        authority: '',
        code: ''
      });
    };
    $scope.reset();
    $scope.getKeywords = function(term) {
      return $http.get('../api/keywords?q=' + term).then(function(response) {
        return response.data;
      });
    };
    return $scope.setKeyword = function($item, keyword) {
      return keyword.vocab = $item.vocab;
    };
  });

  getOpenDataButtonText = function(publication) {
    if (publication === null) {
      return "Not Open Data";
    } else if (publication.openData.lastSuccess !== null) {
      return "Published";
    } else if (publication.openData.signOff !== null) {
      return "Signed Off";
    } else if (publication.openData.assessment.completed) {
      return "Assessed";
    } else {
      return "Not Open Data";
    }
  };

  getPendingSignOff = function(publication) {
    if (publication !== null && publication.openData.assessment.completed && publication.openData.signOff === null) {
      return true;
    } else {
      return false;
    }
  };

  getSecurityText = function(n) {
    switch (n) {
      case 0:
        return 'Official';
      case 1:
        return 'Official-Sensitive';
      case 2:
        return 'Secret';
    }
  };

  getDataFormatObj = function(name, formats) {
    var dataType, format, _i, _j, _len, _len1, _ref;
    if (name !== void 0 && formats !== void 0) {
      for (_i = 0, _len = formats.length; _i < _len; _i++) {
        format = formats[_i];
        _ref = format.formats;
        for (_j = 0, _len1 = _ref.length; _j < _len1; _j++) {
          dataType = _ref[_j];
          if (dataType.name === name) {
            return {
              type: format.name,
              text: dataType.name,
              code: dataType.code,
              glyph: format.glyph
            };
          }
        }
      }
      return {
        text: 'None Selected',
        glyph: 'glyphicon-th'
      };
    } else {
      return {
        text: "Other",
        glyph: 'glyphicon-th'
      };
    }
  };

  updateDataFormatObj = function(name, formats, form) {
    form.gemini.dataFormat = name;
    return getDataFormatObj(name, formats);
  };

  fakeValidationData = {
    errors: [
      {
        message: 'There was an error'
      }, {
        message: 'There was another error'
      }
    ]
  };

}).call(this);

//# sourceMappingURL=EditorController.js.map
