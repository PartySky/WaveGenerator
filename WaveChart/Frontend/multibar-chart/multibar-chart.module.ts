import * as angular from "angular";

import MultibarChartTemplate from "./components/multibar-chart.html";
import { MultibarChartComponent } from "./components/multibar-chart.component";

export const name = "wave.chart.MultibarChart.components";
angular
    .module(name, ['nvd3'])
    .component("wgMultibarChart", {
        template: MultibarChartTemplate,
        controller: MultibarChartComponent
    })