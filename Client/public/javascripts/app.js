/*global angular*/

//Create the main module and Elasticsearch client
var codeSearchApp = angular.module('codeSearch', []);

//Main controller
codeSearchApp.controller('CodeSearchController', function ($scope, $http) {

  'use strict';

  $scope.data = 'default';

  $scope.searchButtonClicked = function() {

    $http({
        method: "GET",
        url: 'http://localhost:9200/_cluster/health?pretty=true',
      })
      .success(function (data, status, headers, config) {
        $scope.data = data;
      })
      .error(function (data, status, headers, config) {
        $scope.data = 'Error';
      });
  };

});

