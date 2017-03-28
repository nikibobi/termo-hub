function makeChart(maxPoints, unit, interval, url) {
    const ctx = document.getElementById('canvas').getContext('2d');
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
    let last = new Date();

    setInterval(function () {
        if (chart.data.labels.length > 0) {
            last = new Date(chart.data.labels[chart.data.labels.length - 1]);
        }
        const query = `?t=${encodeURI(last.toISOString())}`;
        d3.json(url + query, update);
    }, interval);

    function update(data) {
        // add new data
        for (let d of data) {
            chart.data.labels.push(Date.parse(d.time));
            for (let dataset of chart.data.datasets) {
                dataset.data.push(d.value);
            }
        }

        // delete old data
        let n = chart.data.labels.length - maxPoints;
        if (n > 0) {
            chart.data.labels.shift(n);
            for (let dataset of chart.data.datasets) {
                dataset.data.shift(n);
            }
        }
        chart.update();
    }
}
