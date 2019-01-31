﻿// Generated by IcedCoffeeScript 108.0.11
(function() {
  var addOpenDataResource, fakeValidationData, getDataFormatObj, getOpenDataButtonText, getOpenDataButtonToolTip, getPendingSignOff, getResourceUrl, getSecurityText, isFilePath, removeOpenDataResource, trimDoubleQuotes, updateDataFormatObj;

  angular.module('app.controllers').controller('EditorController', function($scope, $http, $routeParams, $location, record, $modal, Account) {
    Account.then(function(user) {
      return $scope.user = user;
    });
    $scope.editing = {};
    $scope.lookups = {};
    $scope.lookups.currentDataFormat = {};
    $scope.recordOutput = record;
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
    $scope.getOpenDataButtonToolTip = getOpenDataButtonToolTip;
    $scope.addOpenDataResource = addOpenDataResource;
    $scope.removeOpenDataResource = removeOpenDataResource;
    $scope.trimDoubleQuotes = trimDoubleQuotes;
    $scope.getResourceUrl = getResourceUrl;
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
      $scope.recordOutput = {
        record: response.record,
        recordState: response.recordState,
        publishingPolicy: response.publishingPolicy
      };
      $scope.validation = {};
      $scope.reset();
      $scope.status.refresh();
      return $location.path('/editor/' + $scope.recordOutput.record.id);
    };
    $scope.save = function() {
      var processResult;
      processResult = function(response) {
        var e, errors, field, _i, _j, _len, _len1, _ref;
        if (response.data.success) {
          $scope.successResponse(response.data);
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
        return $http.put('../api/records/' + $scope.recordOutput.record.id, $scope.form).then(processResult);
      }
    };
    $scope.clone = function() {
      return $location.path('/clone/' + $scope.form.id);
    };
    $scope.reset = function() {
      return $scope.form = angular.copy($scope.recordOutput.record);
    };
    $scope.isNew = function() {
      return $scope.form.id === '00000000-0000-0000-0000-000000000000';
    };
    $scope.isClean = function() {
      return angular.equals($scope.form, $scope.recordOutput.record);
    };
    $scope.isSaveHidden = function() {
      return $scope.isClean() || $scope.recordOutput.record.readOnly;
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
    $scope.isPublishDisabled = function() {
      return !$scope.isSaveHidden();
    };
    $scope.isHttpPath = function(path) {
      return path && path.toLowerCase().startsWith("http");
    };
    $scope.isPublishingModalButtonVisible = function() {
      return $scope.form.publication && $scope.form.publication.gov && $scope.form.publication.gov.publishable === true;
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
        resolve: {
          'recordOutput': function() {
            return $scope.recordOutput;
          }
        },
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
    $scope.setKeyword = function($item, keyword) {
      return keyword.vocab = $item.vocab;
    };
    $scope.fillManagerDetails = function() {
      if (!$scope.form.manager) {
        $scope.form.manager = {};
      }
      return $scope.form.manager.displayName = $scope.user.displayName;
    };
    $scope.setPublishable = function(value) {
      if (!$scope.form.publication) {
        $scope.form.publication = {};
      }
      if (!$scope.form.publication.gov) {
        $scope.form.publication.gov = {};
      }
      if ((value === true && $scope.form.publication.gov.publishable === true) || (value === false && $scope.form.publication.gov.publishable === false)) {
        return $scope.form.publication.gov.publishable = null;
      } else {
        return $scope.form.publication.gov.publishable = value;
      }
    };
    return $scope.togglePublishable = function() {
      if (!$scope.form.publication) {
        $scope.form.publication = {};
      }
      if (!$scope.form.publication.gov) {
        $scope.form.publication.gov = {};
        $scope.form.publication.gov.publishable = null;
      }
      if ($scope.form.publication.gov.publishable === null) {
        return $scope.form.publication.gov.publishable = true;
      } else if ($scope.form.publication.gov.publishable === true) {
        return $scope.form.publication.gov.publishable = false;
      } else {
        return $scope.form.publication.gov.publishable = null;
      }
    };
  });

  isFilePath = function(path) {
    return path && path.match(/^([a-z]:|\\\\jncc-corpfile\\)/i);
  };

  addOpenDataResource = function(record) {
    if (!record.publication) {
      record.publication = {};
    }
    if (!record.publication.data) {
      record.publication.data = {};
    }
    if (!record.publication.data.resources) {
      record.publication.data.resources = [];
    }
    record.publication.data.resources.push({
      path: ""
    });
    return console.log(record.publication.data.resources.length);
  };

  getResourceUrl = function(resource) {
    if (resource.path.startsWith("http://") || resource.path.startsWith("https://")) {
      return resource.path;
    } else if (resource.publishedUrl !== null) {
      return resource.publishedUrl;
    } else {
      return null;
    }
  };

  removeOpenDataResource = function(record, resource) {
    return record.publication.data.resources.splice($.inArray(resource, record.publication.data.resources), 1);
  };

  trimDoubleQuotes = function(s) {
    if (s.match(/^(").*(")$/)) {
      return s.substring(1, s.length - 1);
    } else {
      return s;
    }
  };

  getOpenDataButtonToolTip = function(record, publishingState) {
    if (!isFilePath(record.path)) {
      return "Open data publishing not available for non-file resources";
    } else if (record.publication === null || record.publication.gov === null || record.publication.gov.publishable !== true) {
      return "The open data publication status of the record, editing the record may affect the status.";
    } else if (record.publication.gov.lastSuccess !== null && record.publication.gov.lastSuccess !== void 0 && !publishingState.assessedAndUpToDate) {
      return "This record has been changed since it was last published, it may need republishing.";
    } else {
      return "The open data publication status of the record, editing the record may affect the status.";
    }
  };

  getOpenDataButtonText = function(record, publishingState) {
    if (record.publication === null || record.publication.gov === null || record.publication.gov.publishable !== true) {
      return "Publishable";
    } else if (record.publication.gov.lastSuccess !== null && record.publication.gov.lastSuccess !== void 0 && !publishingState.assessedAndUpToDate) {
      return "Republish";
    } else if (publishingState.uploadedAndUpToDate) {
      return "Published";
    } else if (publishingState.signedOffAndUpToDate) {
      return "Signed Off";
    } else if (publishingState.assessedAndUpToDate) {
      return "Assessed";
    } else {
      return "Publishable";
    }
  };

  getPendingSignOff = function(publication) {
    if (publication !== null && publication.assessment.completed && publication.signOff === null) {
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
