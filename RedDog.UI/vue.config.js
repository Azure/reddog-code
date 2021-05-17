const webpack = require("webpack");
const fetch = require("node-fetch");
// const axios = require('axios').default;


let MAKELINE_SERVICE = "http://127.0.0.1:5980/v1.0/invoke/make-line-service/method/orders/Redmond"
let ACCOUNTING_SERVICE = "http://127.0.0.1:5980/v1.0/invoke/accounting-service/method/OrderMetrics"

if (process.env.NODE_ENV === 'production'){
  console.log('setting prod environment variables')
  MAKELINE_SERVICE = "http://0.0.0.0:3500/v1.0/invoke/make-line-service/method/orders/Redmond"
  ACCOUNTING_SERVICE = "http://0.0.0.0:3500/v1.0/invoke/accounting-service/method/OrderMetrics"
}else{
  console.log('setting dev environment variables')
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
    host: "127.0.0.1",
    public: "127.0.0.1:8080",
    port: 8080,

    before: (app)=> {

      app.get('/orders/inflight', (req, res)=>{

        fetch(MAKELINE_SERVICE)
        .then(response => response.json())
        .then(data => {
          res.json(data).status(200)
        })

      })

      app.get('/orders/metrics', (req, res)=>{
        
        fetch(ACCOUNTING_SERVICE)
        .then(response => response.json())
        .then(data => {
          res.json(data).status(200)
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
