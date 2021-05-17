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
                <div class="card-big-detail text-center">${{ totalSales }}</div>
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
                <div class="card-big-detail text-center">${{ totalProfit }}</div>
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
                <div class="card-big-detail text-center">${{ profitPerOrder }}</div>
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
                <i class="tim-icons icon-cart text-warning"></i>ORDERS IN-FLIGHT
              </div>
              <div class="card-body">
                <StreamChart :chartdata="chartData" :options="chartOptions"/>
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

// // import fakeOrders from "../models/fakeOrderTotals.json";
// chart.defaults.font.family = "'Exo', sans-serif";
// chart.defaults.font.size = 12;
// chart.defaults.font.weight = 700;
// chart.defaults.plugins.legend.display = false;

import StreamChart from '../components/RedDog/StreamChart.vue'

export default {
  components: {
    StreamChart
  },
  data() {
    return {
      inflightOrderArray:[],
      chartData:null,
      chartOptions: null,
      ctx: null,
      unfulfilledOrders: 0,
      avgFulfillmentSec: null,
      totalFulfillmentTime: null,
      fulfilledOrders: null,
      totalSales: null,
      totalCost: null,
      totalProfit: null,
      profitPerOrder: null,
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
            // zero out the metrics
            this.fulfilledOrders = 0;
            this.avgFulfillmentSec = 0;
            this.totalFulfillmentTime = 0;
            this.totalSales = 0;
            this.totalCost = 0;
            this.totalProfit = 0;
            this.profitPerOrder = 0;

            data.forEach((ord, index) => {
              // console.log(ord);
              this.fulfilledOrders = this.fulfilledOrders + ord.orderCount;
              this.totalFulfillmentTime = this.totalFulfillmentTime + (ord.orderCount * ord.avgFulfillmentSec);
              this.totalSales = this.totalSales + ord.totalPrice;
              this.totalCost = this.totalCost + ord.totalCost;

              if (index === data.length - 1) {
                this.totalProfit = (this.totalSales - this.totalCost).toFixed(0) /// TOTAL PROFIT
                this.profitPerOrder = (this.totalProfit / this.fulfilledOrders).toFixed(2) /// PROFIT PER ORDERS
                this.avgFulfillmentSec = (this.totalFulfillmentTime / this.fulfilledOrders).toFixed(0);
                this.totalSales = this.totalSales.toFixed(0);
                
              }
            });
          });
      }, 5000);
    },
    getInFlightOrderMetrics() {
      this.pollingInFlightOrders = setInterval(() => {
        fetch("/orders/inflight")
          .then((response) => response.json())
          .then((data) => {
            this.fillLineChart(data);
            this.unfulfilledOrders = data.length;
            this.getCurrentDateTime();
          });
      }, 2000);
    },
    fillLineChart(orderData){
      // orderData.forEach((ord,ordIndex) => {
      //   let currentMinute = moment(ord.orderDate).format('MMM-Do h:mmA');
      //   console.log(currentMinute)
      // })
      let minuteLabels = [];
      for(var i=10; i>0; i--){
        var dt = moment().subtract(i, 'minutes');
        minuteLabels.push(dt.format('h:mmA'));
      }
      




      this.chartData= {
        labels:minuteLabels,
        datasets: [
          {
            label: 'Orders',
            borderColor:'rgb(132, 232, 137)',
            backgroundColor: 'transparent',
            data: [2,7,9,7,5]
          }
        ]
      }
      
      this.chartOptions = {
        legend: {
            display: false
         },
         scales: {
           yAxes: [{
            ticks: {
               min: 0,
               max: 10,
               stepSize: 2,
               reverse: false,
               beginAtZero: true
            }
          }],
           xAxes: [{
                ticks: {
                    autoSkip: false,
                    maxRotation: 90,
                    minRotation: 90
                }
            }]
        },
        responsive: true,
        maintainAspectRatio: false
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
    this.getInFlightOrderMetrics();
    this.getAccountingOrderMetrics();
    this.fillLineChart(null);
  },
};
</script>
<style>
</style>
