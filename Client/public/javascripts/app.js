/*global angular*/
/*jshint indent:2 */

//Create the main module and Elasticsearch client
var codeSearchApp = angular.module('codeSearch', ['ngSanitize', 'ui.bootstrap', 'hljs', 'toggle-switch']);

//Main controller
codeSearchApp.controller('CodeSearchController', function ($scope, $http, $modal) {

  'use strict';

  /** Elasticsearch server URL. */
  $scope.elasticsearchURL = 'http://localhost:9200/';

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

  $scope.fileContentsSearchTemplate = {

    fields: ['branch', 'file_name', 'repository'],
    size: 25,
    highlight: {
      pre_tags: ['<strong>'],
      post_tags: ['</strong>'],
      fields: {
        fragments: {
          number_of_fragments: 0
        }
      }
    },
    query: {
      filtered: {
        query: {
          simple_query_string: {
            query: '',
            fields: ['fragments'],
            default_operator: 'and'
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
   * 'Template' search object for changesets which will highlight the hits in the message
   */
  $scope.changesetSearchTemplate = {
    size: '25',
    highlight: {
      pre_tags: ['<strong>'],
      post_tags: ['</strong>'],
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
  $scope.searchChangesetsButtonClicked = function (useAndOperator) {

    //Make sure a query has been entered
    var changesetQueryMissing = $scope.changesetSearchTemplate.query.filtered.query.bool.must[0].match.message.query.length === 0;

    if (changesetQueryMissing) {
      $("#changeset-message-input").effect("highlight", {color:"#FFB2B2"}, 2000);
      document.getElementById('changeset-message-input').focus();
      return;
    }
    
    //Make a copy of the template query object to fill out with the search params
    var newTemplate = angular.copy($scope.changesetSearchTemplate);

    //Set the operator
    if (useAndOperator) {
      newTemplate.query.filtered.query.bool.must[0].match.message.operator = 'and';
    } else {
      newTemplate.query.filtered.query.bool.must[0].match.message.operator = 'or';
    }

    //If no search size was specified, use the default
    if (newTemplate.size.length === 0) {
      newTemplate.size = 25;
    }

    if ($scope.changesetSearchFilters.author.length > 0) {

      newTemplate.query.filtered.filter.bool.must.push({
        term: {
          author: $scope.changesetSearchFilters.author
        }
      });

    }

    if ($scope.changesetSearchFilters.branch.length > 0) {

      newTemplate.query.filtered.filter.bool.must.push({
        term: {
          branch: $scope.changesetSearchFilters.branch
        }
      });

    }

    if ($scope.changesetSearchFilters.repository.length > 0) {

      newTemplate.query.filtered.filter.bool.must.push({
        term: {
          repository: $scope.changesetSearchFilters.repository
        }
      });

    }

    $http.post($scope.elasticsearchURL + 'changesets/_search', angular.toJson(newTemplate))
      .success(function (data) {
        $scope.searchResults = {
          changesets: {
            hits: data.hits.hits
          },
          totalResults: data.hits.total,
          returnedResults: data.hits.hits.length
        };

      }).error(function () {
        //TODO: show some sort of error
    });
  };

  $scope.searchContentsButtonClicked = function (useAndOperator, includeDesignerFiles) {
    
    var contentsQueryMissing = $scope.fileContentsSearchTemplate.query.filtered.query.simple_query_string.query.length === 0;

    if (contentsQueryMissing) {
      $("#contents-contents-input").effect("highlight", {color:"#FFB2B2"}, 2000);
      document.getElementById('contents-contents-input').focus();
      return;
    }
    
    //Make a copy of the template query object to fill out with the search params
    var newTemplate = angular.copy($scope.fileContentsSearchTemplate);

    //Set the operator
    if (useAndOperator) {
      newTemplate.query.filtered.query.simple_query_string.default_operator = 'and';
    } else {
      newTemplate.query.filtered.query.simple_query_string.default_operator = 'or';
    }

    //If no search size was specified, use the default
    if (newTemplate.size.length === 0) {
      newTemplate.size = 25;
    }

    if ($scope.contentsSearchFilters.branch.length > 0) {

      newTemplate.query.filtered.filter.bool.must.push({
        term: {
          branch: $scope.contentsSearchFilters.branch
        }
      });

    }

    if ($scope.contentsSearchFilters.repository.length > 0) {

      newTemplate.query.filtered.filter.bool.must.push({
        term: {
          repository: $scope.contentsSearchFilters.repository
        }
      });

    }

    if (includeDesignerFiles === false) {

      newTemplate.query.filtered.filter.bool.must.push({
        term: {
          designer_generated: false
        }
      });
    }

    $http.post($scope.elasticsearchURL + 'file_contents/_search', angular.toJson(newTemplate))
      .success(function (data) {

        $scope.searchResults = {
          file_contents: {
            hits: data.hits.hits
          },
          totalResults: data.hits.total,
          returnedResults: data.hits.hits.length
        };

      }).error(function () {
        //TODO: show some sort of error
    });

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
          field: fieldName,
          size: 100
        }
      }
    };

    return $http.post($scope.elasticsearchURL + indexName + '/_suggest', angular.toJson(params)).then(function (res) {

      var suggestions = [];
      var partialUpper = partialVal.toUpperCase();

      angular.forEach(res.data.field_suggestions[0].options, function (item) {

        if (item.text.toUpperCase().indexOf(partialUpper) === 0) {
          suggestions.push(item.text);
        }

      });

      return suggestions;

    });
  };
  
  /**
   * Shows the modal window detailing the query syntax options.
   */
  $scope.showQuerySyntax = function () {
  
    $modal.open({
      templateUrl: 'querySyntaxTemplate.html',
      controller: QuerySyntaxModalCtrl,
      size: 'lg'
    });
  };
  
});

/**
 * Controller used by the query syntax modal.
 */
var QuerySyntaxModalCtrl = function ($scope, $modalInstance) {

  $scope.ok = function () {
    $modalInstance.close();
  };

};