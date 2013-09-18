

angular.module('app.controllers').controller 'SearchController', ($scope, $location) ->

    alert 'hi'
    $scope.query = { q: '' }

    $scope.$watch 'query.q', (q) -> $location.search('q', q)

    $scope.$watch(
        () -> $location.search()['q']
        () -> $scope.query.q = $location.search()['q'] || ''
    )

