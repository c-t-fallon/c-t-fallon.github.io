window.beamPlot = {
    render: function (elementId, xData, yData, yAxisTitle, lineColor) {
        const el = document.getElementById(elementId);
        if (!el) return;

        Plotly.react(el, [{
            x: xData,
            y: yData,
            type: 'scatter',
            mode: 'lines',
            line: { color: lineColor, width: 1.5 },
            fill: 'tozeroy',
            fillcolor: lineColor + '22',
        }], {
            margin: { t: 10, r: 10, b: 40, l: 55 },
            xaxis: { title: 'x (m)', zeroline: true, zerolinecolor: '#aaa' },
            yaxis: { title: yAxisTitle, zeroline: true, zerolinecolor: '#aaa' },
            paper_bgcolor: 'transparent',
            plot_bgcolor: 'transparent',
        }, {
            responsive: true,
            displayModeBar: false,
        });
    }
};
