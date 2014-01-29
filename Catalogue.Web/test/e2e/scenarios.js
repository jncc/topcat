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

  describe('when viewing a read-only record', function() {
    beforeEach(function() {
      return browser().navigateTo('../../app/#/editor/b65d2914-cbac-4230-a7f3-08d13eea1e92');
    });
    it('should open the record', function() {
      return expect(input('form.gemini.title').val()).toBe('An example read-only record');
    });
    return it('should not be editable', function() {
      input('form.gemini.title').enter('mwaa ha ha');
      return expect(element('.btn-danger:visible').count()).toBe(0);
    });
  });

  describe('can edit top-copy status', function() {
    beforeEach(function() {
      return browser().navigateTo('../../app/#/editor/94f2c217-2e45-42be-8b48-c5075401e508');
    });
    it('should start as not top-copy', function() {
      return expect(element('.editor-top-copy button:contains(\'No\')').attr('class')).toContain('btn-primary');
    });
    return it('should update to top-copy', function() {
      using('.editor-top-copy');
      element('button:contains(\'Yes\')').click();
      expect(element('button:contains(\'Yes\')').attr('class')).toContain('btn-primary');
      return expect(element('button:contains(\'No\')').attr('class')).toContain('btn-default');
    });
  });

  describe('can edit security level', function() {
    beforeEach(function() {
      return browser().navigateTo('../../app/#/editor/679434f5-baab-47b9-98e4-81c8e3a1a6f9');
    });
    it('should start as "open"', function() {
      return expect(element('.editor-security button').attr('class')).toContain('btn-default');
    });
    return it('should update to restricted', function() {
      using('.editor-security');
      element('button:contains(\'Open\')').click();
      return expect(element('button').attr('class')).toContain('btn-warning');
    });
  });

}).call(this);
