(function() {
  angular.module('app.controllers').controller('ResultGridController', function($scope) {
    $scope.glyphColDef = {
      field: 'format.glyph',
      displayName: '',
      width: 20,
      enableColumnMenu: false,
      cellTemplate: '<div> <span class="dark glyphicon {{ row.entity.format.glyph }}"></span> </div>'
    };
    $scope.keywordColDef = {
      field: 'keywords',
      displayName: 'Keywords',
      width: 500,
      cellTemplate: '<div class="non-overflowing-cell cell-padding"> <span tc-tag ng-repeat="k in row.entity.keywords" tc-tip class="pointable"> {{ k.value }} </span> </div>'
    };
    $scope.titleColDef = {
      field: 'title',
      displayName: 'Title',
      width: 300,
      cellTemplate: '<div class="non-overflowing-cell cell-padding"> <a ng-href="#/editor/{{row.entity.id}}" ng-bind-html="row.entity.title"></span> </div>'
    };
    $scope.snippetColDef = {
      field: 'snippet',
      displayName: 'Snippet',
      width: 300,
      cellTemplate: '<div class="non-overflowing-cell cell-padding"> <span ng-bind-html="row.entity.snippet"></span> </div>'
    };
    $scope.redDateCol = {
      field: 'date',
      displayName: 'Ref Date',
      width: 100,
      cellTemplate: '<div class="cell-padding"><span>{{ row.entity.date.substring(0, 4) }}</span> <span tc-top-copy-icon ng-show="row.entity.topCopy"></span></div>'
    };
    $scope.gridColDefs = [
      $scope.glyphColDef, $scope.titleColDef, $scope.snippetColDef, $scope.redDateCol, {
        field: 'resourceType',
        displayName: 'Type',
        width: 100
      }, $scope.keywordColDef
    ];
    return $scope.gridOptions = {
      data: 'result.results',
      columnDefs: $scope.gridColDefs,
      enableGridMenu: true
    };
  });

}).call(this);
