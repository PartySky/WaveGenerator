import * as angular from "angular";

import nvd3ChartTemplate from "./nvd3.html";
import { nvd3ChartComponent } from "./nvd3-chart.component";

export const name = "nvd3";

declare var d3: any;

angular
    .module(name, [])
    .component("wgChart", {
        template: nvd3ChartTemplate,
        controller: nvd3ChartComponent
    }).directive('barGraph', function () {
        return {
            scope: {
                data: '=graphData'
            },
            link: function (scope, element, attrs) {

                //Appends the graph to graph directive
                var graph = d3.select(element[0]);

                var divs = graph.append("div")
                    .attr("class", "chart")
                    .selectAll('div');

                //Render graph based on 'data'
                var render = function (data: any) {

                    if (!data) return;

                    divs = divs.data(data);
                    divs.exit().remove();
                    divs.enter().append('div')
                        .transition().ease("elastic")
                        .style("width", function (d: any) {
                            return d + "%";
                        })
                        .text(function (d: any) {
                            return d + "%";
                        });


                };
                scope.$watch('data', render, true);
            }
        }
    }).controller("DashboardController", ["$scope", function ($scope) {
        $scope.data = [10, 29, 59]

        $scope.addValue = function () {
            if ($scope.newValue) {
                $scope.data.push(this.newValue)
                $scope.newValue = '';
            }
        }
    }]);