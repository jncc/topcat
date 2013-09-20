
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

describe 'when searching for "result"', ->
    
    beforeEach ->
        input('query.q').enter 'result'
    
    it 'should return 3 results', ->
        expect(repeater('.search-result').count()).toBe 3 

    it 'should update search querystring correctly', ->
        expect( browser().location().search() ).toEqual { q: 'result' }
