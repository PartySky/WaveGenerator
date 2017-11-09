import * as angular from "angular";

import usersTableTemplate from "./users-table/users-table.html";
import { UsersTableComponent } from "./users-table/users-table.component";

export const name = "wave.chart.fullstack.users.components";
angular
    .module(name, [])
    .component("wgUsersTable", {
        template: usersTableTemplate,
        controller: UsersTableComponent
    });