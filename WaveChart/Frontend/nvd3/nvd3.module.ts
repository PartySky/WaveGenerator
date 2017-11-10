import * as angular from "angular";

import nvd3ChartTemplate from "./nvd3.html";
import { nvd3ChartComponent } from "./nvd3-chart.component";

export const name = "nvd3";

angular
    .module(name, [])
    .component("wgChart", {
        template: nvd3ChartTemplate,
        controller: nvd3ChartComponent
    });