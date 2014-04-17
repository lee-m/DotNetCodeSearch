/*global angular*/

//Create the main module and Elasticsearch client
var codeSearchApp = angular.module('codeSearch', ['ngSanitize']);

//Main controller
codeSearchApp.controller('CodeSearchController', function ($scope, $http, $filter) {

  'use strict';

  //Set to true if attempting to issue a query results in an error
  $scope.searchError = false;

  //Object to hold changeset search params entered by the user. The fields of thi
  //object are bound to the various fields in the UI
  $scope.changesetSearchParams = {
    repository: '',
    branch: '',
    message: '',
    author: '',
    numResults: 25
  };

  //'Template' search object for changesets which will highlight the hits in the message
  $scope.changesetSearchTemplate = {
    query: {
      query_string: {
        query: '',
        default_operator: "OR"
      }
    },
    highlight: {
      pre_tags: ["<font color=\"#FF3333\"><em>"],
      post_tags: ["</em></font>"],
      fields: {
        author: {},
        branch: {},
        message: {},
        repository: {}
      }
    },
    size: 0
  };

  $scope.searchButtonClicked = function () {

    var newTemplate = $scope.changesetSearchTemplate;
    var fieldQueries = [];

    if ($scope.changesetSearchParams.author.length > 0) {
      fieldQueries.push('author: ' + $scope.changesetSearchParams.author);
    }

    if ($scope.changesetSearchParams.branch.length > 0) {
      fieldQueries.push('branch: ' + $scope.changesetSearchParams.branch);
    }

    if ($scope.changesetSearchParams.message.length > 0) {
      fieldQueries.push('message: ' + $scope.changesetSearchParams.message);
    }

    if ($scope.changesetSearchParams.repository.length > 0) {
      fieldQueries.push('repository: ' + $scope.changesetSearchParams.repository);
    }

    newTemplate.query.query_string.query = fieldQueries.join(' ');
    newTemplate.size = $scope.changesetSearchParams.numResults;

    $scope.debug = $filter('json')(newTemplate);

    $http.post('http://localhost:9200/_search', $filter('json')(newTemplate))
      .success(function (data, status, headers, config) {
        $scope.searchResults = data.hits;
      })
      .error(function (data, status, headers, config) {
        $scope.searchError = true;
      });
  };

  $scope.hasSearchHighlight = function (hit, fieldName) {
    return hit.highlight.hasOwnProperty(fieldName);
  };

});

