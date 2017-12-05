import * as angular from "angular";

import SunburstChartTemplate from "./components/sunburst-chart.html";
import { SunburstChartComponent } from "./components/sunburst-chart.component";

export const name = "wave.chart.SunburstChart.components";
angular
    .module(name, ['nvd3'])
    .component("wgSunburstChart", {
        template: SunburstChartTemplate,
        controller: SunburstChartComponent
    })