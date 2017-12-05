declare let d3: any;
export class MultibarChartComponent {
    trackOptions: object;
    trackData: object;

    constructor() {
        this.trackOptions = {
            chart: {
                type: 'multiBarChart',
                height: 450,
                margin: {
                    top: 20,
                    right: 20,
                    bottom: 45,
                    left: 45
                },
                clipEdge: true,
                //staggerLabels: true,
                duration: 500,
                stacked: true,
                xAxis: {
                    axisLabel: 'Time (ms)',
                    showMaxMin: false,
                    tickFormat: function (d: any) {
                        return d3.format(',f')(d);
                    }
                },
                yAxis: {
                    axisLabel: 'Y Axis',
                    axisLabelDistance: -20,
                    tickFormat: function (d: any) {
                        return d3.format(',.1f')(d);
                    }
                }
            }
        };

        this.trackData = generateData();

        function generateData() {
            return stream_layers(3, 50 + Math.random() * 50, .1).map(function (data: any, i: any) {
                return {
                    key: 'Stream' + i,
                    values: data
                };
            });
        }

        function stream_layers(n: any, m: any, o: any) {
            if (arguments.length < 3) o = 0;
            function bump(a: any) {
                var x = 1 / (.1 + Math.random()),
                    y = 2 * Math.random() - .5,
                    z = 10 / (.1 + Math.random());
                for (var i = 0; i < m; i++) {
                    var w = (i / m - y) * z;
                    a[i] += x * Math.exp(-w * w);
                }
            }
            return d3.range(n).map(function () {
                var a = [], i;
                for (i = 0; i < m; i++) a[i] = o + o * Math.random();
                for (i = 0; i < 5; i++) bump(a);
                return a.map(stream_index);
            });
        }
        function stream_waves(n: any, m: any) {
            return d3.range(n).map(function (i: any) {
                return d3.range(m).map(function (j: any) {
                    var x = 20 * j / m - i / 3;
                    return 2 * x * Math.exp(-.5 * x);
                }).map(stream_index);
            });
        }

        function stream_index(d: any, i: any) {
            return { x: i, y: Math.max(0, d) };
        }
    }

    check() {
        console.log();
        console.log();
    }
}