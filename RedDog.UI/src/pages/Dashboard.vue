<template>
  <div class="container">
    <div class="row justify-content-lg-left">
      <div class="col-lg-6">
        <div class="row">
          <div class="col-lg-12" v-if="isCorp === true || isCorp === 'true'">
            <div class="card" :class="[cardClass]">
              <div class="card-header-title">
                ORDERS / ITEMS OVER TIME <small>ALL LOCATIONS</small>
              </div>
              <div class="card-body chart-body">
                <StreamChart v-if="corpLoaded" :chartData="corpChartData" :options="corpChartOptions"/>
              </div>
            </div>
          </div>
           <div class="col-lg-12" v-else>
            <div class="card" :class="[cardClass]">
              <div class="card-header-title">
                ORDERS OVER TIME <small>VERSUS 10 MINUTES AGO</small>
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
            <div class="card" :class="[cardClass]">
              <div class="card-header-title" v-if="isCorp === true || isCorp === 'true'">
                SALES AND PROFIT <small>ALL LOCATIONS</small>
              </div>
              <div class="card-header-title" v-else>
                SALES AND PROFIT <small>FOR {{storeId.toUpperCase()}} LOCATION</small>
              </div>
              <div class="card-body chart-body">
                <StreamChart v-if="salesChartLoaded" :chartData="salesChartData" :options="salesChartOptions"/>
              </div>
              </div>
            </div>
          </div>
      </div>
    </div>
    <div class="row justify-content-lg-left">
      <div class="col-lg-6">
        <div class="row">
          <div class="col-lg-12" v-if="isCorp === true || isCorp === 'true'">
            <div class="card" :class="[cardClass]">
             <div class="card-header-title">
                TOP STORE SALES <small>OVER TIME</small>
              </div>
              <div class="card-body chart-body">
                <StreamChart v-if="topStoreSalesLoaded" :chartData="topSalesChartData" :options="topSalesChartOptions"/>
              </div>
            </div>
          </div>
          <div class="col-lg-12" v-else>
           <div class="card" :class="[cardClass]">
              <div class="card-header-title">
                ORDER QUEUE <small>BEING PREPARED</small>
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
      <div class="col-lg-3">
        <div class="row">
          <div class="col-lg-12">
            <div class="card" :class="[cardClass]">
              <div class="card-header-title">
                PROFIT / ORDER <small>LAST HOUR</small>
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
            <div class="card" :class="[cardClass]">
              <div class="card-header-title">
                FULFILLMENT <small>AVG TIME</small>
              </div>
              <div class="card-body">
                <div class="card-big-detail text-center">{{ avgFulfillmentSec }}{{ fulfillmentTimeDesc }}</div>
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

