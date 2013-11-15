﻿(function() {
  var module, qtipDefaults;

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

  module.directive('tcEatClick', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).click(function(e) {
          return e.preventDefault();
        });
      }
    };
  });

  qtipDefaults = {
    position: {
      my: 'top center',
      at: 'bottom center',
      viewport: $(window)
    },
    show: {
      event: 'click mouseenter',
      solo: true
    },
    hide: {
      fixed: true,
      delay: 100
    },
    style: {
      classes: 'qtip-dark qtip-rounded tip'
    }
  };

  module.directive('tcTip', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).qtip(qtipDefaults);
      }
    };
  });

  module.directive('tcFocusTip', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).qtip($.extend({}, qtipDefaults, {
          show: {
            event: 'focus'
          },
          hide: {
            event: 'blur'
          },
          position: {
            my: 'bottom center',
            at: 'top center'
          }
        }));
      }
    };
  });

  module.directive('tcTag', function() {
    return {
      link: function(scope, elem, attrs) {
        $(elem).addClass('tag');
        return $(elem).qtip($.extend({}, qtipDefaults, {
          content: {
            text: scope.k.vocab
          }
        }));
      }
    };
  });

  module.directive('tcTagDelete', function() {
    return {
      link: function(scope, elem, attrs) {}
    };
  });

  module.directive('tcTopCopyIcon', function() {
    return {
      link: function(scope, elem, attrs) {
        $(elem).addClass('dark glyphicon glyphicon-leaf');
        return $(elem).qtip($.extend({}, qtipDefaults, {
          position: {
            my: 'left center',
            at: 'right center'
          }
        }));
      }
    };
  });

  module.directive('tcAutosize', function($timeout) {
    return {
      link: function(scope, elem, attrs) {
        return $timeout(function() {
          return $(elem).autosize();
        });
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
