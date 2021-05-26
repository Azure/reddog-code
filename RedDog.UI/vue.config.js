const webpack = require("webpack");
const fetch = require("node-fetch");
const moment = require("moment");
// const axios = require('axios').default;

// http://{{accounting-service}}/Orders/Minute?StoreId=Redmond
let MAKELINE_SERVICE = "http://127.0.0.1:5980/v1.0/invoke/make-line-service/method/orders/Redmond"
let ACCOUNTING_SERVICE = "http://127.0.0.1:5980/v1.0/invoke/accounting-service/method/"

if (process.env.NODE_ENV === 'production'){
  console.log('setting PROD environment variables')
  MAKELINE_SERVICE = "http://0.0.0.0:3500/v1.0/invoke/make-line-service/method/orders/Redmond"
  ACCOUNTING_SERVICE = "http://0.0.0.0:3500/v1.0/invoke/accounting-service/method/OrderMetrics"
}else{
  console.log('setting DEV environment variables')
  console.log(MAKELINE_SERVICE)
  console.log(ACCOUNTING_SERVICE)
}


module.exports = {
  lintOnSave: false,
  configureWebpack: {
    resolve: {
      alias: {
        // "chart.js": "chart.js/dist/chart.js"
      }
    },
    plugins: [
      new webpack.optimize.LimitChunkCountPlugin({
        maxChunks: 6
      }),
      new webpack.DefinePlugin({
        "process.env": {
        }
      })
    ]
  },
  devServer: {
    headers: {
      "Access-Control-Allow-Origin": "*",
      "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, PATCH, OPTIONS",
      "Access-Control-Allow-Headers":
        "X-Requested-With, content-type, Authorization"
    },
    disableHostCheck: true,
    host: "0.0.0.0",
    public: "0.0.0.0:8080",
    port: 8080,
    before: (app)=> {

      app.get('/orders/inflight', (req, res)=>{

        let outData=[];
        let orderMinutes={};
        fetch(MAKELINE_SERVICE)
        .then(response => response.json())
        .then(data => {
          data.forEach((o,i)=>{
            var propName = `${moment(o.orderDate).format("YYYYMMDDHHmm")}`.toString();
            if (orderMinutes.hasOwnProperty(propName)){
              orderMinutes[propName].total = (orderMinutes[propName].total + 1);
            }else{
              orderMinutes[propName] = { total: 1, prettyPrintTime: moment(o.orderDate).format('h:mma') }
            }

            
            if (i === data.length -1){
              let orderKeys = Object.keys(orderMinutes);
              for(var k=0; k<orderKeys.length; k++){
                outData.push(orderMinutes[orderKeys[k]])
                if (k === orderKeys.length -1 ){
                  res.json({e: 0, payload:{ orderCount: data.length, orderByMinute: outData}}).status(200)
                }
              }
            }
          })
          
        })
        .catch(error=>{
          res.json({e: -1, payload: { orderCount:100, orderByMinute: []}})
        })

      })

      app.get('/orders/metrics', (req, res)=>{
        
        fetch(ACCOUNTING_SERVICE + 'OrderMetrics')
        .then(response => response.json())
        .then(data => {
          console.log(data)
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log('error', error)
          res.json({e: -1, payload: {}})
        })

      })

      app.get('/orders/count/minute', (req, res)=>{
        
        fetch(ACCOUNTING_SERVICE + 'Orders/Minute/PT3H?StoreId=Redmond')
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log('error', error)
          res.json({e: -1, payload: {}})
        })

      })


      app.get('/orders/count/hour', (req, res)=>{
        
        fetch(ACCOUNTING_SERVICE + 'Orders/Hour/PT72H?StoreId=Redmond')
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log('error', error)
          res.json({e: -1, payload: {}})
        })

      })

      app.get('/orders/count/day', (req, res)=>{
        
        fetch(ACCOUNTING_SERVICE + 'Orders/Day/P14D?StoreId=Redmond')
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log('error', error)
          res.json({e: -1, payload: {}})
        })

      })
     
    }
    // proxy: devUrls,
  },
  pluginOptions: {
    i18n: {
      locale: "en",
      fallbackLocale: "en",
      localeDir: "locales",
      enableInSFC: false
    }
  },
  css: {
    sourceMap: process.env.NODE_ENV !== "production"
  }
};
