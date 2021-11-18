const webpack = require("webpack");
const fetch = require("node-fetch");
const Dotenv = require("dotenv-webpack");

const IS_CORP_TMP = (process.env.VUE_APP_IS_CORP || false);
const IS_CORP = JSON.stringify(IS_CORP_TMP);

const STORE_ID = (process.env.VUE_APP_STORE_ID || "Redmond");
const SITE_TYPE = (process.env.VUE_APP_SITE_TYPE || "Pharmacy");
const SITE_TITLE = (process.env.VUE_APP_SITE_TITLE || "Red Dog Bodega :: Market fresh food, pharmaceuticals, and fireworks!");
const MAKELINE_BASE_URL = (process.env.VUE_APP_MAKELINE_BASE_URL || "http://austin.makeline.brianredmond.io");
const ACCOUNTING_BASE_URL = (process.env.VUE_APP_ACCOUNTING_BASE_URL || "http://austin.accounting.brianredmond.io");

let variables = {
  IS_CORP: IS_CORP,
  STORE_ID: STORE_ID,
  SITE_TYPE: SITE_TYPE,
  SITE_TITLE: SITE_TITLE,
  ACCOUNTING_BASE_URL: ACCOUNTING_BASE_URL,
  MAKELINE_BASE_URL: MAKELINE_BASE_URL
}

let MAKELINE_SERVICE = MAKELINE_BASE_URL + "/orders/" + STORE_ID


if (process.env.NODE_ENV === "production"){
  console.log("setting PROD environment variables")
}else{
  console.log("setting DEV environment variables")
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
      new Dotenv({
        path: ".env", // Path to .env file (this is the default)
        safe: false, // load .env.example (defaults to "false" which does not use dotenv-safe)
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

      app.get("/variables", (req, res)=>{
        res.json({e: -1, payload:variables}).status(200)
      })

      app.get("/orders/inflight", (req, res)=>{

        fetch(MAKELINE_SERVICE)
        .then(response => response.json())
        .then(data => {
          if( data.length >= 10){
            res.json({e: 0, payload:data.slice(data.length-10, data.length)}).status(200)
          }
          else{
            res.json({e: 0, payload:data}).status(200)
          }
          
        })
        .catch(error=>{
          console.log("error", error)
          res.json({e: -1, payload: {}})
        })

      })

      app.get("/orders/metrics", (req, res)=>{
        var metricsUrl = ACCOUNTING_BASE_URL + "/OrderMetrics?StoreId=" + STORE_ID
        if(IS_CORP === true || IS_CORP === "true"){
          metricsUrl = ACCOUNTING_BASE_URL + "/OrderMetrics"
        }

        fetch(metricsUrl)
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log("error", error)
          res.json({e: -1, payload: {}})
        })

      })

      app.get("/orders/count/minute", (req, res)=>{
        
        fetch(ACCOUNTING_BASE_URL + "/Orders/Minute/PT20M?StoreId=" + STORE_ID)
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log("error", error)
          res.json({e: -1, payload: {}})
        })

      })

      app.get("/corp/salesprofitbranch", (req, res)=>{

        fetch(ACCOUNTING_BASE_URL + "/Corp/SalesProfit/PerStore")
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log("error", error)
          res.json({e: -1, payload: {}})
        })
        
      })

      app.get("/corp/salesprofitcorp", (req, res)=>{

        fetch(ACCOUNTING_BASE_URL + "/Corp/SalesProfit/Total")
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log("error", error)
          res.json({e: -1, payload: {}})
        })
        
      })

      
      app.get("/corp/stores", (req, res)=>{

        fetch(ACCOUNTING_BASE_URL + "/Corp/Stores")
        .then(response => response.json())
        .then(data => {
          res.json({e: 0, payload:data}).status(200)
        })
        .catch(error=>{
          console.log("error", error)
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
