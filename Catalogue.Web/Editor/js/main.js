(function() {
  var module;

  module = angular.module('editor', ['$strap.directives']);

  module.factory('defaults', function() {
    return {
      name: 'John Smit',
      line1: '123 Main St.',
      city: 'Anytown',
      state: 'AA',
      zip: '12345',
      phone: '1(234) 555-1212'
    };
  });

  module.directive('placeholder', function() {
    return {
      restrict: 'A',
      link: function(scope, element, attrs) {
        return $(element).placeholder();
      }
    };
  });

  module.directive('autosize', function() {
    return {
      restrict: 'A',
      link: function(scope, element, attrs) {
        return $(element).autosize();
      }
    };
  });

  module.directive('servervalidation', function($http) {
    return {
      require: 'ngModel',
      link: function(scope, elem, attrs, ctrl) {
        return elem.on('blur', function(e) {
          alert('hi');
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
      restrict: 'A',
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

  module.directive('locationclipboard', function() {
    return {
      restrict: 'A',
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

  $.fn.highlightInputSelectionRange = function(start, end) {
    return this.each(function() {
      var range;
      if (this.setSelectionRange) {
        this.focus();
        return this.setSelectionRange(start, end);
      } else if (this.createTextRange) {
        range = this.createTextRange();
        range.collapse(true);
        range.moveEnd('character', end);
        range.moveStart('character', start);
        return range.select();
      }
    });
  };

}).call(this);
