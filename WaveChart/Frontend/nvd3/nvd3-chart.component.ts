declare var d3: any;
export class nvd3ChartComponent {
    options_old: any;
    data_old: any;
    data2: any;
    testValues: number[];
    constructor() {
        this.testValues = [
            0.14851134646103104,
            0.19287449267511778,
            1.1857293891513511,
            2.12132450506168974,
            0.12408376470542225,
            0.15801432661864756,
            0.10359750019462409,
            0.1361343639702235,
            0.10253539963991792,
            0.15621305883260658,
            0.14071246265914772,
            0.18120801427043154,
            0.1559188154125321,
            0.18174976305847343,
            0.10728734691682704,
            0.16127952729027123,
            0.13315545325272615,
            0.1731639807253087,
            0.18973404321292064,
            0.16809427595982052,
            0.11456085063482434,
            0.14781038851608563,
            0.15758507219336057,
            0.18302419765079353,
            0.22010190041662217,
            0.35014910443869196,
            0.6464603272005277,
            1.0103845170836843,
            1.4668325778203948,
            1.9108357397979079,
            2.0933177880638256,
            2.007162074434141,
            1.5953646345907566,
            1.1646384252421371,
            0.7322333244562277,
            0.3862853833863409,
            0.2166640584337895,
            0.21864813158960417,
            0.1295801448236078,
            0.17605854982106178,
            0.11423136310572213,
            0.1578906492684653,
            0.12044260741971062,
            0.12124027705565518,
            0.16613147187785038,
            0.13415586698128387,
            0.16605061309163888,
            0.19750790196791262,
            0.16977633986864968,
            0.15416592413440083,
            0.17097095077358293,
            0.1601745431342941,
            0.12919823629069188,
            0.1901843581267463,
            0.15049078674934058,
            0.10920849538192842,
            0.1431513103519798,
            0.1002254495124157,
            0.14777373680462808,
            0.11238305971440381,
            0.16325945347155557,
            0.12601562985223316,
            0.10040734546985648,
            0.12459662333786983,
            0.16099060742808688,
            0.10015023110614497,
            0.1013961300892706,
            0.13322368987758626,
            0.1362543044553781,
            0.10695657373421177,
            0.18581020678486287,
            0.14837460270721595,
            0.18524256629915548,
            0.15745682386994214,
            0.15240091815422843,
            0.15401704377992712,
            0.18499753778406344,
            0.16948435430969422,
            0.16485388223288763,
            0.2140929148795242
        ]

        var n = 1, // The number of series.
            m = 80; // The number of values per series.

        // The xz array has m elements, representing the x-values shared by all series.
        // The yz array has n elements, representing the y-values of each of the n series.
        // Each yz[i] is an array of m non-negative numbers representing a y-value for xz[i].
        // The y01z array has the same structure as yz, but with stacked [y₀, y₁] instead of y.
        var xz = d3.range(m),
            yz = d3.range(n).map(() => { return this.testValues; }),
            
            y01z = d3.stack().keys(d3.range(n))(d3.transpose(yz)),
            yMax = d3.max(yz, function (y: any) { return d3.max(y); }),
            y1Max = d3.max(y01z, function (y: any) { return d3.max(y, function (d: any) { return d[1]; }); });

        var svg = d3.select("svg"),
            margin = { top: 40, right: 10, bottom: 20, left: 10 },
            width = +svg.attr("width") - margin.left - margin.right,
            height = +svg.attr("height") - margin.top - margin.bottom,
            g = svg.append("g").attr("transform", "translate(" + margin.left + "," + margin.top + ")");

        var x = d3.scaleBand()
            .domain(xz)
            .rangeRound([0, width])
            .padding(0.08);

        var y = d3.scaleLinear()
            .domain([0, y1Max])
            .range([height, 0]);

        var color = d3.scaleOrdinal()
            .domain(d3.range(n))
            .range(d3.schemeCategory20c);

        var series = g.selectAll(".series")
            .data(y01z)
            .enter().append("g")
            .attr("fill", function (d: any, i: any) { return color(i); });

        var rect = series.selectAll("rect")
            .data(function (d: any) { return d; })
            .enter().append("rect")
            .attr("x", function (d: any, i: any) { return x(i); })
            .attr("y", height)
            .attr("width", x.bandwidth())
            .attr("height", 0);

        rect.transition()
            .delay(function (d: any, i: any) { return i * 10; })
            .attr("y", function (d: any) { return y(d[1]); })
            .attr("height", function (d: any) { return y(d[0]) - y(d[1]); });

        g.append("g")
            .attr("class", "axis axis--x")
            .attr("transform", "translate(0," + height + ")")
            .call(d3.axisBottom(x)
                .tickSize(0)
                .tickPadding(6));

        d3.selectAll("input")
            .on("change", changed);

        var timeout = d3.timeout(function () {
            d3.select("input[value=\"grouped\"]")
                .property("checked", true)
                .dispatch("change");
        }, 2000);

        function changed() {
            timeout.stop();
            if (this.value === "grouped") transitionGrouped();
            else transitionStacked();
        }

        function transitionGrouped() {
            y.domain([0, yMax]);

            rect.transition()
                .duration(500)
                .delay(function (d: any, i: any) { return i * 10; })
                .attr("x", function (d: any, i: any) { return x(i) + x.bandwidth() / n * this.parentNode.__data__.key; })
                .attr("width", x.bandwidth() / n)
                .transition()
                .attr("y", function (d: any) { return y(d[1] - d[0]); })
                .attr("height", function (d: any) { return y(0) - y(d[1] - d[0]); });
        }

        function transitionStacked() {
            y.domain([0, y1Max]);

            rect.transition()
                .duration(500)
                .delay(function (d: any, i: any) { return i * 10; })
                .attr("y", function (d: any) { return y(d[1]); })
                .attr("height", function (d: any) { return y(d[0]) - y(d[1]); })
                .transition()
                .attr("x", function (d: any, i: any) { return x(i); })
                .attr("width", x.bandwidth());
        }
    }
}