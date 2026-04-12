window.renderGenderChart = function (chartId, data) {
    if (FusionCharts(chartId)) {
        FusionCharts(chartId).dispose();
    }
    new FusionCharts({
        type: "bar2d",
        renderAt: chartId,
        width: "100%",
        height: "300",
        dataFormat: "json",
        dataSource: {
            chart: {
                caption: "",
                xAxisName: "Gender",
                yAxisName: "Count",
                theme: "fusion",
                showborder: "0",
                showvalues: "1",
                paletteColors: "#7c3aed,#a78bfa"
            },
            data: data
        },
        events: {
            dataplotClick: function (eventObj, dataObj) {
                window.location.href = '/personlist/' + dataObj.categoryLabel;
            }
        }
    }).render();
};


//nationality
window.renderNationalityChart = function (chartId, data) {
    if (FusionCharts(chartId)) {
        FusionCharts(chartId).dispose();
    }
    new FusionCharts({
        type: "bar2d",
        renderAt: chartId,
        width: "100%",
        height: "300",
        dataFormat: "json",
        dataSource: {
            chart: {
                caption: "",
                xAxisName: "Nationality",
                yAxisName: "Count",
                theme: "fusion",
                showborder: "0",
                showvalues: "1",
                paletteColors: "#059669,#10b981,#34d399,#6ee7b7"
            },
            data: data
        },
        events: {
            dataplotClick: function (eventObj, dataObj) {
                window.location.href = '/personlist/nationality/' + encodeURIComponent(dataObj.categoryLabel);
            }
        }
    }).render();
};