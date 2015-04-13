module = angular.module 'app.filters'

module.filter('camelCaseFilter', () -> 
    (input) -> 
        input.charAt(0).toUpperCase() + input.substr(1).replace(/[A-Z]/g, ' $&')
)


# used for highlighting suggested keywords
module.filter 'highlight', ($sce) -> 
    (text, q) ->
        regex = new RegExp '(' + q + ')', 'gi'
        if q
            text = text.replace regex, '<b>$1</b>'
        $sce.trustAsHtml text