Chart.defaults.global.defaultFontColor = '#c0c0c0';
Chart.defaults.global.defaultFontFamily = "'Exo', sans-serif";
Chart.defaults.global.defaultFontSize = 11;
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
      isCorp: (process.env.VUE_APP_IS_CORP || false),
      cardClass: "card-trans-branch",
      orderChartSegment: 1,
      orderChartSegmentName: 'MINUTES',
      orderChartInterval: null,
      loaded:false,
      inflight:[],
      pollingInflight:null,
      chartData:null,
      chartOptions: null,
      corpLoaded:false,
      corpChartData: null,
      corpChartOptions: null,
      ctx: null,
      avgFulfillmentSec: null,
      fulfillmentTimeDesc: ' sec',
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
      pollingSalesProfit: null,
      pollingSalesProfitCorp:null,
      pollingTopStoresMetrics: null,
      salesChartData: null,
      salesChartOptions: null,
      salesChartLoaded: false,
      storeId: "CORP",
      topStoreSalesLoaded: false,
      topSalesChartData: null,
      topSalesChartOptions: null

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
    fillBranchOrderChart(data){
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
    fillCorpOrderChart(data){

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
                this.fulfilledOrders = this.fulfilledOrders + ord.orderCount;
                this.totalFulfillmentTime = this.totalFulfillmentTime + (ord.orderCount * ord.avgFulfillmentSec);
                this.totalSales = this.totalSales + ord.totalPrice;
                this.totalCost = this.totalCost + ord.totalCost;

                if (index === data.payload.length - 1) {
                  // this.totalProfit = (this.totalSales - this.totalCost).toFixed(0); /// TOTAL PROFIT
                  // this.totalProfitFormatted = currency(this.totalProfit, {precision:0}).format(); /// TOTAL PROFIT FORMATTEd
                  // this.profitPerOrder = (this.totalProfit / this.fulfilledOrders).toFixed(2) /// PROFIT PER ORDER 
                  // this.profitPerOrderFormatted = currency(this.profitPerOrder, {precision:2}).format(); /// PROFIT PER ORDER FORMATTED 
                  

                  // FOR DEMO ONLY
                  // console.log(`Fulfillment Time: ${this.totalFulfillmentTime}`)
                  // TODO: LYNN ORRELL DEMO 
                  this.avgFulfillmentSec = moment.duration((this.totalFulfillmentTime / this.fulfilledOrders).toFixed(0), "seconds").seconds();
                  if(this.avgFulfillmentSec > 60){
                    this.fulfillmentTimeDesc = ' min'
                    this.avgFulfillmentSec = (this.avgFulfillmentSec / 60).toFixed(0)
                  }
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
    getOrderChartBranch(){
      clearInterval(this.orderChartInterval)
      let orderChartUrl = '/orders/count/minute'
      this.orderChartInterval = setInterval(() => {
        fetch(orderChartUrl)
          .then((response) => response.json())
          .then((data) => {
            if (data.e === 0 ) {
              this.fillBranchOrderChart(data.payload);
            }else{
              console.log('some kind of connection issue - you might want to get that looked at')
            }
          });
      }, 10000);
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
              // console.log('loaded current orders')
            }
          })
        })
      }, 10000);
    },
    getSalesProfitMetricsBranch(){
      clearInterval(this.pollingSalesProfit)

      this.pollingSalesProfit = setInterval(() => {
        let salesLabels = [], salesValues = [], profitValues = [], modSaleslabels = [], modSalesValues = [], modProfitValues = []
        
        fetch("/corp/salesprofitbranch")
        .then((response) => response.json())
        .then((data) => {

          if(data.e === 0){
            
              // "storeId": "Redmond",
              // "orderYear": 2021,
              // "orderMonth": 6,
              // "orderDay": 9,
              // "orderHour": 23,
              // "totalOrders": 705,
              // "totalOrderItems": 45131,
              // "totalSales": 1396845.7600,
              // "totalProfit": 489346.5500
            
            data.payload.forEach((ord, index) => {
                if(ord.storeId === this.storeId){
                  salesLabels.push(moment(`${ord.orderMonth}-${ord.orderDay}-${ord.orderYear}`, "MM-DD-YYYY").add(ord.orderHour, 'hours').add(-4, 'hours').format('M/D hA'))
                  salesValues.push(ord.totalSales.toFixed(0))
                  profitValues.push(ord.totalProfit.toFixed(0))
                  this.profitPerOrderFormatted = currency((ord.totalProfit / ord.totalOrders), {precision:2}).format()
                }
                if (index === data.payload.length - 1) {
                  
                  modSaleslabels = salesLabels.slice(salesLabels.length-10, salesLabels.length)
                  modSalesValues = salesValues.slice(salesValues.length-10, salesValues.length)
                  modProfitValues = profitValues.slice(profitValues.length-10, profitValues.length)
                  this.createSalesProfitLineChart(modSaleslabels, modSalesValues, modProfitValues);
                }
            })

          }
          else{
            console.log('some kind of connection issue - you might want to get that looked at')
          }


        })

      }, 10000);


    },
    getSalesProfitMetricsCorp(){
      clearInterval(this.pollingSalesProfitCorp)

      this.pollingSalesProfitCorp = setInterval(() => {
        let salesLabels = [], salesValues = [], profitValues = [], modSaleslabels = [], modSalesValues = [], modProfitValues = []
        let orderLabels = [], orderValues = [], itemValues = [], modOrderLabels = [], modOrderValues = [], modItemValues = []
        
        fetch("/corp/salesprofitcorp")
        .then((response) => response.json())
        .then((data) => {

          if(data.e === 0){
            data.payload.forEach((ord, index) => {
                
                salesLabels.push(moment(`${ord.orderMonth}-${ord.orderDay}-${ord.orderYear}`, "MM-DD-YYYY").add(ord.orderHour, 'hours').add(-4, 'hours').format('M/D hA'))
                
                salesValues.push(ord.totalSales.toFixed(0))
                profitValues.push(ord.totalProfit.toFixed(0))

                orderLabels.push(moment(`${ord.orderMonth}-${ord.orderDay}-${ord.orderYear}`, "MM-DD-YYYY").add(ord.orderHour, 'hours').add(-4, 'hours').format('M/D hA'))
                orderValues.push(ord.totalOrders)
                itemValues.push(ord.totalOrderItems)



                if (index === data.payload.length - 1) {

                  this.profitPerOrderFormatted = currency((ord.totalProfit / ord.totalOrders), {precision:2}).format()



                  modSaleslabels = salesLabels.slice(salesLabels.length-10, salesLabels.length)
                  modSalesValues = salesValues.slice(salesValues.length-10, salesValues.length)
                  modProfitValues = profitValues.slice(profitValues.length-10, profitValues.length)


                  modOrderLabels = orderLabels.slice(orderLabels.length-10, orderLabels.length)
                  modOrderValues = orderValues.slice(orderValues.length-10, orderValues.length)
                  modItemValues = itemValues.slice(itemValues.length-10, itemValues.length)
                  
                  this.createSalesProfitLineChart(modSaleslabels, modSalesValues, modProfitValues);
                  this.createCorpOrderLineChart(modOrderLabels, modOrderValues, modItemValues);
                }

            })

          }
          else{
            console.log('some kind of connection issue - you might want to get that looked at')
          }

        })

      }, 10000);
    },
    getTopStoresMetrics(){
      clearInterval(this.pollingTopStoresMetrics)
      this.pollingTopStoresMetrics = setInterval(() => {
        fetch("/corp/stores")
        .then((response) => response.json())
        .then((data) => {
          if(data.e === 0){
            this.getPerStoreMetrics(data.payload)
          }
          else{
            console.log('some kind of connection issue - you might want to get that looked at')
          }

        })


      }, 10000)
    },
    getPerStoreMetrics(stores){
      let arrDatasets = [], arrLabels = [], payloadData = []
      fetch("/corp/salesprofitbranch")
        .then((response) => response.json())
        .then((data) => {

          if(data.e === 0){
            payloadData = data.payload
            stores.forEach((storeId, index) => {
              var filterDs = payloadData.filter(e=>e.storeId === storeId);
              var lastTen
              if(filterDs.length >= 10){
                lastTen = filterDs.slice(filterDs.length -10, filterDs.length)
                arrLabels = filterDs.slice(filterDs.length -10, filterDs.length)
              }
              else{
                lastTen = filterDs
              }
              arrDatasets.push(lastTen)

              if(index === stores.length-1){
                this.createCorpTopSalesChart(arrLabels, arrDatasets)
              }
            })
          }
          else{
            console.log('some kind of connection issue - you might want to get that looked at')
          }
        })

    },
    createOrderLineChart(labels, totals, prevTotals){
      this.chartData= {
        labels:labels,
        datasets: [
          {
            label: 'CURRENT',
            borderColor:'rgb(112, 162, 255)',
            backgroundColor: 'rgba(112, 162, 255, .05)',
            data: totals
          }
          ,
          {
            label: `PREVIOUS 10`,
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
          yAxes: {
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
          },
          xAxes: {
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
          }
        },
        responsive: true,
        // maintainAspectRatio: false
      };
      this.loaded = true;
    },
    createCorpOrderLineChart(labels, orders, items){

      this.corpChartData= {
        labels:labels,
        datasets: [
          {
            label: 'ORDERS',
            borderColor:'rgb(214, 31, 255)',
            backgroundColor: 'rgba(214, 31, 255, .05)',
            data: orders
          },
          {
            label: 'ITEMS',
            borderColor:'rgb(255, 153, 36)',
            backgroundColor: 'rgba(255, 153, 36, .05)',
            data: items
          }
        ]
      };

      this.corpChartOptions = {
        legend: {
            display: true,
            position: 'bottom'
         },
         scales: {
          yAxes: {
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
          },
          xAxes: {
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
          }
        },
        responsive: true,
        // maintainAspectRatio: false
      };
      this.corpLoaded = true;

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
          yAxes: {
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
          },
          xAxes: {
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
          }
        },
        responsive: true,
        // maintainAspectRatio: false
      };
      this.salesChartLoaded = true;

    },
    createCorpTopSalesChart(labels, valuesArr){

      var tmpSalesOptions = {
        legend: {
            display: true,
            position: 'bottom',
            labels: {
                fontColor: 'rgb(220, 220, 220)'
            }
         },
         scales: {
          yAxes: {
            ticks: {
              autoSkip: true,
              beginAtZero: false,
            },
            gridLines: {
              display: false
            },
          },
          xAxes: {
            ticks: {
              autoSkip: false,
              maxRotation: 90,
              minRotation: 90,

            },
            gridLines: {
              display: false
            },
          }
        },
        responsive: true,
        // maintainAspectRatio: false
      }

      var tmpSalesChartData = {
        labels:null,
        datasets: []
      }

      let labelStrings = []
      labels.forEach((lb, ind) =>{
        var dtString = moment(`${lb.orderMonth}-${lb.orderDay}-${lb.orderYear}`, "MM-DD-YYYY").add(lb.orderHour, 'hours').add(-4, 'hours').format('M/D hA')
        labelStrings.push(dtString)
        if(ind === labels.length -1){
          tmpSalesChartData.labels = labelStrings.slice(labelStrings.length-10, labelStrings.length)
        }
      })

      valuesArr.forEach((val, valInd)=>{
        var chartColor = this.getRGBColorForChart(valInd)
        var chartDs = {label: val[0].storeId, borderColor: chartColor.borderColor, backgroundColor: chartColor.backgroundColor, data: val.map(v=>v.totalSales)  }
        tmpSalesChartData.datasets.push(chartDs)
        if(valInd === valuesArr.length -1){
          this.topSalesChartData = tmpSalesChartData;
          this.topSalesChartOptions = tmpSalesOptions;
          this.topStoreSalesLoaded = true;
        }
      })
      

    },
    getRGBColorForChart(index){
      let colors = [
        {borderColor:'rgb(43, 255, 227)', backgroundColor:'transparent'},
        {borderColor:'rgb(176, 33, 121)', backgroundColor:'transparent'},
        {borderColor:'rgb(0, 150, 48)', backgroundColor:'transparent'},
        {borderColor:'rgb(135, 102, 36)', backgroundColor:'transparent'},
        {borderColor:'rgb(143, 0, 191)', backgroundColor:'transparent'},
        {borderColor:'rgb(217, 215, 115)', backgroundColor:'transparent'},
        {borderColor:'rgb(77, 28, 255)', backgroundColor:'transparent'},
        {borderColor:'rgb(252, 182, 129)', backgroundColor:'transparent'},
        {borderColor:'rgb(62, 133, 163)', backgroundColor:'transparent'},
        {borderColor:'rgb(109, 33, 176)', backgroundColor:'transparent'}
      ];
      return colors[index]
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
    
    clearInterval(this.pollingOrderMetrics)
    clearInterval(this.pollingInflight)
    clearInterval(this.orderChartInterval)
    clearInterval(this.pollingSalesProfit)
    clearInterval(this.pollingSalesProfitCorp)
    clearInterval(this.pollingTopStoresMetrics)
    
    if (this.$rtl.isRTL) {
      this.i18n.locale = "en";
      this.$rtl.disableRTL();
    }
  },
  created() {

    if (process.env.VUE_APP_IS_CORP === true || process.env.VUE_APP_IS_CORP === 'true'){
      
      this.isCorp = true;
      this.getAccountingOrderMetrics();
      this.getSalesProfitMetricsCorp();
      this.getTopStoresMetrics();
      this.cardClass="card-trans-corp"

    }
    else{

      this.isCorp = false;
      this.storeId = process.env.VUE_APP_STORE_ID 
      this.getOrderChartBranch();
      this.getAccountingOrderMetrics();
      this.getSalesProfitMetricsBranch();
      this.getCurrentOrders();
      this.cardClass="card-trans-branch"

    }

  },
};
</script>
<style>
</style>
