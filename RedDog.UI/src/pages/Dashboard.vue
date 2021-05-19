<template>
  <div class="container">
    <div class="row justify-content-lg-left">
      <div class="col-lg-4">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                <i class="tim-icons icon-cart text-warning"></i>ORDERS IN-FLIGHT
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">
                  {{ unfulfilledOrders }}
                </div>
                <div class="card-footer-title text-right">
                  <!-- Footer stuff here -->
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-lg-4">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                <i class="tim-icons icon-check-2 text-success"></i>FULFILLED
                ORDERS
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">
                  {{ fulfilledOrders }}
                </div>
                <div class="card-footer-title text-right">
                  <!-- Footer stuff here -->
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="col-lg-4">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                <i class="tim-icons icon-watch-time text-secondary"></i>AVG
                ORDER TIME
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
      <div class="col-lg-4">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                <i class="tim-icons icon-money-coins text-success"></i>TOTAL
                SALES
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
    <div class="col-lg-4">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                <i class="tim-icons icon-money-coins text-success"></i>TOTAL
                PROFIT
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
      <div class="col-lg-4">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                <i class="tim-icons icon-money-coins text-success"></i>PROFIT PER ORDER
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
    </div>
    <div class="row justify-content-lg-left">
      <div class="col-lg-4">
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
              <div class="card-footer-title text-right">
                <!-- Footer stuff here -->
              </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import moment from 'moment'
import currency from 'currency.js'

import Chart from 'chart.js'

Chart.defaults.global.defaultFontColor = '#FFF';
Chart.defaults.global.defaultFontFamily = "'Exo', sans-serif";
Chart.defaults.global.defaultFontSize = 10;
Chart.defaults.global.defaultFontStyle = 300;
// Chart.defaults.global.gridLines.display = false;

// Chart.defaults.plugins.legend.display = false;

import StreamChart from '../components/RedDog/StreamChart.vue'



export default {
  components: {
    StreamChart
  },
  data() {
    return {
      inflightChart: true,
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

    getCurrentDateTime() {
      var current = new Date();
      this.currentDateTime = current.toLocaleString();
    },
    getAccountingOrderMetrics() {
      this.pollingOrderMetrics = setInterval(() => {
        fetch("/orders/metrics")
          .then((response) => response.json())
          .then((data) => {
            console.log(data)
              if(data.e === 0){
              // zero out the metrics
              this.fulfilledOrders = 0;
              this.avgFulfillmentSec = 0;
              this.totalFulfillmentTime = 0;
              this.totalSales = 0;
              this.totalCost = 0;
              this.totalProfit = 0;
              this.profitPerOrder = 0;

              data.payload.forEach((ord, index) => {
                // console.log(ord);
                this.fulfilledOrders = this.fulfilledOrders + ord.orderCount;
                this.totalFulfillmentTime = this.totalFulfillmentTime + (ord.orderCount * ord.avgFulfillmentSec);
                this.totalSales = this.totalSales + ord.totalPrice;
                this.totalCost = this.totalCost + ord.totalCost;

                if (index === data.payload.length - 1) {
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
      }, 5000);
    },
    getInFlightOrderMetrics() {
      this.pollingInFlightOrders = setInterval(() => {
        fetch("/orders/inflight")
          .then((response) => response.json())
          .then((data) => {
            if (data.e === 0 ) {
              this.fillLineChart(data.payload.orderByMinute);
              this.unfulfilledOrders = data.payload.orderCount;
              this.getCurrentDateTime();
            }else{
              console.log('some kind of connection issue - you might want to get that looked at')
            }
          });
      }, 2000);
    },
    createOrderLineChart(minuteLabels, minuteTotals){
      this.chartData= {
        labels:minuteLabels,
        datasets: [
          {
            label: 'Orders',
            borderColor:'rgb(0, 255, 132)',
            backgroundColor: 'rgba(0, 255, 132, .05)',
            data: minuteTotals
          }
        ]
      };

      this.chartOptions = {
        legend: {
            display: false
         },
         scales: {
          yAxes: [{
            ticks: {
              stepSize: 2,
              min:0,
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
      this.loaded = true;


    },
    fillLineChart(orderData){

      let minuteLabels = [];
      let dataValues = [];

      orderData.forEach((o,i)=>{
        minuteLabels.push(o.prettyPrintTime)
        dataValues.push(o.total)
        var _ml, _dv
        if (i === orderData.length -1){
          if (minuteLabels.length >=10){
            _ml = minuteLabels.slice(minuteLabels.length -10, minuteLabels.length)
            _dv = dataValues.slice(minuteLabels.length -10, minuteLabels.length)
            this.createOrderLineChart(_ml, _dv)
          }else{
            this.createOrderLineChart(minuteLabels, dataValues)
          }
        }
      })
      
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
    this.getInFlightOrderMetrics();
    this.getAccountingOrderMetrics();
    // this.fillLineChart(null);
  },
};
</script>
<style>
</style>
