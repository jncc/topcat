angular.module('app.controllers',['ui.bootstrap']).controller 'MainController',

    ($scope, $rootScope, $timeout, $cookies, Account) ->

        # implement busy spinner feature
        busyCount = 0
        $scope.busy =
            start: -> busyCount = busyCount + 1
            stop : -> busyCount = busyCount - 1
            value: -> busyCount > 0

        # implement notifications feature
        notifications = []
        $scope.notifications =
            current: notifications
            add: (message) ->
                n = message: message
                notifications.push n
                remove = -> notifications.splice ($.inArray n, notifications)
                $timeout remove, 4000

        # every page needs a user
        Account.then (user) -> $scope.user = user
        
        # horrid hack to ensure qtips hide when url (location) changes
        # don't know why qtip can't cope with this naturally but it leaves
        # tooltips hanging visible when the element has gone
        $rootScope.$on '$locationChangeStart', -> $('.qtip').qtip 'hide'
        
        probablyChrome = navigator.userAgent.toLowerCase().indexOf('chrome') isnt -1
        if not probablyChrome
            
            $scope.notifications.add 'Topcat works best on Chrome!'
        
# todo move this! used for hightlighting suggested keywords
angular.module('filters').filter 'highlight', ($sce) -> 
    (text, q) ->
        regex = new RegExp '(' + q + ')', 'gi'
        if q
            text = text.replace regex, '<b>$1</b>'
        $sce.trustAsHtml text
