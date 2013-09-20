(function() {

  describe('when starting the app', function() {
    beforeEach(function() {
      return browser().navigateTo('../../app/index.html');
    });
    it('should redirect to /', function() {
      return expect(browser().location().path()).toBe('/');
    });
    return it('should have empty query parameter values', function() {
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
    it('should return 3 results', function() {
      return expect(repeater('.search-result').count()).toBe(3);
    });
    return it('should update search querystring correctly', function() {
      return expect(browser().location().search()).toEqual({
        q: 'result'
      });
    });
  });

  describe('when deleting all search box content', function() {
    beforeEach(function() {
      input('query.q').enter('result');
      input('query.q').enter('resul');
      input('query.q').enter('resu');
      input('query.q').enter('res');
      input('query.q').enter('re');
      input('query.q').enter('r');
      return input('query.q').enter('');
    });
    return it('should show no results', function() {
      return expect(repeater('.search-result').count()).toBe(0);
    });
  });

}).call(this);
