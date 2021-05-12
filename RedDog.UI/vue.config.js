const webpack = require("webpack");
const fetch = require("node-fetch");

module.exports = {
  lintOnSave: false,
  configureWebpack: {
    resolve: {
      alias: {
        "chart.js": "chart.js/dist/chart.js"
      }
    },
    plugins: [
      new webpack.optimize.LimitChunkCountPlugin({
        maxChunks: 6
      }),
      new webpack.DefinePlugin({
        "process.env": {
          // put environment variables needed here
          'MAKELINE_SERVICE_ROOT': 'http://0.0.0.0:5200/',
          'ACCOUNTING_SERVICE_ROOT': 'http://0.0.0.0:5700/OrderMetrics',
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

    before: function(app) {
      
      app.get('/orders/inflight', (req, res)=>{
        fetch('http://0.0.0.0:5200/orders/Redmond')
        .then(response => response.json())
        .then(data => {
          res.json(data).status(200)
        })
      })

      app.get('/orders/metrics', (req, res)=>{
        fetch('http://0.0.0.0:5700/OrderMetrics')
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
