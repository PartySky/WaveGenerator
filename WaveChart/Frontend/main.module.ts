import * as angular from "angular";

import * as usersComponentsModule from "./users/components/users.components.module";
import * as nvd3 from "./nvd3/nvd3.module";


angular
    .module('wave.chart.fullstack', [
        usersComponentsModule.name,
        nvd3.name
    ]);