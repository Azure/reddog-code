const webpack = require("webpack");
// const fetch = require("node-fetch");

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
      
      // app.get("/currentOrders", (req, res) => {
      //   fetch("https://dapralgibbon.westus.cloudapp.azure.com/makeline/orders/Denver")
      //   .then(response => response.json())
      //   .then(data => {
      //     console.log(data)
      //     res.json(data).status(200)
      //   })
      // });
       
        
        
      app.get("/sales/monthly", (req, res) => {
        var monthlySales = [
          { month: "March", year: 2020, dollars: 128000 },
          { month: "April", year: 2020, dollars: 91000 },
          { month: "May", year: 2020, dollars: 102000 },
          { month: "June", year: 2020, dollars: 201000 },
          { month: "July", year: 2020, dollars: 191000 },
          { month: "August", year: 2020, dollars: 188000 },
          { month: "September", year: 2020, dollars: 175000 },
          { month: "October", year: 2020, dollars: 144000 },
          { month: "November", year: 2020, dollars: 168000 },
          { month: "December", year: 2020, dollars: 200000 },
          { month: "January", year: 2021, dollars: 225000 }
        ];
        res.json(monthlySales).status(200);
      });
      app.get("/traffic/monthly", (req, res) => {
        var monthlyTraffic = [
          { month: "March", year: 2020, visitors: 1800 },
          { month: "April", year: 2020, visitors: 1300 },
          { month: "May", year: 2020, visitors: 1990 },
          { month: "June", year: 2020, visitors: 1200 },
          { month: "July", year: 2020, visitors: 1500 },
          { month: "August", year: 2020, visitors: 1600 },
          { month: "September", year: 2020, visitors: 2800 },
          { month: "October", year: 2020, visitors: 2000 },
          { month: "November", year: 2020, visitors: 2200 },
          { month: "December", year: 2020, visitors: 3100 },
          { month: "January", year: 2021, visitors: 3500 }
        ];
        res.json(monthlyTraffic).status(200);
      });
    }
    // proxy: devUrls,
  },
  pwa: {
    name: "Vue Black Dashboard",
    themeColor: "#344675",
    msTileColor: "#344675",
    appleMobileWebAppCapable: "yes",
    appleMobileWebAppStatusBarStyle: "#344675"
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
