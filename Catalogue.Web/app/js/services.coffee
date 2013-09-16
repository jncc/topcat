
module = angular.module 'app.services'

module.factory 'Record', ['$resource', ($resource) ->
    $resource '../api/records/:id', id: '@id' ]

module.factory 'RecordLoader', (Record, $route, $q) ->
    () ->
        d = $q.defer()
        Record.get
            id: $route.current.params.recordId,
            (record) -> d.resolve record,
            ()       -> d.reject 'Unable to fetch record ' + $route.current.params.recordId
        d.promise

module.factory 'defaults', () ->
    name: 'John Smit',
    line1: '123 Main St.',
    city: 'Anytown',
    state: 'AA',
    zip: '12345',
    phone: '1(234) 555-1212',


