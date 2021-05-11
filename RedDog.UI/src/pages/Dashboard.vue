<template>
  <div class="container">
    <div class="row justify-content-lg-left">
      <div class="col-lg-4">
          <div class="row">
            <div class="col-lg-12">
              <div class="card card-trans-base">
                <div class="card-header-title"><i class="tim-icons icon-check-2 text-success"></i>FULFILLED ORDERS</div>
                <div class="card-body">
                  <div class="card-big-detail text-center">{{ totalOrders }}</div>
                  <div class="card-footer-title text-right">1D 1W 1M</div>
                </div>              
              </div>
            </div>
          </div>
        </div>
      <div class="col-lg-4">
          <div class="row">
            <div class="col-lg-12">
              <div class="card card-trans-base">
                <div class="card-header-title"><i class="tim-icons icon-cart text-warning"></i>IN-PROCESS ORDERS</div>
                <div class="card-body">
                  <div class="card-big-detail text-center">8</div>
                  <div class="card-footer-title text-right"><i class="tim-icons icon-refresh-01"></i></div>
                </div>              
              </div>
            </div>
          </div>
        </div>
      <div class="col-lg-4">
          <div class="row">
            <div class="col-lg-12">
              <div class="card card-trans-base">
                <div class="card-header-title"><i class="tim-icons icon-watch-time text-secondary"></i>AVG ORDER TIME</div>
                <div class="card-body">
                  <div class="card-big-detail text-center">96s</div>
                  <div class="card-footer-title text-right"><i class="tim-icons icon-refresh-01"></i></div>
                </div>              
              </div>
            </div>
          </div>
        </div>
    </div>
    <div class="row justify-content-lg-left">
      <div class="col-lg-4">
          <div class="row">
            <div class="col-lg-12">
              <div class="card card-trans-base">
                <div class="card-header-title"><i class="tim-icons icon-satisfied text-info"></i>FULFILLMENT GOAL</div>
                <div class="card-body chart-doughnut-canvas">
                  <!-- <canvas id="compOrdersChart" width="40vh" height="40vh"></canvas> -->
                  <canvas id="compOrdersChart" ></canvas>
                  <!-- <doughnut-chart chart-id="white-doughnut" :chart-data="doChart.chartData"></doughnut-chart> -->
                </div>              
              </div>
            </div>
          </div>
        </div>
        <div class="col-lg-4">
          <div class="row">
            <div class="col-lg-12">
              <div class="card card-trans-base">
                <div class="card-header-title"><i class="tim-icons icon-camera-18 text-danger"></i>IN-STORE TRAFFIC</div>
                <div class="card-body">
                  <div class="card-big-detail text-center">12</div>
                  <div class="card-footer-title text-right"><i class="tim-icons icon-refresh-01"></i></div>
                </div>              
              </div>
            </div>
          </div>
        </div>
        <div class="col-lg-4" :class="{ 'text-right': isRTL }">
          <!-- <card type="chart">
            <template slot="header">
              <h5 class="card-category">{{ $t("dashboard.averageMakeTime") }}</h5>
              <h3 class="card-title">
                <i class="tim-icons icon-send text-warning"></i>
                {{ AverageMakeTime }} seconds
              </h3>
            </template>
            <div class="chart-area">
              <line-chart
                style="height: 100%"
                chart-id="orange-line-chart"
                :chart-data="orangeLineChart.chartData"
                :gradient-stops="orangeLineChart.gradientStops"
                :extra-options="orangeLineChart.extraOptions"
              >
              </line-chart>
            </div>
          </card> -->
        </div>
      </div>
  </div>
</template>
<script>
import chart from 'chart.js'

chart.defaults.font.family = "'Exo', sans-serif";
chart.defaults.font.size = 12;
chart.defaults.font.weight = 700;
chart.defaults.plugins.legend.display =false;

const getOrCreateTooltip = (chart) => {
  let tooltipEl = chart.canvas.parentNode.querySelector('div');

  if (!tooltipEl) {
    tooltipEl = document.createElement('div');
    tooltipEl.style.background = 'rgba(255,255,255, 0.9)';
    tooltipEl.style.borderRadius = '3px';
    tooltipEl.style.color = '#222';
    tooltipEl.style.opacity = 1;
    tooltipEl.style.pointerEvents = 'none';
    tooltipEl.style.position = 'absolute';
    tooltipEl.style.transform = 'translate(-50%, 0)';
    tooltipEl.style.transition = 'all .1s ease';

    const table = document.createElement('table');
    table.style.margin = '0px';

    tooltipEl.appendChild(table);
    chart.canvas.parentNode.appendChild(tooltipEl);
  }

  return tooltipEl;
};

