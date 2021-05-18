<script>
import { Line } from "vue-chartjs";

export default {
  extends: Line,
  props: {
    chartdata: {
      type: Object,
      default: null
    },
    options: {
      type: Object,
      default: null
    }
  },
  mounted () {
      this.renderChart(this.chartdata, this.options)
      console.log(this.$data._chart)
      // this.$data._chart.defaults.font.family = "'Exo', sans-serif";
  }
}


// // import fakeOrders from "../models/fakeOrderTotals.json";
// chart.defaults.font.family = "'Exo', sans-serif";
// chart.defaults.font.size = 12;
// chart.defaults.font.weight = 700;
// chart.defaults.plugins.legend.display = false;
</script>