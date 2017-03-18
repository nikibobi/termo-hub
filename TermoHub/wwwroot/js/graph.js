function chart(data)
{
    data.forEach(function (d) {
        d.time = d3.isoParse(d.time);
    });

    function px(prop) {
        return +this.style(prop).slice(0, -2);
    }
    
    let svg = d3.select("#chart");
    px = px.bind(svg);
    svg.attr("width", px("width"));
    svg.attr("height", px("height"));
    let width = svg.attr("width");
    let height = svg.attr("height");

    let x = d3.scaleTime()
        .domain(d3.extent(data, function (d) { return d.time; }))
        .range([0, width]);

    let y = d3.scaleLinear()
        .domain(d3.extent(data, function (d) { return d.value; }))
        .range([height, 0]);

    let line = d3.line()
        .curve(d3.curveMonotoneX)
        .x(function (d) { return x(d.time); })
        .y(function (d) { return y(d.value); });

    // Add the X Axis
    svg.append("g")
        .call(d3.axisBottom(x));

    // Add the Y Axis
    svg.append("g")
        .call(d3.axisRight(y));

    svg.append("path")
        .datum(data)
        .attr("class", "line")
        .attr("d", line);
}