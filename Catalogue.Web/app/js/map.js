(function() {
  var heightForFullHeight, module;

  module = angular.module('app.map');

  heightForFullHeight = function($window, elem) {
    var elemTop, viewTop;
    viewTop = angular.element($window).innerHeight();
    elemTop = angular.element(elem).offset().top;
    return (viewTop - elemTop - 50) + 'px';
  };

  module.directive('tcSearchMap', function($window, $location, $anchorScroll) {
    return {
      link: function(scope, elem, attrs) {
        var group, hilite, map, normal, query;
        elem.css('height', heightForFullHeight($window, elem));
        map = L.map(elem[0]);
        L.tileLayer('https://{s}.tiles.mapbox.com/v4/petmon.lp99j25j/{z}/{x}/{y}.png?access_token=pk.eyJ1IjoicGV0bW9uIiwiYSI6ImdjaXJLTEEifQ.cLlYNK1-bfT0Vv4xUHhDBA', {
          maxZoom: 18,
          attribution: 'Map data &copy; <a href="http://openstreetmap.org">OpenStreetMap</a> contributors, ' + '<a href="http://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, ' + 'Imagery © <a href="http://mapbox.com">Mapbox</a>',
          id: 'petmon.lp99j25j'
        }).addTo(map);
        group = L.layerGroup().addTo(map);
        query = {};
        normal = {
          color: '#222',
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
          query = (function() {
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
                      return scope.highlighted.id = r.id;
                    });
                  });
                  rect.on('click', function() {
                    return scope.$apply(function() {
                      scope.highlighted.id = r.id;
                      $location.hash(r.id);
                      return $anchorScroll();
                    });
                  });
                  return {
                    id: r.id,
                    bounds: bounds,
                    rect: rect
                  };
                })(r));
              }
            }
            return _results;
          })();
          group.clearLayers();
          for (_i = 0, _len = query.length; _i < _len; _i++) {
            x = query[_i];
            group.addLayer(x.rect);
          }
          if (query.length > 0) {
            scope.highlighted.id = query[0].id;
            return map.fitBounds((function() {
              var _j, _len1, _results;
              _results = [];
              for (_j = 0, _len1 = query.length; _j < _len1; _j++) {
                x = query[_j];
                _results.push(x.bounds);
              }
              return _results;
            })(), {
              padding: [5, 5]
            });
          }
        });
        return scope.$watch('highlighted.id', function(newer, older) {
          var x, _ref, _ref1;
          if ((_ref = ((function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = query.length; _i < _len; _i++) {
              x = query[_i];
              if (x.id === older) {
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
            for (_i = 0, _len = query.length; _i < _len; _i++) {
              x = query[_i];
              if (x.id === newer) {
                _results.push(x.rect);
              }
            }
            return _results;
          })())[0]) != null ? _ref1.setStyle(hilite) : void 0;
        });
      }
    };
  });

  module.directive('tcBlah', function($window) {
    return {
      link: function(scope, elem, attrs) {
        var win;
        win = angular.element($window);
        return win.bind('scroll', function() {
          var el, query;
          query = (function() {
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
          if (query.length > 0) {
            return scope.$apply(function() {
              return scope.highlighted.id = query[0].id;
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
