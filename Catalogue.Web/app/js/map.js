(function() {
  var baseLayer, calculateBestHeightForMap, getBestPadding, hilite, makeTuple, module, normal, tuples;

  module = angular.module('app.map');

  calculateBestHeightForMap = function($window, elem) {
    var elemTop, viewTop;
    viewTop = angular.element($window).innerHeight();
    elemTop = angular.element(elem).offset().top;
    return (viewTop - elemTop - 10) + 'px';
  };

  baseLayer = L.tileLayer('https://{s}.tiles.mapbox.com/v4/petmon.lp99j25j/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoicGV0bW9uIiwiYSI6ImdjaXJLTEEifQ.cLlYNK1-bfT0Vv4xUHhDBA', {
    maxZoom: 18,
    attribution: 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' + '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' + 'Imagery © <a href="http://mapbox.com">Mapbox</a>',
    id: 'petmon.lp99j25j'
  });

  normal = {
    fillOpacity: 0.2,
    weight: 1,
    color: '#222'
  };

  hilite = {
    fillOpacity: 0.6,
    weight: 1,
    color: 'rgb(217,38,103)'
  };

  tuples = {};

  makeTuple = function(r, scope) {
    var bounds, rect;
    bounds = [[r.box.south, r.box.west], [r.box.north, r.box.east]];
    rect = L.rectangle(bounds, normal);
    rect.on('click', function() {
      return scope.$apply(function() {
        return scope.highlighted.result = r;
      });
    });
    return {
      r: r,
      bounds: bounds,
      rect: rect
    };
  };

  getBestPadding = function(tuples) {
    if (tuples.length === 1) {
      return {
        padding: [50, 50]
      };
    } else {
      return {
        padding: [5, 5]
      };
    }
  };

  module.directive('tcSearchMap', function($window, $location, $anchorScroll) {
    return {
      link: function(scope, elem, attrs) {
        var group, map;
        map = L.map(elem[0]);
        map.addLayer(baseLayer);
        group = L.layerGroup().addTo(map);
        scope.$watch('result.results', function(results) {
          var r, x, _i, _len;
          tuples = (function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = results.length; _i < _len; _i++) {
              r = results[_i];
              if (r.box.north) {
                _results.push((function(r) {
                  return makeTuple(r, scope);
                })(r));
              }
            }
            return _results;
          })();
          group.clearLayers();
          for (_i = 0, _len = tuples.length; _i < _len; _i++) {
            x = tuples[_i];
            group.addLayer(x.rect);
          }
          elem.css('height', calculateBestHeightForMap($window, elem));
          if (tuples.length > 0) {
            return scope.highlighted.result = tuples[0].r;
          }
        });
        map.on('zoomend', function() {
          return scope.$evalAsync(function() {
            return scope.highlighted.goto = null;
          });
        });
        scope.$watch('highlighted.result', function(newer, older) {
          var x, _ref, _ref1;
          scope.highlighted.goto = null;
          if (tuples.length) {
            map.fitBounds((function() {
              var _i, _len, _results;
              _results = [];
              for (_i = 0, _len = tuples.length; _i < _len; _i++) {
                x = tuples[_i];
                _results.push(x.bounds);
              }
              return _results;
            })(), getBestPadding(tuples));
          }
          if ((_ref = ((function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = tuples.length; _i < _len; _i++) {
              x = tuples[_i];
              if (x.r === older) {
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
            for (_i = 0, _len = tuples.length; _i < _len; _i++) {
              x = tuples[_i];
              if (x.r === newer) {
                _results.push(x.rect);
              }
            }
            return _results;
          })())[0]) != null ? _ref1.setStyle(hilite) : void 0;
        });
        return scope.$watch('highlighted.goto', function(newer) {
          var rectangle, x;
          rectangle = ((function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = tuples.length; _i < _len; _i++) {
              x = tuples[_i];
              if (x.r === newer) {
                _results.push(x.rect);
              }
            }
            return _results;
          })())[0];
          if (rectangle) {
            return map.fitBounds(rectangle, {
              padding: [50, 50]
            });
          }
        });
      }
    };
  });

  module.directive('tcSearchResultScrollHighlighter', function($window) {
    return {
      link: function(scope, elem, attrs) {
        var win;
        win = angular.element($window);
        return win.bind('scroll', function() {
          var el, q, result, x;
          q = (function() {
            var _i, _len, _ref, _results;
            _ref = elem.children();
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              el = _ref[_i];
              if (angular.element(el).offset().top > win.scrollTop()) {
                _results.push(el);
              }
            }
            return _results;
          })();
          result = ((function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = tuples.length; _i < _len; _i++) {
              x = tuples[_i];
              if (x.r.id === q[0].id) {
                _results.push(x.r);
              }
            }
            return _results;
          })())[0];
          if (result) {
            return scope.$apply(function() {
              return scope.highlighted.result = result;
            });
          }
        });
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

}).call(this);

//# sourceMappingURL=map.js.map
