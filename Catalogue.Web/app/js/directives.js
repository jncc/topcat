(function() {
  var module, qtipDefaults;

  module = angular.module('app.directives');

  module.directive('tcSearchMap', function($location, $anchorScroll) {
    return {
      link: function(scope, elem, attrs) {
        var group, hilite, map, normal, xs;
        map = L.map(elem[0]);
        L.tileLayer('https://{s}.tiles.mapbox.com/v4/petmon.lp99j25j/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoicGV0bW9uIiwiYSI6ImdjaXJLTEEifQ.cLlYNK1-bfT0Vv4xUHhDBA', {
          maxZoom: 18,
          attribution: 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' + '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' + 'Imagery © <a href="http://mapbox.com">Mapbox</a>',
          id: 'petmon.lp99j25j'
        }).addTo(map);
        group = L.layerGroup().addTo(map);
        xs = {};
        normal = {
          color: '#444',
          fillOpacity: 0.2,
          weight: 1
        };
        hilite = {
          color: 'rgb(217,38,103)',
          fillOpacity: 0.6,
          weight: 1
        };
        scope.$watch('result.results', function(results) {
          var r, x, _i, _len;
          xs = (function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = results.length; _i < _len; _i++) {
              r = results[_i];
              if (r.box) {
                _results.push((function(r) {
                  var bounds, rect;
                  bounds = [[r.box.south, r.box.west], [r.box.north, r.box.east]];
                  rect = L.rectangle(bounds, normal);
                  rect.on('mouseover', function() {
                    return scope.$apply(function() {
                      return scope.highlighted.result = r;
                    });
                  });
                  rect.on('click', function() {
                    return scope.$apply(function() {
                      scope.highlighted.result = r;
                      $location.hash(r.id);
                      return $anchorScroll();
                    });
                  });
                  return {
                    result: r,
                    bounds: bounds,
                    rect: rect
                  };
                })(r));
              }
            }
            return _results;
          })();
          group.clearLayers();
          for (_i = 0, _len = xs.length; _i < _len; _i++) {
            x = xs[_i];
            group.addLayer(x.rect);
          }
          if (xs.length > 0) {
            return map.fitBounds((function() {
              var _j, _len1, _results;
              _results = [];
              for (_j = 0, _len1 = xs.length; _j < _len1; _j++) {
                x = xs[_j];
                _results.push(x.bounds);
              }
              return _results;
            })());
          }
        });
        return scope.$watch('highlighted.result', function(newer, older) {
          var x, _ref, _ref1;
          if ((_ref = ((function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = xs.length; _i < _len; _i++) {
              x = xs[_i];
              if (x.result === older) {
                _results.push(x.rect);
              }
            }
            return _results;
          })())[0]) != null) {
            _ref.setStyle(normal);
          }
          return (_ref1 = ((function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = xs.length; _i < _len; _i++) {
              x = xs[_i];
              if (x.result === newer) {
                _results.push(x.rect);
              }
            }
            return _results;
          })())[0]) != null ? _ref1.setStyle(hilite) : void 0;
        });
      }
    };
  });

  module.directive('tcSearchHighlightScroller', function($window) {
    return {
      link: function(scope, elem, attrs) {
        return scope.$watch('highlighted.scroll', function(newer, older) {});
      }
    };
  });

  module.directive('tcStickToTop', function($window, $timeout) {
    return {
      link: function(scope, elem, attrs) {
        var f, getPositions, win;
        win = angular.element($window);
        getPositions = function() {
          return {
            v: win.scrollTop(),
            e: elem.offset().top,
            w: elem.width()
          };
        };
        f = function() {
          var initial;
          initial = getPositions();
          return win.bind('scroll', function() {
            var current;
            current = getPositions();
            if (current.v > current.e) {
              elem.addClass('stick-to-top');
              return elem.css('width', initial.w);
            } else if (current.v < initial.e) {
              elem.removeClass('stick-to-top');
              return elem.css('width', '');
            }
          });
        };
        return $timeout(f, 100);
      }
    };
  });

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

  module.directive('tcFocus', function($timeout) {
    return {
      link: function(scope, elem, attrs) {
        return $timeout((function() {
          return elem[0].focus();
        }), 100);
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

  module.directive('tcTag', function(colourHasher) {
    return {
      link: function(scope, elem, attrs) {
        var colour, edge;
        elem.addClass('tag');
        edge = scope.k.vocab ? (colour = colourHasher.hashStringToColour(scope.k.vocab), angular.element('<span class="vocabful-colour-edge" style="background-color:' + colour + '">&nbsp;</span>')) : angular.element('<span class="vocabless-colour-edge"></span>');
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

  module.directive('tcBindScrollPosition', function($parse, $timeout) {
    return {
      link: function(scope, elem, attrs) {
        var getter, value;
        getter = $parse(attrs.tcBindScrollPosition);
        value = getter(scope);
        if (value) {
          $timeout((function() {
            return elem[0].scrollTop = value;
          }), 0);
        }
        return elem.bind('scroll', function() {
          var setter;
          setter = getter.assign;
          setter(scope, elem[0].scrollTop);
          return scope.$apply();
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

}).call(this);

//# sourceMappingURL=directives.js.map
