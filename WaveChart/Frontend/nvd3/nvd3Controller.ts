// import { EmployeeService } from "../employee/services/employee.api.service";

export class nvd3Controller {
    users: any[];
    options: any;
    data: any;
    constructor(
        public $scope: any,

    ) {
        $scope.ctrl = this;

        this.users = [
            "User 1 from nvd3",
            "User 2 from nvd3",
            "User 3 from nvd3",
        ];


        /* Chart options */
        this.options = {
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




        /* Chart data */
        // $scope.data = [{
        this.data = [{
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
    }

}