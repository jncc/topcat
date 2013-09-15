(function() {
  var module;

  module = angular.module('app.directives');

  module.directive('placeholder', function() {
    return {
      link: function(scope, element, attrs) {
        return $(element).placeholder();
      }
    };
  });

  module.directive('autosize', function() {
    return {
      link: function(scope, element, attrs) {
        return $(element).autosize();
      }
    };
  });

  module.directive('tcBackButton', [
    '$window', function($window) {
      return {
        link: function(scope, elem, attrs) {
          return elem.on('click', function() {
            return $window.history.back();
          });
        }
      };
    }
  ]);

  module.directive('spinner', [
    '$rootScope', function($rootScope) {
      return {
        link: function(scope, elem, attrs) {
          elem.addClass('hide');
          $rootScope.$on('$routeChangeStart', function() {
            return elem.removeClass('hide');
          });
          return $rootScope.$on('$routeChangeSuccess', function() {
            return elem.addClass('hide');
          });
        }
      };
    }
  ]);

  module.directive('servervalidation', function($http) {
    return {
      require: 'ngModel',
      link: function(scope, elem, attrs, ctrl) {
        return elem.on('blur', function(e) {
          return scope.$apply(function() {
            return $http.post('../api/validator', {
              "value": elem.val()
            }).success(function(data) {
              return ctrl.$setValidity('myErrorKey', data.valid);
            });
          });
        });
      }
    };
  });

  module.directive('tooltip', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).tooltip({
          placement: 'auto',
          delay: {
            show: 500,
            hide: 100
          }
        });
      }
    };
  });

  module.directive('tcFocustip', function() {
    return {
      link: function(scope, elem, attrs) {
        return $(elem).tooltip({
          trigger: 'focus',
          placement: 'top',
          delay: {
            show: 0,
            hide: 100
          }
        });
      }
    };
  });

  module.directive('locationclipboard', function() {
    return {
      link: function(scope, elem, attrs) {
        var clip;
        clip = new ZeroClipboard($(elem));
        return clip.on('complete', function(client, args) {
          var l;
          l = $('#location');
          l.highlightInputSelectionRange(0, (l.val().length));
          l.tooltip({
            title: 'Copied to clipboard!',
            trigger: 'manual',
            container: 'body'
          });
          l.tooltip('show');
          return setTimeout((function() {
            return l.tooltip('hide');
          }), 2000);
        });
      }
    };
  });

}).call(this);
