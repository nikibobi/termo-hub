async function makeChartLive(baseUrl, maxPoints, interval) {
    let chart = makeChart('canvas');
    setInterval(tick, interval);

    async function tick() {
        await updateChart.call(chart, baseUrl, getFromDate(chart.data.labels), null, maxPoints); 
    }

    function getFromDate(points) {
        let date;
        if (points.length > 0) {
            date = new Date(points[points.length - 1]);
        } else {
            date = new Date();
            date.setMilliseconds(date.getMilliseconds() - maxPoints * interval);
        }
        return date.toISOString();
    }
}

async function makeChartStatic(baseUrl, from, to) {
    let chart = makeChart('canvas');
    await updateChart.call(chart, baseUrl, from, to, Infinity);
}

function makeChart(id) {
    const ctx = document.getElementById(id).getContext('2d');
    const unit = '°C';
    const data = {
        labels: [],
        datasets: [
            makeDataset('Temp', 'seagreen', 'lightgreen'),
            makeDataset('Alert', 'red', '#ffa07a')
        ]
    };
    const options = {
        scales: {
            xAxes: [{
                type: 'time',
                time: {
                    unit: 'second',
                    unitStepSize: 60,
                    round: 'second',
                    tooltipFormat: 'L LTS',
                    displayFormats: {
                        second: 'LT',
                        minute: 'LT',
                        hour: 'Do H:mm',
                        day: 'MMM Do LT'
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

    function makeDataset(name, mainColor, altColor) {
        return {
            label: name,
            fill: false,
            backgroundColor: altColor,
            pointRadius: 2,
            pointHoverRadius: 2,
            pointBorderWidth: 1,
            borderColor: mainColor,
            pointBorderColor: mainColor,
            pointHoverBorderColor: 'white',
            pointBackgroundColor: altColor,
            pointHoverBackgroundColor: mainColor,
            spanGaps: true, // used to draw lines where data is null
            data: []
        }
    }
}

async function updateChart(baseUrl, from, to, maxPoints) {
    let points = this.data.labels;
    const data = await getDataJson(baseUrl, from, to);
    const alert = await getAlertJson(baseUrl);
    // add new data
    appendData.call(this, data);
    if (alert != null) {
        const n = points.length;
        // update alert line
        appendAlert.call(this, alert, n);
    }
    if (points.length > maxPoints) {
        const n = points.length - maxPoints;
        // delete old data
        discardData.call(this, n);
    }
    this.update();
}

function getAlertJson(baseUrl) {
    const url = `${baseUrl}/alert`;
    return $.get(url);
}

function getDataJson(baseUrl, from, to) {
    const url = `${baseUrl}/data.json?from=${encodeURI(from)}&to=${encodeURI(to)}`;
    return $.get(url);
}

function appendData(data) {
    let labels = this.data.labels;
    let dataset = this.data.datasets[0];
    for (let d of data) {
        labels.push(Date.parse(d.time));
        dataset.data.push(d.value);
    }
}

function appendAlert(alert, n) {
    let dataset = this.data.datasets[1];
    dataset.fill = getAlertFill(alert.sign);

    // insert nulls at [1]
    while (dataset.data.length < n) {
        dataset.data.splice(1, 0, null);
    }
    // set first and last point to alert's value
    dataset.data[0] = alert.value;
    dataset.data[dataset.data.length - 1] = alert.value;

    function getAlertFill(sign) {
        if (sign < 0) {
            return 'bottom';
        } else if (sign > 0) {
            return 'top';
        } else {
            return false;
        }
    }
}

function discardData(n) {
    let labels = this.data.labels;
    let dataset = this.data.datasets[0];
    for (let i = 0; i < n; i++) {
        labels.shift();
        dataset.data.shift();
    }
}
