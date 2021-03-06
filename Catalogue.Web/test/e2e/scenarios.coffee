﻿
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
# possibly do something like this to force the blur event speed up the tests??
#        element('#queryq').query( (elements, done) ->
#            elements.trigger 'blur'
#            done())
    
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

describe 'when viewing a read-only record', ->

    beforeEach ->
        browser().navigateTo '../../app/#/editor/b65d2914-cbac-4230-a7f3-08d13eea1e92'

    it 'should open the record', ->
        expect(input('form.gemini.title').val()).toBe 'An example read-only record'
    it 'should not be editable', ->
        input('form.gemini.title').enter 'mwaa ha ha'
        expect(element('.btn-danger:visible').count()).toBe 0 # erm, assuming the only button with this class...
        
describe 'can edit top-copy status', ->

    beforeEach ->
        browser().navigateTo '../../app/#/editor/94f2c217-2e45-42be-8b48-c5075401e508'

    it 'should start as not top-copy', ->
        expect(element('''.editor-top-copy button:contains('No')''').attr('class')).toContain 'btn-primary'

    it 'should update to top-copy', ->
        using '.editor-top-copy'
        element('''button:contains('Yes')''').click();
        expect(element('''button:contains('Yes')''').attr('class')).toContain 'btn-primary'
        expect(element('''button:contains('No')''').attr('class')).toContain 'btn-default'

describe 'can edit security level', ->

    beforeEach ->
        browser().navigateTo '../../app/#/editor/679434f5-baab-47b9-98e4-81c8e3a1a6f9'

    it 'should start as "open"', ->
        expect(element('.editor-security button').attr('class')).toContain 'btn-default'

    it 'should update to restricted', ->
        using '.editor-security'
        # this isn't right - i think it's that the using is not really scoping it
        #element('button').click();
        element('''button:contains('Open')''').click();
        expect(element('button').attr('class')).toContain 'btn-warning'
        # todo...





        