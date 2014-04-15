/*global angular*/

//Create the main module and Elasticsearch client
var codeSearchApp = angular.module('codeSearch', ['ngSanitize']);

//Main controller
codeSearchApp.controller('CodeSearchController', function ($scope, $http, $filter) {

  'use strict';

  //Set to true if attempting to issue a query results in an error
  $scope.searchError = false;

  //'Template' search object for changesets which will highlight the hits in the message
  $scope.searchTemplate = {
    query: {
      simple_query_string: {
        query: "+critical +error",
        default_operator: "AND"
      },
    },
    highlight: {
      pre_tags: ["<strong>"],
      post_tags: ["</strong>"],
      fields: {
        message: {}
      },
    },
    size: 50,
  };

  $scope.searchButtonClicked = function() {

    $http.post('http://localhost:9200/_search', $filter('json')($scope.searchTemplate))
      .success(function (data, status, headers, config) {
        $scope.searchResults = data.hits;
      })
      .error(function (data, status, headers, config) {
        $scope.searchError = true;
      });

    /*
    $http({
        method: "GET",
        url: 'http://localhost:9200/_cluster/health?pretty=true',
      })
      .success(function (data, status, headers, config) {
        $scope.searchError = false;
      })
      .error(function (data, status, headers, config) {
        $scope.searchError = true;
      });*/
  };

});

