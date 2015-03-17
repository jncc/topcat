module = angular.module 'app.services'

module.factory 'Account', ($http, $q) ->
    d = $q.defer()
    $http.get('../api/account').success (data) ->
        d.resolve data
    d.promise

module.factory 'Vocabulary', ($resource) ->
    $resource '../api/vocabularies/:id', {},
    query: {method:'GET', params: {id: '@id'}}
    update: {method:'PUT', params: {id: '@id'}}
    
module.factory 'VocabLoader', (Vocabulary, $route, $q) ->
    () ->
        d = $q.defer()
        Vocabulary.get
            id: $route.current.params.vocabId,
            (vocabulary) -> d.resolve vocabulary,
            () -> d.reject 'Unable to fetch vocabulary ' + $route.current.params.vocabId
        d.promise

module.factory 'Record', ($resource) ->
    $resource '../api/records/:id', {},
    query: {method:'GET', params: {id: '@id'}}
    update: {method:'PUT', params: {id: '@id'}}
    
module.factory 'RecordLoader', (Record, $route, $q) ->
    () ->
        d = $q.defer()
        Record.get
            id: $route.current.params.recordId,
            (record) -> d.resolve record,
            () -> d.reject 'Unable to fetch record ' + $route.current.params.recordId
        d.promise

# just currently using this for a spike in SandboxController
module.factory 'Formats', ($http, $q) ->
    d = $q.defer()
    $http.get('../api/formats').success (data) ->
        d.resolve data
    d.promise


module.factory 'defaults', ->
    name: 'John Smit',
    line1: '123 Main St.',
    city: 'Anytown',
    state: 'AA',
    zip: '12345',
    phone: '1(234) 555-1212',

