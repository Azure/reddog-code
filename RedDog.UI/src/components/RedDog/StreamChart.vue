<script>
import { Line, mixins } from 'vue-chartjs'

export default {
  name: 'sales-chart',
  extends: Line,
  mixins: [mixins.reactiveProp],
  props: ['options', 'chart-id', 'width', 'height'],
  mounted () {
    this.renderChart(this.chartData, this.options)
  },
  created() {

  }
}
</script>