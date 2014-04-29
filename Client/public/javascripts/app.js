/*global angular,alert*/

//Create the main module and Elasticsearch client
var codeSearchApp = angular.module('codeSearch', ['ngSanitize', 'ui.bootstrap', 'hljs' ]);

//Main controller
codeSearchApp.controller('CodeSearchController', function ($scope, $http, $filter) {

  'use strict';

  /**
   * Object to hold changeset search filters entered by the user. The fields of this
   * object are bound to the corresponding input fields in the UI
   * @type {Object}
   */
  $scope.changesetSearchFilters = {
    repository: '',
    branch: '',
    author: ''
  };

  /**
   * Object to hold file contents search filters entered by the user. The fields of this
   * object are bound to the corresponding input fields in the UI
   * @type {Object}
   */
  $scope.contentsSearchFilters = {
    repository: '',
    branch: ''
  };

  /**
   * 'Template' search object for changesets which will highlight the hits in the message
   */
  $scope.changesetSearchTemplate = {
    size: '25',
    highlight: {
      pre_tags: ['<font color="#FF3333"><em>'],
      post_tags: ['</em></font>'],
      order: 'score',
      fields: {
        message: {
          matched_fields: ['message', 'message.plain'],
          number_of_fragments: 0,
          type: 'fvh'
        }
      }
    },
    query: {
      filtered: {
        query: {
          bool: {
            must: [
              {
                match: {
                  message: {
                    operator: 'and',
                    query: ''
                  }
                }
              }
            ],
            should: [
              {
                match_phrase: {
                  'message.plain': ''
                }
              }
            ]
          }
        },
        filter: {
          bool: {
            must: [
            ]
          }
        }
      }
    }
  };

  /**
   * Performs a search of the changesets index using the search parameters entered by the user.
   */
  $scope.searchChangesetsButtonClicked = function () {

    //Make a copy of the template query object to fill out with the search params
    var newTemplate = angular.copy($scope.changesetSearchTemplate);

    if ($scope.changesetSearchFilters.author.length > 0) {

      newTemplate.query.filtered.query.bool.must.push({
        term: {
          author: $scope.changesetSearchFilters.author
        }
      });

    }

    if ($scope.changesetSearchFilters.branch.length > 0) {

      newTemplate.query.filtered.query.bool.must.push({
        term: {
          branch: $scope.changesetSearchFilters.branch
        }
      });

    }

    if ($scope.changesetSearchFilters.repository.length > 0) {

      newTemplate.query.filtered.query.bool.must.push({
        term: {
          repository: $scope.changesetSearchFilters.repository
        }
      });

    }

    $http.post('http://localhost:9200/changesets/_search', angular.toJson(newTemplate))
      .success(function (data, status, headers, config) {

        $scope.searchResults = {
          changesets: {
            hits: data.hits.hits
          },
          totalResults: data.hits.total,
          returnedResults: data.hits.hits.length
        };

      }).error(function (data, status, headers, config) {
        //TODO: show some sort of error
      });
  };

  $scope.searchContentsButtonClicked = function () {

    //TODO: populate this from the UI
    var queryParams = {
      query: {
        bool: {
          must: [{
            match: {
              contents: "AnimalRepeatRow"
            }
          }]
        }
      },
      fields: ["branch", "file_name", "repository"],
      highlight: {
        fields: {
          order: "score",
          contents: {
            fragment_size: 200
          }
        }
      }
    };

    $http.post('http://localhost:9200/file_contents/_search', queryParams)
      .success(function (data, status, headers, config) {

        $scope.searchResults = {
          file_contents: {
            hits: data.hits.hits
          },
          totalResults: data.hits.total,
          returnedResults: data.hits.hits.length
        };

      }).error(function (data, status, headers, config) {
        //TODO: show some sort of error
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