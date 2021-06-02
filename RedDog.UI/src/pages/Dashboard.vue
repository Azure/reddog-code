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
              </div>
            </div>
          </div>
        </div>
      <div class="col-lg-6">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                ORDER QUEUE
              </div>
              <div class="card-table-par">
                <div class="table-responsive">
                  <table id="tblInflight" class="table table-striped table-dark table-fixed">
                    <thead>
                      <tr>
                        <th scope="col" class="col-1 th-order">#</th>
                        <th scope="col" class="col-2 th-order">TIME</th>
                        <th scope="col" class="col-4 th-order">NAME</th>
                        <th scope="col" class="col-5 th-order th-count">ITEMS</th>
                      </tr>
                    </thead>
                    <tbody>
                      <!-- {{ this.inflight.forEach((item, index)) }} -->
                      <tr v-for="(order, i) in inflight" class="tr-item-queue">
                        <td scope="row" class="col-1 td-queue-position">{{ i+1 }}</td>  
                        <td class="col-2 td-time">{{ order.timeIn }}</td>  
                        <td class="col-4 td-name">{{ order.first + ' ' + order.last}}</td> 
                        <td class="col-5 td-count">
                          <ul v-for="oi in order.itemDetails" class="list-unstyled inflight-items">
                            <li><span class="td-product">{{oi.productName}}</span><span class="td-price">${{oi.unitPrice}}</span></li>
                          </ul>
                        </td>  
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="row justify-content-lg-left">
      <div class="col-lg-6">
        <div class="row">
          <div class="col-lg-12">
            <div class="card card-trans-base">
              <div class="card-header-title">
                P & L OVER TIME
              </div>
              <div class="card-body chart-body">
                <StreamChart v-if="salesChartLoaded" :chartData="salesChartData" :options="salesChartOptions"/>
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
                AVG PROFIT
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">{{ profitPerOrderFormatted }}</div>
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
                <div class="card-big-detail text-center">{{ avgFulfillmentSec }} min</div>
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
      inflight:[],
      pollingInflight:null,
      chartData:null,
      chartOptions: null,
      ctx: null,
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
    fillOrderChart(data){
      let minuteLabels = [], dataValues = [], dataValuesPrev = [], previousArr = [], lastArr = []
      
      previousArr = data.values.slice(data.values.length-20, data.values.length-10)
      lastArr = data.values.slice(data.values.length-10, data.values.length) 

      lastArr.forEach((lt,li) => {
        minuteLabels.push(moment(lt.pointInTime).add(-4, 'hours').format("h:mmA"))
        dataValues.push(lt.value)
        if (previousArr.length > 0){
          dataValuesPrev.push(previousArr[li].value)
        }else{
          dataValuesPrev.push(0)
        }
        if(li=== lastArr.length-1){
          this.createOrderLineChart(minuteLabels, dataValues, dataValuesPrev)
        }
      });
    },
    getCurrentDateTime() {
      var current = new Date();
      this.currentDateTime = current.toLocaleString();
    },
    getAccountingOrderMetrics() {
      clearInterval(this.pollingOrderMetrics)
      this.pollingOrderMetrics = setInterval(() => {
        let salesLabels = [], salesValues = [], profitLabels =[], profitValues=[]
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
                salesLabels.push(moment(ord.orderDate).add(ord.orderHour, 'hours').add(-4, 'hours').format('M/D hA'))
                salesValues.push(ord.totalPrice)
                profitLabels.push(moment(ord.orderDate).add(ord.orderHour, 'hours').add(-4, 'hours').format('M/D hA'))
                profitValues.push(ord.totalPrice-ord.totalCost)
                this.fulfilledOrders = this.fulfilledOrders + ord.orderCount;
                this.totalFulfillmentTime = this.totalFulfillmentTime + (ord.orderCount * ord.avgFulfillmentSec);
                this.totalSales = this.totalSales + ord.totalPrice;
                this.totalCost = this.totalCost + ord.totalCost;

                if (index === data.payload.length - 1) {
                  this.createSalesProfitLineChart(salesLabels, salesValues, profitValues);
                  this.totalProfit = (this.totalSales - this.totalCost).toFixed(0); /// TOTAL PROFIT
                  this.totalProfitFormatted = currency(this.totalProfit, {precision:0}).format(); /// TOTAL PROFIT FORMATTEd
                  this.profitPerOrder = (this.totalProfit / this.fulfilledOrders).toFixed(2) /// PROFIT PER ORDER 
                  this.profitPerOrderFormatted = currency(this.profitPerOrder, {precision:2}).format(); /// PROFIT PER ORDER FORMATTED 
                  // this.avgFulfillmentSec = (this.totalFulfillmentTime / this.fulfilledOrders).toFixed(0); /// AVG FULFILLMENT TIME
                  this.avgFulfillmentSec = moment.duration((this.totalFulfillmentTime / this.fulfilledOrders).toFixed(0), "seconds").minutes();
                  this.totalSales = this.totalSales.toFixed(0); /// TOTAL SALES
                  this.totalSalesFormatted = currency(this.totalSales, {precision:0}).format(); /// TOTAL SALES FORMATTED
                }
              });
            }else{
              console.log('some kind of connection issue - you might want to get that looked at')
            }
          }
          );
      }, 3000);
    },

    getOrderChart(){
      clearInterval(this.orderChartInterval)
      let orderChartUrl = '/orders/count/minute'
      this.orderChartInterval = setInterval(() => {
        fetch(orderChartUrl)
          .then((response) => response.json())
          .then((data) => {
            if (data.e === 0 ) {
              this.fillOrderChart(data.payload);
            }else{
              console.log('some kind of connection issue - you might want to get that looked at')
            }
          });
      }, 3000);
    },
    getCurrentOrders(){
      clearInterval(this.pollingInflight)
      this.pollingInflight = setInterval(() => {
        fetch("/orders/inflight")
        .then((response) => response.json())
        .then((data) => {
          this.inflight = []
          data.payload.forEach((ord, index) => {
            this.inflight.push({
              id: ord.orderId,
              timeIn: moment(ord.orderDate).format('h:mmA'),
              first: ord.firstName,
              last: ord.lastName,
              itemCount: ord.orderItems.length,
              itemDetails: ord.orderItems
            })
            if(index === data.payload.length -1){
              console.log('loaded current orders')
            }
          })
        })
      }, 3000);
    },
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
    createSalesProfitLineChart(labels, salesValues, profitValues){
      this.salesChartData= {
        labels:labels,
        datasets: [
          {
            label: 'SALES',
            borderColor:'rgb(41, 219, 255)',
            backgroundColor: 'rgba(0, 227, 53, .05)',
            data: salesValues
          },
          {
            label: 'PROFIT',
            borderColor:'rgb(0, 227, 53)',
            backgroundColor: 'rgba(0, 227, 53, .05)',
            data: profitValues
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
    clearInterval(this.pollingOrderMetrics);
    clearInterval(this.pollingInflight);
    clearInterval(this.orderChartInterval);
    
    if (this.$rtl.isRTL) {
      this.i18n.locale = "en";
      this.$rtl.disableRTL();
    }
  },
  created() {
    document.title = process.env.VUE_APP_SITE_TITLE
    this.getOrderChart();
    this.getAccountingOrderMetrics();
    this.getCurrentOrders();
  },
};
</script>
<style>
</style>
