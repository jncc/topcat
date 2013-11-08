
# http://docs.angularjs.org/guide/dev_guide.e2e-testing

# read this: http://www.yearofmoo.com/2013/01/full-spectrum-testing-with-angularjs-and-karma.html

describe 'when starting the app', ->

    beforeEach ->
        browser().navigateTo '../../app/index.html'

    it 'should redirect to /', ->
        expect(browser().location().path()).toBe '/'

    it 'should have empty query parameter values', ->
        for v in browser().location().search()
            do (v) -> expect(v).toBe ''

describe 'when searching for "underwater"', ->
    
    beforeEach ->
        input('query.q').enter 'underwater'
    
    it 'should return 3 results', ->
        expect(repeater('.search-result').count()).toBe 3 

    it 'should update search querystring correctly', ->
        expect( browser().location().search() ).toEqual { q: 'underwater' }

describe 'when deleting all search box content', ->
    
    beforeEach ->
        input('query.q').enter ''

    it 'should show no results', ->
        expect(repeater('.search-result').count()).toBe 0 

describe 'search results specifications', ->
    
    it 'can search for partial words', ->
        input('query.q').enter 'bio'
        expect(element('.search-result').text()).toContain 'biota'
        expect(element('.search-result').text()).toContain 'biotopes'
        expect(repeater('.search-result').count()).toBeGreaterThan 5 

    it 'can search for integers', ->
        input('query.q').enter '2003'
        expect(element('.search-result').text()).toContain '2003'
        expect(repeater('.search-result').count()).toBeGreaterThan 5 

    it 'can search for variations of stem', ->
        input('query.q').enter 'study'
        expect(element('.search-result').text()).toContain 'study'
        expect(element('.search-result').text()).toContain 'studied'
        expect(element('.search-result').text()).toContain 'studies'

    it 'title should be more important than abstract', ->
        input('query.q').enter 'bird'
        # there's one record with 'bird' in the title and two with it in the abstract;
        # the title one should appear top
        # todo
        #expect(repeater('.search-result').row(0)).toContain 'Bird' 


