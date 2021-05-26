<template>
  <div class="container">
    <div class="row justify-content-lg-left">
      <div class="col-lg-6">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                ORDERS OVER TIME
              </div>
              <div class="card-body chart-body">
                <StreamChart v-if="loaded" :chartData="chartData" :options="chartOptions"/>
              </div>
              <div class="card-footer-title chart-options text-right">
                <a class="chart-segment" :class="{ activeSegment : orderChartSegment === 'MINUTE' }" v-on:click="orderChartUpdate('MINUTE')">
                  MINUTES
                </a>
                <a class="chart-segment" :class="{ activeSegment : orderChartSegment === 'HOUR' }" v-on:click="orderChartUpdate('HOUR')">
                  HOURS
                </a>
                <a class="chart-segment" :class="{ activeSegment : orderChartSegment === 'DAY' }" v-on:click="orderChartUpdate('DAY')">
                  DAYS
                </a>
              </div>
              </div>
            </div>
          </div>
        </div>
      <div class="col-lg-6">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                SALES OVER TIME
              </div>
              <div class="card-body chart-body">
                <StreamChart v-if="salesChartLoaded" :chartData="salesChartData" :options="salesChartOptions"/>
              </div>
              <!-- <div class="card-footer-title chart-options text-right">
                <a class="chart-segment" :class="{ activeSegment : orderChartSegment === 'MINUTE' }" v-on:click="orderChartUpdate('MINUTE')">
                  MINUTES
                </a>
                <a class="chart-segment" :class="{ activeSegment : orderChartSegment === 'HOUR' }" v-on:click="orderChartUpdate('HOUR')">
                  HOURS
                </a>
                <a class="chart-segment" :class="{ activeSegment : orderChartSegment === 'DAY' }" v-on:click="orderChartUpdate('DAY')">
                  DAYS
                </a>
              </div> -->
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="row justify-content-lg-left">
      <div class="col-lg-3">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                TOTAL SALES <span class="card-header-subtitle">LAST 30 DAYS</span>
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">{{ totalSalesFormatted }}</div>
                <div class="card-footer-title text-right">
                  <!-- Footer stuff here -->
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    <div class="col-lg-3">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                TOTAL PROFIT
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">{{ totalProfitFormatted }}</div>
                <div class="card-footer-title text-right">
                  <!-- Footer stuff here -->
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-lg-3">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                PROFIT PER ORDER
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">{{ profitPerOrderFormatted }}</div>
                <div class="card-footer-title text-right">
                  <!-- Footer stuff here -->
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-lg-3">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                AVG ORDER FILL TIME
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">
                  {{ avgFulfillmentSec }}s
                </div>
                <div class="card-footer-title text-right">
                  <!-- Footer stuff here -->
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="row justify-content-lg-left">
      <!-- <div class="col-lg-4">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                <i class="tim-icons icon-cart text-warning"></i> IN-FLIGHT
              </div>
              <div class="card-body chart-body" v-if="inflightChart">
                <StreamChart v-if="loaded" :chartData="chartData" :options="chartOptions"/>
              </div>
              <div class="card-body" v-else>
                <div class="card-big-detail text-center">
                  {{ unfulfilledOrders }}
                </div>
              </div>
              <div class="card-footer-title text-right" v-on:click="switchInflight" v-if="inflightChart">
                <i class="tim-icons icon-sound-wave"></i>
              </div>
              <div class="card-footer-title text-right" v-on:click="switchInflight" v-else>
                <i class="tim-icons icon-sound-wave active"></i>
              </div>
              </div>
            </div>
          </div>
        </div> -->
      </div>
    </div>
  </div>
</template>
<script>
import moment from 'moment'
import currency from 'currency.js'
import Chart from 'chart.js'

Chart.defaults.global.defaultFontColor = '#e6fff9';
Chart.defaults.global.defaultFontFamily = "'Exo', sans-serif";
Chart.defaults.global.defaultFontSize = 14;
Chart.defaults.global.defaultFontStyle = 500;
// Chart.defaults.global.gridLines.display = false;

// Chart.defaults.plugins.legend.display = false;

import StreamChart from '../components/RedDog/StreamChart.vue'



