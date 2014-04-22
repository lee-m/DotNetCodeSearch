/*global angular,alert*/

//Create the main module and Elasticsearch client
var codeSearchApp = angular.module('codeSearch', ['ngSanitize', 'ui.bootstrap']);

//Main controller
codeSearchApp.controller('CodeSearchController', function ($scope, $http, $filter) {

  'use strict';

  /**
   * Object to hold changeset search params entered by the user. The fields of this
   * object are bound to the various fields in the UI
   * @type {Object}
   */
  $scope.changesetSearchParams = {
    repository: '',
    branch: '',
    message: '',
    author: '',
    numResults: 25
  };

  /**
   * 'Template' search object for changesets which will highlight the hits in the message
   */
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

  /**
   * Performs a search of the changesets index using the search parameters entered by the user.
   */
  $scope.searchChangesetsButtonClicked = function () {

    //Make a copy of the template query object to fill out with the search params
    var newTemplate = angular.copy($scope.changesetSearchTemplate);
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

  $scope.searchContentsButtonClicked = function () {
    alert("Search contents");
  }

  /**
   * Callback when query execution fails to ask Elasticsearch for an explanation of why it failed
   * so that a better error message can be shown to the user.
   * @param  {Object} searchParams A JSON object representing the query which failed.
   */
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

  /**
   * Checks if a particular field has a hit highlight.
   * @param  {Object}  hit The hit object returned by Elasticsearch containing the highlighting.
   * @param  {[type]}  fieldName The name of the field to check for any highlighting fragments.
   * @return {Boolean} True if a highlight fragment exist, otherwise false.
   */
  $scope.hasSearchHighlight = function (hit, fieldName) {
    return hit.highlight.hasOwnProperty(fieldName);
  };

  /**
   * Queries the Elasticsearch server for a list of suggestions for a field based on the first
   * few characters of the field value.
   * @param  {string} partialVal The partial value which the user has typed so far.
   * @param  {string} fieldName The name of the field to get suggestions for.
   * @param {string} indexName Name of the index to query for suggestions.
   * @return {Objec} An Angular $http promise which will return the list of suggested field values.
   */
  $scope.getSuggestionsForField = function (partialVal, fieldName, indexName) {

    var params = {
      field_suggestions: {
        text: partialVal,
        completion: {
          field: fieldName
        }
      }
    };

    return $http.post('http://localhost:9200/' + indexName + '/_suggest', $filter('json')(params)).then(function (res) {

      var suggestions = [];

      angular.forEach(res.data.field_suggestions[0].options, function (item) {
        suggestions.push(item.text);
      });

      return suggestions;

    });
  };

});