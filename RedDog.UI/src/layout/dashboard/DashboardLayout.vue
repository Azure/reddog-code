<template>
  <div class="wrapper">
    <side-bar :backgroundColor="sideBarColor">
      <template slot="links">
        <sidebar-link to="/dashboard" :name="$t('sidebar.dashboard')" icon="tim-icons icon-chart-pie-36"/>
        <sidebar-link to="/maps" :name="$t('sidebar.maps')" icon="tim-icons icon-globe-2"/>
      </template>
    </side-bar>
    <div class="main-panel">
      <top-navbar></top-navbar>
      <dashboard-content @click.native="toggleSidebar">
      </dashboard-content>
      <content-footer></content-footer>
    </div>
  </div>
</template>
<style lang="scss">
</style>
<script>
import TopNavbar from "./TopNavbar.vue";
import ContentFooter from "./ContentFooter.vue";
import DashboardContent from "./Content.vue";
export default {
  components: {
    TopNavbar,
    ContentFooter,
    DashboardContent
  },
  data() {
    return {
      isCorp: (process.env.VUE_APP_IS_CORP || false),
      sideBarColor: "branch"
    }
  },
  methods: {
    toggleSidebar() {
      if (this.$sidebar.showSidebar) {
        this.$sidebar.displaySidebar(false);
      }
    },
  },
  beforeDestroy() {
    },
  created() {
    if(this.isCorp === true || this.isCorp === 'true'){
      this.sideBarColor = "corp"
    }
    else{
      this.sideBarColor = "branch"
    }
  },
};
</script>
<style>
</style>
