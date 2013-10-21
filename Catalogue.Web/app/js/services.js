(function() {
  var hslToRgb, module;

  module = angular.module('app.services');

  module.factory('Record', [
    '$resource', function($resource) {
      return $resource('../api/records/:id', {
        id: '@id'
      });
    }
  ]);

  module.factory('RecordLoader', function(Record, $route, $q) {
    return function() {
      var d;
      d = $q.defer();
      Record.get({
        id: $route.current.params.recordId
      }, function(record) {
        return d.resolve(record, function() {
          return d.reject('Unable to fetch record ' + $route.current.params.recordId);
        });
      });
      return d.promise;
    };
  });

  module.factory('misc', function() {
    return {
      hashStringToColor: function(s) {
        var hue, rgb;
        if (!s) {
          return '#fff';
        } else {
          hue = Math.abs(s.hashCode() % 99) * 0.01;
          rgb = hslToRgb(hue, 0.7, 0.5);
          return 'rgb(' + rgb[0].toFixed(0) + ',' + rgb[1].toFixed(0) + ',' + rgb[2].toFixed(0) + ')';
        }
      }
    };
  });

  module.factory('defaults', function() {
    return {
      name: 'John Smit',
      line1: '123 Main St.',
      city: 'Anytown',
      state: 'AA',
      zip: '12345',
      phone: '1(234) 555-1212'
    };
  });

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
