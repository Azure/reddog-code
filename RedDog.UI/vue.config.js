const webpack = require("webpack");
const fetch = require("node-fetch");

const STORE_ID = process.env.STORE_ID || "Redmond";
const SITE_TYPE = process.env.SITE_TYPE || 'Pharmacy';
const SITE_TITLE = process.env.SITE_TITLE || 'Contoso :: Pharmacy & Convenience Store';
let variables = {
  STORE_ID: STORE_ID,
  SITE_TYPE: SITE_TYPE,
  SITE_TITLE: SITE_TITLE
 }

let MAKELINE_SERVICE = "http://127.0.0.1:5980/v1.0/invoke/make-line-service/method/orders/" + STORE_ID
let ACCOUNTING_SERVICE = "http://127.0.0.1:5980/v1.0/invoke/accounting-service/method/"

if (process.env.NODE_ENV === 'production'){
  console.log('setting PROD environment variables')
  MAKELINE_SERVICE = "http://0.0.0.0:3500/v1.0/invoke/make-line-service/method/orders/" + STORE_ID
  ACCOUNTING_SERVICE = "http://0.0.0.0:3500/v1.0/invoke/accounting-service/method/"
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

      app.get('/variables', (req, res)=>{

        res.json({e: -1, payload:variables}).status(200)

      })

      app.get('/orders/inflight', (req, res)=>{

        fetch(MAKELINE_SERVICE)
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log('error', error)
          res.json({e: -1, payload: {}})
        })

      })

      app.get('/orders/metrics', (req, res)=>{
        
        fetch(ACCOUNTING_SERVICE + 'OrderMetrics?StoreId=' + STORE_ID)
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log('error', error)
          res.json({e: -1, payload: {}})
        })

      })

      app.get('/orders/count/minute', (req, res)=>{
        
        fetch(ACCOUNTING_SERVICE + 'Orders/Minute/PT20M?StoreId=' + STORE_ID)
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
        
        fetch(ACCOUNTING_SERVICE + 'Orders/Hour/P1D?StoreId=' + STORE_ID)
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
        
        fetch(ACCOUNTING_SERVICE + 'Orders/Day/P2D?StoreId=' + STORE_ID)
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
