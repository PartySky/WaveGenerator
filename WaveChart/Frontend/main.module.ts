import * as angular from "angular";

import * as SunburstChart from "./sunburst-chart/sunburst-chart.module";
import * as MultibarChart from "./multibar-chart/multibar-chart.module";


angular
    .module('wave.chart', [
        SunburstChart.name,
        MultibarChart.name
    ]);