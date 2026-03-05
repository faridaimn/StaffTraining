window.renderFusionChart = function (
  chartId,
  chartType,
  caption,
  subCaption,
  xAxisName,
  yAxisName,
  data,
) {
  if (FusionCharts(chartId)) {
    FusionCharts(chartId).dispose();
  }

  var chartConfig = {
    type: chartType,
    renderAt: chartId,
    width: "100%",
    height: "350",
    dataFormat: "json",
    dataSource: {
      chart: {
        caption: caption,
        subCaption: subCaption,
        xAxisName: xAxisName,
        yAxisName: yAxisName,
        theme: "fusion",
        showValues: "1",
        labelFontSize: "13",
        baseFontSize: "13",
      },
      data: data,
    },
  };

  var chart = new FusionCharts(chartConfig);
  chart.render();
};
