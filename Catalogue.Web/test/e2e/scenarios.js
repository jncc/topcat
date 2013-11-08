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

  describe('when searching for "underwater"', function() {
    beforeEach(function() {
      return input('query.q').enter('underwater');
    });
    it('should return 3 results', function() {
      return expect(repeater('.search-result').count()).toBe(3);
    });
    return it('should update search querystring correctly', function() {
      return expect(browser().location().search()).toEqual({
        q: 'underwater'
      });
    });
  });

  describe('when deleting all search box content', function() {
    beforeEach(function() {
      return input('query.q').enter('');
    });
    return it('should show no results', function() {
      return expect(repeater('.search-result').count()).toBe(0);
    });
  });

  describe('search results specifications', function() {
    it('can search for partial words', function() {
      input('query.q').enter('bio');
      expect(element('.search-result').text()).toContain('biota');
      expect(element('.search-result').text()).toContain('biotopes');
      return expect(repeater('.search-result').count()).toBeGreaterThan(5);
    });
    it('can search for integers', function() {
      input('query.q').enter('2003');
      expect(element('.search-result').text()).toContain('2003');
      return expect(repeater('.search-result').count()).toBeGreaterThan(5);
    });
    it('can search for variations of stem', function() {
      input('query.q').enter('study');
      expect(element('.search-result').text()).toContain('study');
      expect(element('.search-result').text()).toContain('studied');
      return expect(element('.search-result').text()).toContain('studies');
    });
    return it('title should be more important than abstract', function() {
      return input('query.q').enter('bird');
    });
  });

}).call(this);
