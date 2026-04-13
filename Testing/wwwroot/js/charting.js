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

// Marital Status Chart
window.renderMaritalChart = function (chartId, data) {
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
                xAxisName: "Marital Status",
                yAxisName: "Count",
                theme: "fusion",
                showborder: "0",
                showvalues: "1",
                paletteColors: "#fbbf24,#f59e0b,#d97706"
            },
            data: data
        },
        events: {
            dataplotClick: function (eventObj, dataObj) {
                window.location.href = '/personlist/marital/' + encodeURIComponent(dataObj.categoryLabel);
            }
        }
    }).render();
};

// Birth Year Chart
window.renderBirthYearChart = function (chartId, data) {
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
                xAxisName: "Birth Year",
                yAxisName: "Count",
                theme: "fusion",
                showborder: "0",
                showvalues: "1",
                paletteColors: "#3b82f6,#60a5fa,#93c5fd,#bfdbfe"
            },
            data: data
        },
        events: {
            dataplotClick: function (eventObj, dataObj) {
                window.location.href = '/personlist/birthyear/' + encodeURIComponent(dataObj.categoryLabel);
            }
        }
    }).render();
};

// Color Chart with Click Event
window.renderColorChart = function (chartId, data, dotNetHelper) {
    if (FusionCharts(chartId)) {
        FusionCharts(chartId).dispose();
    }
    new FusionCharts({
        type: "bar2d",
        renderAt: chartId,
        width: "100%",
        height: "280",
        dataFormat: "json",
        dataSource: {
            chart: {
                caption: "",
                xAxisName: "Color",
                yAxisName: "Count",
                theme: "fusion",
                showborder: "0",
                showvalues: "1",
                paletteColors: "#a855f7,#c084fc,#d8b4fe,#e9d5ff"
            },
            data: data
        },
        events: {
            dataplotClick: function (eventObj, dataObj) {
                dotNetHelper.invokeMethodAsync('OnColorBarClick', dataObj.categoryLabel);
            }
        }
    }).render();
};

// Brand Chart with Click Event
window.renderBrandChart = function (chartId, data, dotNetHelper) {
    if (FusionCharts(chartId)) {
        FusionCharts(chartId).dispose();
    }
    new FusionCharts({
        type: "bar2d",
        renderAt: chartId,
        width: "100%",
        height: "280",
        dataFormat: "json",
        dataSource: {
            chart: {
                caption: "",
                xAxisName: "Brand",
                yAxisName: "Count",
                theme: "fusion",
                showborder: "0",
                showvalues: "1",
                paletteColors: "#22c55e,#4ade80,#86efac,#bbf7d0"
            },
            data: data
        },
        events: {
            dataplotClick: function (eventObj, dataObj) {
                dotNetHelper.invokeMethodAsync('OnBrandBarClick', dataObj.categoryLabel);
            }
        }
    }).render();
};

// Model Chart with Click Event
window.renderModelChart = function (chartId, data, dotNetHelper) {
    if (FusionCharts(chartId)) {
        FusionCharts(chartId).dispose();
    }
    new FusionCharts({
        type: "bar2d",
        renderAt: chartId,
        width: "100%",
        height: "280",
        dataFormat: "json",
        dataSource: {
            chart: {
                caption: "",
                xAxisName: "Model",
                yAxisName: "Count",
                theme: "fusion",
                showborder: "0",
                showvalues: "1",
                paletteColors: "#f97316,#fb923c,#fdba74,#fed7aa"
            },
            data: data
        },
        events: {
            dataplotClick: function (eventObj, dataObj) {
                dotNetHelper.invokeMethodAsync('OnModelBarClick', dataObj.categoryLabel);
            }
        }
    }).render();
};