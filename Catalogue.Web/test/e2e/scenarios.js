﻿(function() {

  describe('when starting the app', function() {
    beforeEach(function() {
      return browser().navigateTo('../../app/index.html');
    });
    it('should redirect to /', function() {
      return expect(browser().location().path()).toBe('/');
    });
    return it('query parameter values should be empty ', function() {
      var v, _i, _len, _ref, _results;
      _ref = browser().location().search();
      _results = [];
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        v = _ref[_i];
        _results.push((function(v) {
          return expect(v).toBe('');
        })(v));
      }
      return _results;
    });
  });

  describe('when searching for "result"', function() {
    beforeEach(function() {
      return input('query.q').enter('result');
    });
    return it('should return 3 results', function() {
      return expect(repeater('.search-result').count()).toEqual(3);
    });
  });

}).call(this);
