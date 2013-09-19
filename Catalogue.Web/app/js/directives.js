(function() {
  var module;

  module = angular.module('app.directives');

  module.directive('placeholder', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).placeholder();
      }
    };
  });

  module.directive('autofocus', function() {
    return {
      link: function(scope, elem, attrs) {
        return elem[0].focus();
      }
    };
  });

  module.directive('tcAutofocusIfBlank', function() {
    return {
      link: function(scope, elem, attrs) {
        if (!elem.val()) {
          return elem[0].focus();
        }
      }
    };
  });

  module.directive('tcAutosize', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).autosize();
      }
    };
  });

  module.directive('tcBackButton', function($window) {
    return {
      link: function(scope, elem, attrs) {
        return elem.on('click', function() {
          return $window.history.back();
        });
      }
    };
  });

  module.directive('tcSpinner', function($rootScope) {
    return {
      link: function(scope, elem, attrs) {
        elem.addClass('ng-hide');
        $rootScope.$on('$routeChangeStart', function() {
          return elem.removeClass('ng-hide');
        });
        return $rootScope.$on('$routeChangeSuccess', function() {
          return elem.addClass('ng-hide');
        });
      }
    };
  });

  module.directive('tcDatepicker', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).datepicker({
          format: 'yyyy-dd-mm'
        });
      }
    };
  });

  module.directive('tcServerValidation', function($http) {
    return {
      require: 'ngModel',
      link: function(scope, elem, attrs, ctrl) {
        return elem.on('blur', function(e) {
          return scope.$apply(function() {
            return $http.post('../api/validator', {
              "value": elem.val()
            }).success(function(data) {
              return ctrl.$setValidity('myErrorKey', data.valid);
            });
          });
        });
      }
    };
  });

  module.directive('tcFocusTip', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).tooltip({
          trigger: 'focus',
          placement: 'top',
          delay: {
            show: 0,
            hide: 100
          }
        });
      }
    };
  });

  module.directive('tcCopyToClipboard', function() {
    return {
      link: function(scope, elem, attrs) {
        var clip;
        clip = new ZeroClipboard($(elem));
        return clip.on('complete', function(client, args) {
          var t;
          t = $('#' + attrs.tcCopyToClipboard);
          console.log(t);
          t.tooltip('destroy');
          t.highlightInputSelectionRange(0, (t.val().length));
          return setTimeout((function() {
            t.tooltip({
              html: 'Copied to clipboard!',
              trigger: 'manual',
              position: 'bottom'
            });
            t.tooltip('show');
            return setTimeout((function() {
              return t.tooltip('hide');
            }), 2000);
          }), 1000);
        });
      }
    };
  });

}).call(this);
