function makeChartLive(maxPoints, interval, url) {
    let chart = makeChart('canvas');
    let last = new Date();
    last.setMilliseconds(last.getMilliseconds() - maxPoints * interval);

    setInterval(function () {
        if (chart.data.labels.length > 0) {
            last = new Date(chart.data.labels[chart.data.labels.length - 1]);
        }
        const query = `?from=${encodeURI(last.toISOString())}`;
        d3.json(url + query, function (data) {
            // add new data
            appendData(chart, data);
            // delete old data
            discardData(chart, chart.data.labels.length - maxPoints);
            chart.update();
        });
    }, interval);
}

function makeChartStatic(from, to, url) {
    let chart = makeChart('canvas');
    const query = `?from=${from}&to=${to}`;
    d3.json(url + query, function (data) {
        appendData(chart, data);
        chart.update();
    });
}

function makeChart(id) {
    const ctx = document.getElementById(id).getContext('2d');
    const unit = '°C';
    const data = {
        labels: [],
        datasets: [
            {
                label: 'Temp',
                fill: false,
                backgroundColor: 'lightgreen',
                pointRadius: 6,
                pointHoverRadius: 6,
                pointBorderWidth: 2,
                borderColor: 'seagreen',
                pointBorderColor: 'seagreen',
                pointHoverBorderColor: 'white',
                pointBackgroundColor: 'lightgreen',
                pointHoverBackgroundColor: 'seagreen',
                data: []
            }
        ]
    };
    const options = {
        scales: {
            xAxes: [{
                type: 'time',
                time: {
                    tooltipFormat: 'D/M/YYYY HH:mm:ss',
                    unit: 'minute',
                    displayFormats: {
                        minute: 'HH:mm'
                    }
                }
            }],
            yAxes: [{
                ticks: {
                    callback: function (label) {
                        return `${label.toFixed(2)} ${unit}`;
                    }
                }
            }]
        },
        tooltips: {
            callbacks: {
                label: function (item) {
                    return `Temp: ${item.yLabel.toFixed(2)} ${unit}`;
                },
                labelColor: function (item) {
                    return {
                        borderColor: 'seagreen',
                        backgroundColor: 'lightgreen'
                    };
                }
            }
        }
    };
    let chart = new Chart.Line(ctx, { data, options });
    return chart;
}

function appendData(chart, data) {
    for (let d of data) {
        chart.data.labels.push(Date.parse(d.time));
        for (let dataset of chart.data.datasets) {
            dataset.data.push(d.value);
        }
    }
}

function discardData(chart, n) {
    if (n <= 0)
        return;
    chart.data.labels.shift(n);
    for (let dataset of chart.data.datasets) {
        dataset.data.shift(n);
    }
}
