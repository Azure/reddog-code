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
                <div class="card-big-detail text-center">{{ fulfilledOrders }}</div>
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
                <div class="card-big-detail text-center">{{ avgFulfillmentSec }}s</div>
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
                <i class="tim-icons icon-satisfied text-info"></i>FULFILLMENT
                GOAL
              </div>
              <div class="card-body chart-doughnut-canvas">
                <!-- <canvas id="compOrdersChart" width="40vh" height="40vh"></canvas> -->
                <canvas id="compOrdersChart"></canvas>
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
              <div class="card-header-title">
                <i class="tim-icons icon-camera-18 text-danger"></i>IN-STORE
                TRAFFIC
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">12</div>
                <div class="card-footer-title text-right">
                  <i class="tim-icons icon-refresh-01"></i>
                </div>
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
import chart from "chart.js";

// import fakeOrders from "../models/fakeOrderTotals.json";
chart.defaults.font.family = "'Exo', sans-serif";
chart.defaults.font.size = 12;
chart.defaults.font.weight = 700;
chart.defaults.plugins.legend.display = false;

export default {
  components: {},
  data() {
    return {
      ctx: null,
      unfulfilledOrders: 0,
      avgFulfillmentSec: null,
      totalFulfillmentTime: null,
      fulfilledOrders: null,
      currentDateTime: "",
      pollingInFlightOrders: null,
      pollingOrderMetrics: null,
      totalOrders: null,
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
    },
  },
  methods: {
    getCurrentDateTime() {
      var current = new Date();
      this.currentDateTime = current.toLocaleString();
    },
    getAccountingOrderMetrics(){
      
      this.pollingOrderMetrics = setInterval(() => {
        fetch("/orders/metrics")
        .then((response) => response.json())
        .then((data) => {
          // zero out the metrics
          this.fulfilledOrders = 0;
          this.avgFulfillmentSec = 0;
          this.totalFulfillmentTime = 0;
          data.forEach((ord, index)=>{
            this.fulfilledOrders = this.fulfilledOrders + ord.orderCount
            this.totalFulfillmentTime = this.totalFulfillmentTime + (ord.orderCount * ord.avgFulfillmentSec)
            if (index===data.length-1){
              this.avgFulfillmentSec = (this.totalFulfillmentTime / this.fulfilledOrders)
              this.avgFulfillmentSec = this.avgFulfillmentSec.toFixed(0);
              console.log(this.avgFulfillmentSec)
            }
          })
        });
      }, 5000);
    },
    getInFlightOrderMetrics() {
      this.pollingInFlightOrders = setInterval(() => {
        fetch("/orders/inflight")
          .then((response) => response.json())
          .then((data) => {
            this.unfulfilledOrders = data.length;
            this.getCurrentDateTime();
          });
      }, 5000);
    },
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
  },
};
</script>
<style>
</style>