export default {
  components: {
    StreamChart
  },
  data() {
    return {
      orderChartSegment: 1,
      orderChartSegmentName: 'MINUTES',
      orderChartInterval: null,
      loaded:false,
      inflightOrderArray:[],
      chartData:null,
      chartOptions: null,
      ctx: null,
      unfulfilledOrders: 0,
      avgFulfillmentSec: null,
      totalFulfillmentTime: null,
      fulfilledOrders: null,
      totalSales: null,
      totalSalesFormatted: null,
      totalCost: null,
      totalProfit: null,
      totalProfitFormatted: null,
      profitPerOrder: null,
      profitPerOrderFormatted: null,
      currentDateTime: "",
      pollingInFlightOrders: null,
      pollingOrderMetrics: null,
      salesChartData: null,
      salesChartOptions: null,
      salesChartLoaded: false
    };
  },
  computed: {
    enableRTL() {
      return this.$route.query.enableRTL;
    },
    isRTL() {
      return this.$rtl.isRTL;
    },
  },
  methods: {
    fillOrderChart(data, segment){
      let minuteLabels = [], dataValues = [], dataValuesPrev = [], previousArr = [], lastArr = []

      switch (segment) {
        case 'MINUTE':
          previousArr = data.values.slice(data.values.length-20, data.values.length-10)
          lastArr = data.values.slice(data.values.length-10, data.values.length) 
          break
        case 'HOUR':
          previousArr = data.values.slice(data.values.length-14, data.values.length-7)
          lastArr = data.values.slice(data.values.length-7, data.values.length)
          break
        case 'DAY':
          previousArr = data.values.slice(data.values.length-14, data.values.length-7)
          lastArr = data.values.slice(data.values.length-7, data.values.length)
          break
        default:
          previousArr = data.values.slice(data.values.length-20, data.values.length-10)
          lastArr = data.values.slice(data.values.length-10, data.values.length) 
          break
      }

      lastArr.forEach((lt,li) => {
        minuteLabels.push(moment(lt.pointInTime).add(-4, 'hours').format("h:mmA"))
        dataValues.push(lt.value)
        dataValuesPrev.push(previousArr[li].value)

        if(li=== lastArr.length-1){
          this.createOrderLineChart(minuteLabels, dataValues, dataValuesPrev)
        }
      });
    },
    fillSalesChart(labels, values){
      let outLabels = [], outValues = []
      if(labels.length> 10){
        outLabels = labels.slice(labels.length-10, labels.length)
        outValues = values.slice(labels.length-10, labels.length)
      }else{
        outLabels = labels
        outValues = values
      }
      this.createSalesLineChart(outLabels, outValues)
    },
    getCurrentDateTime() {
      var current = new Date();
      this.currentDateTime = current.toLocaleString();
    },
    getAccountingOrderMetrics() {
      this.pollingOrderMetrics = setInterval(() => {
        let salesLabels = [], salesValues = []
        fetch("/orders/metrics")
          .then((response) => response.json())
          .then((data) => {
              if(data.e === 0){
              this.fulfilledOrders = 0;
              this.avgFulfillmentSec = 0;
              this.totalFulfillmentTime = 0;
              this.totalSales = 0;
              this.totalCost = 0;
              this.totalProfit = 0;
              this.profitPerOrder = 0;

              data.payload.forEach((ord, index) => {
                salesLabels.push(moment(ord.orderDate).add(ord.orderHour, 'hours').format('M/D hA'))
                salesValues.push(ord.totalPrice)
                this.fulfilledOrders = this.fulfilledOrders + ord.orderCount;
                this.totalFulfillmentTime = this.totalFulfillmentTime + (ord.orderCount * ord.avgFulfillmentSec);
                this.totalSales = this.totalSales + ord.totalPrice;
                this.totalCost = this.totalCost + ord.totalCost;

                if (index === data.payload.length - 1) {
                  this.fillSalesChart(salesLabels, salesValues);
                  this.totalProfit = (this.totalSales - this.totalCost).toFixed(0); /// TOTAL PROFIT
                  this.totalProfitFormatted = currency(this.totalProfit, {precision:0}).format(); /// TOTAL PROFIT FORMATTEd
                  this.profitPerOrder = (this.totalProfit / this.fulfilledOrders).toFixed(2) /// PROFIT PER ORDER 
                  this.profitPerOrderFormatted = currency(this.profitPerOrder, {precision:2}).format(); /// PROFIT PER ORDER FORMATTED 
                  this.avgFulfillmentSec = (this.totalFulfillmentTime / this.fulfilledOrders).toFixed(0);
                  this.totalSales = this.totalSales.toFixed(0); /// TOTAL SALES
                  this.totalSalesFormatted = currency(this.totalSales, {precision:0}).format(); /// TOTAL SALES FORMATTED
                }
              });
            }else{
              console.log('some kind of connection issue - you might want to get that looked at')
            }
          }
          );
      }, 10000);
    },
    getOrderChart(segment){
      let orderChartUrl = '/orders/count/minute'
      clearInterval(this.orderChartInterval)

      switch (segment) {
        case 'MINUTE':
          orderChartUrl = '/orders/count/minute';
          this.orderChartSegmentName = 'MINUTE';
          break;
        case 'HOUR':
          orderChartUrl = '/orders/count/hour';
          this.orderChartSegmentName = 'HOUR';
          break;
        case 'DAY':
          orderChartUrl = '/orders/count/day';
          this.orderChartSegmentName = 'DAY';
          break;
        default:
          orderChartUrl = '/orders/count/minute';
          this.orderChartSegmentName = 'MINUTE';
          break;
      }
      this.orderChartInterval = setInterval(() => {
        fetch(orderChartUrl)
          .then((response) => response.json())
          .then((data) => {
            if (data.e === 0 ) {
              this.fillOrderChart(data.payload, this.orderChartSegmentName);
            }else{
              console.log('some kind of connection issue - you might want to get that looked at')
            }
          });
      }, 10000);
    },
    // getInFlightOrderMetrics() {

    //   this.pollingInFlightOrders = setInterval(() => {
    //     fetch("/orders/byminute")
    //       .then((response) => response.json())
    //       .then((data) => {
    //         if (data.e === 0 ) {
    //           this.fillLineChart(data.payload);
    //           // this.unfulfilledOrders = data.payload.orderCount;
    //           this.getCurrentDateTime();
    //         }else{
    //           console.log('some kind of connection issue - you might want to get that looked at')
    //         }
    //       });
    //   }, 10000);
    // },
    createOrderLineChart(labels, totals, prevTotals, segment){
      this.chartData= {
        labels:labels,
        datasets: [
          {
            label: 'CURRENT',
            borderColor:'rgb(112, 162, 255)',
            backgroundColor: 'rgba(112, 162, 255, .05)',
            data: totals
          },
          {
            label: `PREVIOUS 10 ${this.orderChartSegmentName}`,
            borderColor:'rgb(227, 121, 0)',
            backgroundColor: 'rgba(227, 121, 0, .05)',
            data: prevTotals
          }
        ]
      };

      this.chartOptions = {
        legend: {
            display: true,
            position: 'bottom'
         },
         scales: {
          yAxes: [{
            ticks: {
              // stepSize: 2,
              // min:0,
              autoSkip: true,
              //reverse: false,
              beginAtZero: false,
              padding: 14
            },
            gridLines: {
              display: true,
              color: "rgba(150,150,150, .05)"
            },
          }],
          xAxes: [{
            ticks: {
              autoSkip: true,
              maxRotation: 90,
              minRotation: 90,
              padding: 14
            },
            gridLines: {
              display: true ,
              color: "rgba(150,150,150, .05)"
            },
          }]
        },
        responsive: true,
        // maintainAspectRatio: false
      };
      this.loaded = true;
    },


    createSalesLineChart(labels, values){
      this.salesChartData= {
        labels:labels,
        datasets: [
          {
            label: 'US DOLLARS',
            borderColor:'rgb(0, 227, 53)',
            backgroundColor: 'rgba(0, 227, 53, .05)',
            data: values
          },
        ]
      };

      this.salesChartOptions = {
        legend: {
            display: true,
            position: 'bottom'
         },
         scales: {
          yAxes: [{
            ticks: {
              // stepSize: 2,
              // min:0,
              autoSkip: true,
              //reverse: false,
              //beginAtZero: true,
              padding: 14
            },
            gridLines: {
              display: true,
              color: "rgba(150,150,150, .05)"
            },
          }],
          xAxes: [{
            ticks: {
              autoSkip: true,
              maxRotation: 90,
              minRotation: 90,
              padding: 14
            },
            gridLines: {
              display: true ,
              color: "rgba(150,150,150, .05)"
            },
          }]
        },
        responsive: true,
        // maintainAspectRatio: false
      };
      this.salesChartLoaded = true;

    },
    orderChartUpdate(segment){

      console.log('updating chart to ', segment )
      if(this.orderChartSegment != segment){
        this.orderChartSegment = segment;
        this.getOrderChart(this.orderChartSegment)
      }

    }

  },
  mounted() {
    this.i18n = this.$i18n;
    if (this.enableRTL) {
      this.i18n.locale = "ar";
      this.$rtl.enableRTL();
    }
  },
  beforeDestroy() {
    clearInterval(this.pollingInFlightOrders);
    clearInterval(this.pollingOrderMetrics);

    if (this.$rtl.isRTL) {
      this.i18n.locale = "en";
      this.$rtl.disableRTL();
    }
  },
  created() {
    this.getOrderChart(this.orderChartSegment);
    this.getAccountingOrderMetrics();
  },
};
</script>
<style>
</style>
