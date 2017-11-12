declare var d3: any;
export class nvd3ChartComponent {
    users: any[];
    options_old: any;
    data_old: any;
    data2: any;

    constructor() {
        this.users = [
            "User 1",
            "User 2"
        ];

        /* Chart options */
        this.options_old = {
            // $scope.options = {
            chart: {
                type: "pieChart",
                height: 180, // actual chart size will be 104px
                width: 180, // actual chart size will be 104px
                donut: true,
                donutRatio: 0.6,
                x: function (d: any) {
                    return d.group;
                },
                y: function (d: any) {
                    return d.count;
                },
                duration: 500,
                // title: this.tasks.TotalCount.toString(),
                title: 'title for chart',
                showLegend: false,
                showLabels: false,
                growOnHover: true,
                color: function (d: any) {
                    return d.color;
                },
                tooltip: {
                    enabled: false
                },
                style: {
                    classes: {
                        "donut_big": true
                    }
                }
            }
        };

        this.data2 = [10, 29, 59];


        /* Chart data */
        // $scope.data = [{
        this.data_old = [{
            group: "Total",
            count: 0.001

            // key: "Cumulative Return",
            // values: [
            //     // { "label": "A", "value": -29.765957771107 },
            //     // { "label": "B", "value": 0 },
            //     // { "label": "C", "value": 32.807804682612 },
            //     // { "label": "D", "value": 196.45946739256 },
            //     // { "label": "E", "value": 0.19434030906893 },
            //     // { "label": "F", "value": -98.079782601442 },
            //     // { "label": "G", "value": -13.925743130903 },
            //     // { "label": "H", "value": -5.1387322875705 }
            // ]
        }]

        var svg = d3.select("svg"),
            margin = {
                top: 20,
                right: 20,
                bottom: 30,
                left: 50
            },
            width = +svg.attr("width") - margin.left - margin.right,
            height = +svg.attr("height") - margin.top - margin.bottom,
            g = svg.append("g").attr("transform", "translate(" + margin.left + "," + margin.top + ")");

        var parseTime = d3.timeParse("%d-%b-%y");

        var x = d3.scaleTime()
            .rangeRound([0, width]);

        var y = d3.scaleLinear()
            .rangeRound([height, 0]);

        var line = d3.line()
            .x(function (d: any) {
                return x(d.date);
            })
            .y(function (d: any) {
                return y(d.close);
            });

        d3.tsv(
            "data.tsv",
            function (d: any) {
                d.date = parseTime(d.date);
                d.close = +d.close;
                return d;
            },
            function (error: any, data: any) {
                if (error) throw error;

                x.domain(d3.extent(data, function (d: any) {
                    return d.date;
                }));
                y.domain(d3.extent(data, function (d: any) {
                    return d.close;
                }));

                g.append("g")
                    .attr("transform", "translate(0," + height + ")")
                    .call(d3.axisBottom(x))
                    .select(".domain")
                    .remove();

                g.append("g")
                    .call(d3.axisLeft(y))
                    .append("text")
                    .attr("fill", "#000")
                    .attr("transform", "rotate(-90)")
                    .attr("y", 6)
                    .attr("dy", "0.71em")
                    .attr("text-anchor", "end")
                    .text("Price ($)");

                g.append("path")
                    .datum(data)
                    .attr("fill", "none")
                    .attr("stroke", "steelblue")
                    .attr("stroke-linejoin", "round")
                    .attr("stroke-linecap", "round")
                    .attr("stroke-width", 1.5)
                    .attr("d", line);
            });
    }
}