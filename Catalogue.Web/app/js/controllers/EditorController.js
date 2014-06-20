(function() {
  var fakeValidationData, getDataFormatObj, getSecurityText, updateDataFormatObj;

  angular.module('app.controllers').controller('EditorController', function($scope, $http, $routeParams, $location, record, Record) {
    $scope.lookups = {};
    $scope.lookups.currentDataFormat = {};
    $scope.lookups.languages = [
      {
        code: 'eng',
        text: 'English'
      }, {
        code: 'cym',
        text: 'Welsh'
      }, {
        code: 'gle',
        text: 'Gaelic (Irish)'
      }, {
        code: 'gla',
        text: 'Gaelic (Scottish)'
      }, {
        code: 'cor',
        text: 'Cornish'
      }, {
        code: 'sco',
        text: 'Ulster Scots'
      }
    ];
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
    $scope.save = function() {
      var processResult;
      processResult = function(response) {
        var e, errors, field, _i, _j, _len, _len1, _ref;
        if (response.data.success) {
          record = response.data.record;
          $scope.validation = {};
          $scope.reset();
          $scope.notifications.add('Edits saved');
          $location.path('/editor/' + record.id);
        } else {
          $scope.validation = response.data.validation;
          errors = response.data.validation.errors;
          if (errors.length > 0) {
            $scope.notifications.add('There were errors');
          }
          for (_i = 0, _len = errors.length; _i < _len; _i++) {
            e = errors[_i];
            _ref = e.fields;
            for (_j = 0, _len1 = _ref.length; _j < _len1; _j++) {
              field = _ref[_j];
              $scope.theForm[field].$setValidity('server', false);
            }
          }
        }
        return $scope.busy.stop();
      };
      $scope.busy.start();
      if ($routeParams.recordId !== '00000000-0000-0000-0000-000000000000') {
        return $http.put('../api/records/' + record.id, $scope.form).then(processResult);
      } else {
        return $http.post('../api/records', $scope.form).then(processResult);
      }
    };
    $scope.reset = function() {
      record.gemini.metadataPointOfContact.name = $scope.user.displayName;
      record.gemini.metadataPointOfContact.email = $scope.user.email;
      return $scope.form = angular.copy(record);
    };
    $scope.isClean = function() {
      return angular.equals($scope.form, record);
    };
    $scope.isSaveHidden = function() {
      return $scope.isClean() || record.readOnly;
    };
    $scope.isCancelHidden = function() {
      return $scope.isClean();
    };
    $scope.isSaveDisabled = function() {
      return $scope.isClean();
    };
    $scope.removeKeyword = function(keyword) {
      return $scope.form.gemini.keywords.splice($.inArray(keyword, $scope.form.gemini.keywords), 1);
    };
    $scope.addKeyword = function() {
      return $scope.form.gemini.keywords.push({
        vocab: '',
        value: ''
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
  });

  getSecurityText = function(n) {
    switch (n) {
      case 0:
        return 'Open';
      case 1:
        return 'Restricted';
      case 2:
        return 'Classified';
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
