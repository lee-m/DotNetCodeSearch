/*global angular,alert*/

//Create the main module and Elasticsearch client
var codeSearchApp = angular.module('codeSearch', ['ngSanitize', 'ui.bootstrap']);

//Main controller
codeSearchApp.controller('CodeSearchController', function ($scope, $http, $filter) {

  'use strict';

  //Object to hold changeset search params entered by the user. The fields of this
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
        default_operator: "AND",
        fields: ["repository", "branch", "message", "message.plain^10", "author"]
      }
    },
    highlight: {
      pre_tags: ["<font color=\"#FF3333\"><em>"],
      post_tags: ["</em></font>"],
      order: "score",
      fields: {
        author: {},
        branch: {},
        message: {
          matched_fields: ["message", "message.plain"],
          number_of_fragments: 0,
          type: "fvh"
        },
        repository: {}
      }
    },
    size: 0
  };

  $scope.searchButtonClicked = function () {

    //Make a copy of the template query object to fill out with the search params
    var newTemplate = angular.fromJson(angular.toJson($scope.changesetSearchTemplate));
    var fieldQueries = [];
    var paramsJSON = '';

    if ($scope.changesetSearchParams.author.length > 0) {
      fieldQueries.push('author: ' + $scope.changesetSearchParams.author);
    }

    if ($scope.changesetSearchParams.branch.length > 0) {
      fieldQueries.push('branch: ' + $scope.changesetSearchParams.branch);
    }

    //Changeset messages are indexed in two different ways so need to include both copies in the query
    if ($scope.changesetSearchParams.message.length > 0) {
      fieldQueries.push('message.plain: ' + $scope.changesetSearchParams.message);
      fieldQueries.push('message: ' + $scope.changesetSearchParams.message);
    }

    if ($scope.changesetSearchParams.repository.length > 0) {
      fieldQueries.push('repository: ' + $scope.changesetSearchParams.repository);
    }

    newTemplate.query.query_string.query = fieldQueries.join(' ');
    newTemplate.size = $scope.changesetSearchParams.numResults;

    paramsJSON = angular.toJson(newTemplate);
    $scope.debug = paramsJSON;

    $http.post('http://localhost:9200/_search', paramsJSON)
      .success(function (data, status, headers, config) {
        $scope.searchResults = data.hits;
      })
      .error(function (data, status, headers, config) {
        $scope.queryFailedCallback(paramsJSON);
      });
  };

  $scope.queryFailedCallback = function (searchParams) {

    //Executing the query choked so try and get an explanation of why
    $http.post('http://localhost:9200/_validate/query?explain', searchParams)
      .success(function (data, status, headers, config) {
        alert(data.explanations[0].error);
      })
      .error(function (data, status, headers, config) {
        alert('Unknown error execting query. Please check search parameters and try again.');
      });
  };

  $scope.hasSearchHighlight = function (hit, fieldName) {
    return hit.highlight.hasOwnProperty(fieldName);
  };

  $scope.getSuggestionsForField = function (partialVal, fieldName) {

    var params = {
      field_suggestions: {
        text: partialVal,
        completion: {
          field: fieldName
        }
      }
    };

    return $http.post('http://localhost:9200/_suggest', $filter('json')(params)).then(function (res) {

      var suggestions = [];

      angular.forEach(res.data.field_suggestions[0].options, function (item) {
        suggestions.push(item.text);
      });

      return suggestions;

    });
  };

});