export default {
  components: {
  },
  data() {
    return {
      ctx:null,
      currentDateTime: "",
      polling: null,
      totalOrders: 345,
      currentOrders: 0,
      InFlightSales: 666.1, // Dynamic values to be updated
      DailySales: 3500.07,
      AverageMakeTime: 30.6,
      MonthlySales: 80873.09,
      
    };
  },
  computed: {
    enableRTL() {
      return this.$route.query.enableRTL;
    },
    isRTL() {
      return this.$rtl.isRTL;
    }
  },
  methods: {
    completedOrders(){
      console.log(`filling completed orders chart`);
      let compOrdersChart = document.getElementById("compOrdersChart");
      let chartHeight = compOrdersChart.height;
      // console.log(chartHeight);
      
      
      const data = {
        labels: [
          
          'Orders Filled Today',
        ],
        datasets: [{
          data: [128],
          // borderRadius: 10,
          borderAlign: 'center',
          borderRadius:{
            outerStart:8,
            innerStart:8,
            outerEnd:8,
            innerEnd:8
          },
          borderWidth: -10,
          borderColor: '#222',
          rotation:240,
          circumference: 195,
          backgroundColor: [
            'rgba(144, 184, 51, .95)'
          ],
          weight:405
          // hoverOffset: 10
        },

        // {data: [300],
        //   borderRadius: 10,
        //   borderWidth: 5,
        //   borderColor: 'rgba(54, 162, 235, .25)',
        //   rotation:180,
        //   circumference: 270,
        //   backgroundColor: [
        //     'rgb(54, 162, 235)',
        //   ],
        //   hoverOffset: 10,
        // }
        ]
      };
      const config = {
        type: 'doughnut',
        responsive: true,
        data,
        options: {
          maintainAspectRatio:false,
          cutout:'80%', 
          radius: '85%',
          plugins: { 
             interaction: {
              intersect: false,
              mode: 'index',
            },
            tooltip:{
              enabled: false,
              position: 'nearest',
              external: this.createToolTip
            },
            title: {
              display: true,
              position: 'bottom',
              // text: data.datasets[0].data[0],
              text: 128,
              align: 'center',
              color: '#FFF',
              font: {
                size: 28,
                weight: '600'
            },
            padding: {
              x: 0,
              y: -(chartHeight *.3)
              }
            }
          }
        }
      };

      var coChart = new chart(
          compOrdersChart,
          config
        );

    },
    initOrdersInProgress(index) {
      // Orders In Progress: get num and cost of orders in prog
      // code here for current cost of total orders
      console.log(this.CurrentOrderData);
      this.InFlightOrders = this.CurrentOrderData.length;
      this.InFlightSales = 0.0;
      this.orderCostData = [];
      this.orderIndex = [];
      this.CurrentOrderData.forEach((sd, i) => {
        this.InFlightSales += sd.orderTotal;
        this.orderCostData.push(sd.orderTotal);
        this.orderIndex.push(i + 1);
      });
      // console.log(this.InFlightSales);
      // console.log(this.InFlightOrders);
      // console.log(this.orderIndex);

      // let chartData = {
      //   datasets: [
      //     {
      //       label: "Total",
      //       fill: true,
      //       borderColor: config.colors.purple,
      //       borderWidth: 2,
      //       borderDash: [],
      //       borderDashOffset: 0.0,
      //       pointBackgroundColor: config.colors.purple,
      //       pointBorderColor: "rgba(255,255,255,0)",
      //       pointHoverBackgroundColor: config.colors.purple,
      //       pointBorderWidth: 20,
      //       pointHoverRadius: 4,
      //       pointHoverBorderWidth: 15,
      //       pointRadius: 4,
      //       data: this.orderCostData,
      //     },
      //   ],
      //   labels: this.orderIndex,
      // };

      // this.$refs.purpleChart.updateGradients(chartData);
      // this.purpleLineChart.chartData = chartData;
      // this.purpleLineChart.activeIndex = index;
    },
    createToolTip(context){
      console.log('testing')
      // Tooltip Element
      const {chart, tooltip} = context;
      const tooltipEl = getOrCreateTooltip(chart);

      // Hide if no tooltip
      if (tooltip.opacity === 0) {
        tooltipEl.style.opacity = 0;
        return;
      }

      // Set Text
      if (tooltip.body) {
        const titleLines = tooltip.title || [];
        const bodyLines = tooltip.body.map(b => b.lines);

        const tableHead = document.createElement('thead');

        titleLines.forEach(title => {
          const tr = document.createElement('tr');
          tr.style.borderWidth = 0;

          const th = document.createElement('th');
          th.style.borderWidth = 0;
          const text = document.createTextNode(title);

          th.appendChild(text);
          tr.appendChild(th);
          tableHead.appendChild(tr);
        });

        const tableBody = document.createElement('tbody');
        bodyLines.forEach((body, i) => {
          // var b = JSON.parse(body[0])
          // var o = JSON.stringify(body[0])
          // var oj = "{" + body[0] + "}";
          // var ojt = JSON.stringify(oj);
          // var ojo = JSON.parse(ojt);
          // // console.log(body[0].[0]);
          // console.log(tooltip);
          // console.log(tooltip.dataPoints[0].formattedValue);

          // console.log(tooltip.dataPoints[0].dataset.data);
          // [0].dataset.formattedValue
          const colors = tooltip.labelColors[i];

          // const span = document.createElement('span');
          // span.style.border = "1px solid #FFF";
          // console.log(span);

          // const span = document.createElement('span');
          // span.style.background = colors.backgroundColor;
          // span.style.borderColor = colors.borderColor;
          // span.style.borderWidth = '2px';
          // span.style.marginRight = '10px';
          // span.style.height = '10px';
          // span.style.width = '24px';
          // span.style.display = 'inline-block';

          const tr = document.createElement('tr');
          tr.style.backgroundColor = 'inherit';
          tr.style.borderWidth = 0;
          

          const td = document.createElement('td');
          td.style.borderWidth = 0;
          td.style.width = "120px";
          td.style.minWidth = "120px";
          td.style.letterSpacing = "1px";
          td.style['text-align'] = "center";
          console.log(body);
          const text = document.createTextNode(["128 out of 208"]);

          // td.appendChild(span);
          td.appendChild(text);
          tr.appendChild(td);
          tableBody.appendChild(tr);
        });

        const tableRoot = tooltipEl.querySelector('table');

        // Remove old children
        while (tableRoot.firstChild) {
          tableRoot.firstChild.remove();
        }

        // Add new children
        tableRoot.appendChild(tableHead);
        tableRoot.appendChild(tableBody);
      }

      const {offsetLeft: positionX, offsetTop: positionY} = chart.canvas;

      // Display, position, and set styles for font
      tooltipEl.style.opacity = 1;
      tooltipEl.style.left = positionX + tooltip.caretX + 'px';
      tooltipEl.style.top = positionY + tooltip.caretY + 'px';
      tooltipEl.style.font = tooltip.options.bodyFont.string;
      tooltipEl.style.padding = tooltip.options.padding + 'px ' + tooltip.options.padding + 'px';

    },
    getCurrentDateTime() {
      var current = new Date();
      this.currentDateTime = current.toLocaleString();
    },
    pollOrderCount() {
      this.currentOrders = 12;
      // this.polling = setInterval(() => {
      //   fetch("/currentOrders")
      //     .then((response) => response.json())
      //     .then((data) => {
      //       this.currentOrders = data.length;
      //       this.getCurrentDateTime();
      //     });
      // }, 3000);
    },
    // toolTipDoughnut(tt){
    //   console.log(tt)
    //   tt["formattedValue"] = 525;

    //   return "AMAZING: 100%";
    //   // tt.forEach((item) =>{
    //   //   console.log(item)
    //   // })
    //   // console.log(tt)
    // }
  },
  mounted() {
    // const plugin = document.createElement("script");
    // plugin.setAttribute(
    //   "src",
    //   "//cdn.jsdelivr.net/npm/chart.js"
    // );
    // plugin.async = true;
    // document.head.appendChild(plugin);
    
    this.completedOrders();
    // fetch("/sales/monthly")
    //   .then((response) => response.json())
    //   .then((data) => {
    //     this.SalesData = data;
    //     // this.initBigChart(0);
    //   });
    // this.SalesLabels = [];
    // this.MonthlySalesDollars = [];
    this.i18n = this.$i18n;
    if (this.enableRTL) {
      this.i18n.locale = "ar";
      this.$rtl.enableRTL();
    }

    // call orders EP at this time - done
    // Orders In Progress: get num orders in process - tally cost
    // Gross Sales??
    // Average Maketime??

    this.SalesLabels = [];
    this.MonthlySalesDollars = [];
    this.i18n = this.$i18n;
    if (this.enableRTL) {
      this.i18n.locale = "ar";
      this.$rtl.enableRTL();
    }
  },
  beforeDestroy() {
    clearInterval(this.polling);

    if (this.$rtl.isRTL) {
      this.i18n.locale = "en";
      this.$rtl.disableRTL();
    }
  },
  created() {
    this.pollOrderCount();
  },
};
</script>
<style>
</style>
