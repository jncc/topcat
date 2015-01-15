module = angular.module 'app.services'

module.factory 'Account', ($http, $q) ->
    d = $q.defer()
    $http.get('../api/account').success (data) ->
        d.resolve data
    d.promise

module.factory 'Vocabulary', ($resource) ->
    $resource '../api/vocabularies/?id=:id', {},
    query: {method:'GET', params: {id: '@id'}}
    update: {method:'PUT', params: {id: '@id'}}
    
module.factory 'VocabLoader', (Vocabulary, $route, $q) ->
    () ->
        #console.log("vocabloader")
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

module.factory 'misc', ->
    hashStringToColor: (s) ->
        if !s
            '#444'
        else if s == 'http://vocab.jncc.gov.uk/jncc-broad-category'
            'rgb(38,110,217)' # special case to make this one look good
        else
            hue = Math.abs(s.hashCode() % 99) * 0.01
            rgb = hslToRgb(hue, 0.7, 0.5)
            'rgb(' + rgb[0].toFixed(0) + ',' + rgb[1].toFixed(0) + ',' + rgb[2].toFixed(0) + ')';


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

###
Converts an HSL color value to RGB. Conversion formula
adapted from http://en.wikipedia.org/wiki/HSL_color_space.
Assumes h, s, and l are contained in the set [0, 1] and
returns r, g, and b in the set [0, 255].

@param   Number  h       The hue
@param   Number  s       The saturation
@param   Number  l       The lightness
@return  Array           The RGB representation
###
hslToRgb = (h, s, l) ->
  r = undefined
  g = undefined
  b = undefined
  if s is 0
    r = g = b = l # achromatic
  else
    hue2rgb = (p, q, t) ->
      t += 1  if t < 0
      t -= 1  if t > 1
      return p + (q - p) * 6 * t  if t < 1 / 6
      return q  if t < 1 / 2
      return p + (q - p) * (2 / 3 - t) * 6  if t < 2 / 3
      p
    q = (if l < 0.5 then l * (1 + s) else l + s - l * s)
    p = 2 * l - q
    r = hue2rgb(p, q, h + 1 / 3)
    g = hue2rgb(p, q, h)
    b = hue2rgb(p, q, h - 1 / 3)
  [r * 255, g * 255, b * 255]

