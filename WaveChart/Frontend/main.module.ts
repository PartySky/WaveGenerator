import * as angular from "angular";

import * as usersComponentsModule from "./users/components/users.components.module";

angular
    .module("wave.chart.fullstack", [
        usersComponentsModule.name
    ]);