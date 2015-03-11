(function() {
  var hashStringToColour, hslToRgb, module, qtipDefaults;

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
      my: 'bottom center',
      at: 'top center',
      viewport: $(window)
    },
    show: {
      event: 'click mouseenter',
      solo: true,
      delay: 0
    },
    hide: {
      fixed: true,
      delay: 0
    },
    style: {
      classes: 'qtip-dark qtip-rounded tip'
    }
  };

  module.directive('tcTip', function() {
    return {
      link: function(scope, elem, attrs) {
        if (attrs.tcTip === 'bottom') {
          return $(elem).qtip($.extend({}, qtipDefaults, {
            position: {
              my: 'top center',
              at: 'bottom center'
            }
          }));
        } else {
          return $(elem).qtip(qtipDefaults);
        }
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
          }
        }));
      }
    };
  });

  module.directive('tcQtipTitle', function() {
    return {
      link: function(scope, elem, attrs) {
        return scope.$watch('lookups.currentDataFormat.text', function() {
          var text;
          text = scope.lookups.currentDataFormat.text;
          if (scope.lookups.currentDataFormat.code !== void 0 && scope.lookups.currentDataFormat.code !== '') {
            text = text + ' (' + scope.lookups.currentDataFormat.code + ')';
          }
          return $(elem).qtip($.extend({}, qtipDefaults, {
            overwrite: true,
            content: {
              text: text
            },
            show: {
              event: 'mouseenter'
            },
            position: {
              my: 'top center',
              at: 'bottom center'
            }
          }));
        });
      }
    };
  });

  module.directive('tcTag', function() {
    return {
      link: function(scope, elem, attrs) {
        var colour, edge;
        elem.addClass('tag');
        edge = scope.k.vocab ? (colour = hashStringToColour(scope.k.vocab), angular.element('<span class="vocabful-colour-edge" style="background-color:' + colour + '">&nbsp;</span>')) : angular.element('<span class="vocabless-colour-edge"></span>');
        elem.prepend(edge);
        return $(elem).qtip($.extend({}, qtipDefaults, {
          content: {
            text: scope.k.vocab || "No vocabulary"
          },
          position: {
            my: 'top center',
            at: 'bottom center'
          }
        }));
      }
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


  /* module.directive 'tcDatepicker', () ->
    link: (scope, elem, attrs) ->
          $(elem).datepicker
              format: 'yyyy-dd-mm'
   */

  module.directive('tcServerValidation', function($http) {
    return {
      require: 'ngModel',
      link: function(scope, elem, attrs, ctrl) {
        return elem.on('blur', function(e) {
          return scope.$apply(function() {
            return $http.post('../api/validator', {
              "value": elem.val()
            });
          }).success(function(data) {
            return ctrl.$setValidity('myErrorKey', data.valid);
          });
        });
      }
    };
  });

  module.directive('tcCopyPathToClipboard', function($timeout) {
    return {
      link: function(scope, elem, attrs) {
        var clip;
        clip = new ZeroClipboard($(elem));
        $(clip.htmlBridge).qtip($.extend({}, qtipDefaults, {
          content: {
            text: 'Copy path to clipboard'
          },
          position: {
            my: 'top center',
            at: 'bottom center'
          }
        }));
        $(clip.htmlBridge).addClass('hover-fix');
        return clip.on('complete', function(client, args) {
          var t, wrapper;
          t = $('#' + attrs.tcCopyPathToClipboard);
          t.highlightInputSelectionRange(0, (t.val().length));
          t.qtip('disable', true);
          wrapper = $('.editor-path');
          wrapper.qtip($.extend({}, qtipDefaults, {
            content: {
              text: 'Copied to clipboard!'
            }
          }));
          wrapper.qtip('show');
          return $timeout((function() {
            wrapper.qtip('hide');
            wrapper.qtip('disable');
            return t.qtip('disable', false);
          }), 2000);
        });
      }
    };
  });

  module.directive('tcServerValidated', function() {
    return {
      require: 'ngModel',
      link: function(scope, elem, attrs, modelCtrl) {
        return modelCtrl.$viewChangeListeners.push(function() {
          return modelCtrl.$setValidity('server', true);
        });
      }
    };
  });

  module.directive("tcDebounce", function($timeout) {
    return {
      restrict: "A",
      require: "ngModel",
      priority: 99,
      link: function(scope, elm, attr, ngModelCtrl) {
        var debounce;
        if (attr.type === "radio" || attr.type === "checkbox") {
          return;
        }
        elm.unbind("input");
        debounce = void 0;
        elm.bind("input", function() {
          $timeout.cancel(debounce);
          return debounce = $timeout(function() {
            return scope.$apply(function() {
              return ngModelCtrl.$setViewValue(elm.val());
            });
          }, 500);
        });
        return elm.bind("blur", function() {
          return scope.$apply(function() {
            return ngModelCtrl.$setViewValue(elm.val());
          });
        });
      }
    };
  });

  module.directive('tcDropdown', function($timeout) {
    return {
      restrict: 'E',
      transclude: true,
      replace: true,
      template: '<div> <form> <input type="text" ng-model="term" ng-change="query()" autocomplete="off" /> </form> <div ng-transclude></div> </div>',
      scope: {
        search: '&',
        select: '&',
        items: '=',
        term: '='
      },
      controller: function($scope) {
        $scope.items = [];
        $scope.hide = false;
        return this.activate = function(item) {
          return $scope.active = item;
        };
      }
    };
  });

  hashStringToColour = function(s) {
    var hue, rgb;
    if (s === 'http://vocab.jncc.gov.uk/jncc-broad-category') {
      return 'rgb(38,110,217)';
    } else {
      hue = Math.abs(s.hashCode() % 99) * 0.01;
      rgb = hslToRgb(hue, 0.7, 0.5);
      return 'rgb(' + rgb[0].toFixed(0) + ',' + rgb[1].toFixed(0) + ',' + rgb[2].toFixed(0) + ')';
    }
  };


  /*
  Converts an HSL color value to RGB. Conversion formula
  adapted from http://en.wikipedia.org/wiki/HSL_color_space.
  Assumes h, s, and l are contained in the set [0, 1] and
  returns r, g, and b in the set [0, 255].
  
  @param   Number  h       The hue
  @param   Number  s       The saturation
  @param   Number  l       The lightness
  @return  Array           The RGB representation
   */

  hslToRgb = function(h, s, l) {
    var b, g, hue2rgb, p, q, r;
    r = void 0;
    g = void 0;
    b = void 0;
    if (s === 0) {
      r = g = b = l;
    } else {
      hue2rgb = function(p, q, t) {
        if (t < 0) {
          t += 1;
        }
        if (t > 1) {
          t -= 1;
        }
        if (t < 1 / 6) {
          return p + (q - p) * 6 * t;
        }
        if (t < 1 / 2) {
          return q;
        }
        if (t < 2 / 3) {
          return p + (q - p) * (2 / 3 - t) * 6;
        }
        return p;
      };
      q = (l < 0.5 ? l * (1 + s) : l + s - l * s);
      p = 2 * l - q;
      r = hue2rgb(p, q, h + 1 / 3);
      g = hue2rgb(p, q, h);
      b = hue2rgb(p, q, h - 1 / 3);
    }
    return [r * 255, g * 255, b * 255];
  };

}).call(this);

//# sourceMappingURL=directives.js.map
