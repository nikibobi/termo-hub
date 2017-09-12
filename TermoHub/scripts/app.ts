async function makeChartLive(baseUrl: string, unit: string, maxPoints: number, interval: number) {
    const chart = makeChart('canvas', unit);
    setInterval(tick, interval);

    async function tick() {
        await updateChart.call(chart, baseUrl, getFromDate(chart.data.labels), null, maxPoints); 
    }

    function getFromDate(points: any[]): string {
        let date: Date;
        if (points.length > 0) {
            date = new Date(points[points.length - 1]);
        } else {
            date = new Date();
            date.setMilliseconds(date.getMilliseconds() - maxPoints * interval);
        }
        return date.toISOString();
    }
}

async function makeChartStatic(baseUrl: string, unit: string, from: string, to: string) {
    const chart = makeChart('canvas', unit);
    await updateChart.call(chart, baseUrl, from, to, Infinity);
}

function makeChart(id: string, unit: string): Chart {
    const canvas = document.getElementById(id) as HTMLCanvasElement;
    const ctx = canvas.getContext('2d');
    const data: Chart.ChartData = {
        labels: [],
        datasets: [
            makeDataset('Temp', 'seagreen', 'lightgreen'),
            makeDataset('Alert', 'red', '#ffa07a')
        ]
    };
    const options: Chart.ChartOptions = {
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
                label: function (item: Chart.ChartTooltipItem) {
                    return `Temp: ${Number(item.yLabel).toFixed(2)} ${unit}`;
                }
            }
        }
    };
    const chart = new Chart(ctx, { type: 'line', data, options });
    return chart;

    function makeDataset(name: string, mainColor: string, altColor: string): Chart.ChartDataSets {
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
            data: [],
            spanGaps: "true" // used to draw lines where data is null
        };
    }
}

async function updateChart(baseUrl: string, from: string, to: string, maxPoints: number) {
    const points = this.data.labels;
    // add new data
    const data: Data[] = await getDataJson(baseUrl, from, to);
    appendData.call(this, data);
    // delete old data
    if (points.length > maxPoints) {
        const n = points.length - maxPoints;
        discardData.call(this, n);
    }
    // update alert line
    const alert: Alert = await getAlertJson(baseUrl);
    if (alert != null) {
        const n = points.length;
        appendAlert.call(this, alert, n);
    }
    this.update();
}

function getAlertJson(baseUrl: string): JQuery.jqXHR<Alert> {
    const url = `${baseUrl}/alert`;
    return $.get(url);
}

function getDataJson(baseUrl: string, from: string, to: string): JQuery.jqXHR<Data[]> {
    const url = `${baseUrl}/data.json?from=${encodeURI(from)}&to=${encodeURI(to)}`;
    return $.get(url);
}

interface Data
{
    time: string,
    value: number
}

function appendData(data: Data[]) {
    const labels = this.data.labels;
    const dataset = this.data.datasets[0];
    for (let d of data) {
        labels.push(Date.parse(d.time));
        dataset.data.push(d.value);
    }
}

interface Alert
{
    sign: number,
    value: number
}

function appendAlert(alert: Alert, n: number) {
    const dataset = this.data.datasets[1];
    dataset.fill = getAlertFill(alert.sign);

    // insert nulls at [1]
    while (dataset.data.length < n) {
        dataset.data.splice(1, 0, null);
    }
    // set first and last point to alert's value
    dataset.data[0] = alert.value;
    dataset.data[dataset.data.length - 1] = alert.value;

    function getAlertFill(sign: number): string | boolean {
        if (sign < 0) {
            return 'bottom';
        } else if (sign > 0) {
            return 'top';
        } else {
            return false;
        }
    }
}

function discardData(n: number) {
    const labels = this.data.labels;
    const dataset = this.data.datasets[0];
    for (let i = 0; i < n; i++) {
        labels.shift();
        dataset.data.shift();
    }
}
