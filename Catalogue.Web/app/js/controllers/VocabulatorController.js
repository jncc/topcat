(function() {
  angular.module('app.controllers').controller('VocabulatorController', function($scope, $http) {
    $scope.vocabs = {};
    $scope.selected = {};
    $scope.find = {};
    $http.get('../api/vocabularylist').success(function(result) {
      $scope.vocabs.all = result;
      return $scope.vocabs.filtered = result;
    });
    $scope.doFind = function(text) {
      var q, v;
      if ($scope.vocabs.all) {
        q = (function() {
          var _i, _len, _ref, _results;
          _ref = $scope.vocabs.all;
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            v = _ref[_i];
            if (v.name.toLowerCase().indexOf(text.toLowerCase()) !== -1) {
              _results.push(v);
            }
          }
          return _results;
        })();
        console.log(q);
        return $scope.vocabs.filtered = q;
      }
    };
    $scope.$watch('find.text', $scope.doFind, true);
    $scope.selected.keyword = {
      vocab: 'http://vocab.jncc.gov.uk/original-seabed-classification-system',
      value: 'MNCR'
    };
    return $scope.close = function() {
      return $scope.$close($scope.selected.keyword);
    };
  });

}).call(this);

//# sourceMappingURL=VocabulatorController.js.map
