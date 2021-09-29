<template>
  <nav class="navbar navbar-expand-lg navbar-absolute" 
       :class="{'bg-white': showMenu, 'navbar-transparent': !showMenu}">
    <div class="container-fluid">
      <div class="navbar-wrapper">
        <div class="navbar-toggle d-inline" :class="{toggled: $sidebar.showSidebar}">
          <button type="button"
                  class="navbar-toggler"
                  aria-label="Navbar toggle button"
                  @click="toggleSidebar">
            <span class="navbar-toggler-bar bar1"></span>
            <span class="navbar-toggler-bar bar2"></span>
            <span class="navbar-toggler-bar bar3"></span>
          </button>
        </div>
        <div class="logo-content">
          <div class="left-logo"><img src="img/reddog-logo-text.png" width="184px"/></div>
          <div class="right-logo logo-simple-text" v-if="isCorp === true || isCorp === 'true'">CORP<small>headquarters</small></div>
          <div class="right-logo logo-simple-text" v-else>{{ storeId }}<small class="branch">branch</small></div>
        </div>
      </div>
      <button class="navbar-toggler" type="button"
              @click="toggleMenu"
              data-toggle="collapse"
              data-target="#navigation"
              aria-controls="navigation-index"
              aria-label="Toggle navigation">
        <span class="navbar-toggler-bar navbar-kebab"></span>
        <span class="navbar-toggler-bar navbar-kebab"></span>
        <span class="navbar-toggler-bar navbar-kebab"></span>
      </button>

      <collapse-transition>
        <div class="collapse navbar-collapse show" v-show="showMenu">
          <ul class="navbar-nav" :class="$rtl.isRTL ? 'mr-auto' : 'ml-auto'">
            <base-dropdown tag="li"
                           :menu-on-right="!$rtl.isRTL"
                           title-tag="a"
                           class="nav-item"
                           menu-classes="dropdown-navbar">
              <a slot="title" href="#" class="dropdown-toggle nav-link" data-toggle="dropdown" aria-expanded="true">
                <span class="site-name">{{ siteType }}</span>
                <div class="photo">
                  <img src="img/max-lord.png">
                </div>
                <!-- <b class="caret d-none d-lg-block d-xl-block"></b> -->
                <p class="d-lg-none">
                  Log out
                </p>
              </a>
              <li class="nav-link">
                <a href="#" class="nav-item dropdown-item">Life is good...</a>
              </li>
              <li class="nav-link">
                <a href="#" class="nav-item dropdown-item">But it could be better.</a>
              </li>
              <li class="nav-link">
                <a href="#" class="nav-item dropdown-item text-success">Batman sucks</a>
              </li>
              <div class="dropdown-divider"></div>
              <li class="nav-link">
                <a href="#" class="nav-item dropdown-item">Log out</a>
              </li>
            </base-dropdown>
          </ul>
        </div>
      </collapse-transition>
    </div>
  </nav>
</template>
<script>
  import { CollapseTransition } from 'vue2-transitions';
  import Modal from '@/components/Modal';

  export default {
    components: {
      CollapseTransition,
      Modal
    },
    computed: {
      routeName() {
        const { name } = this.$route;
        return this.capitalizeFirstLetter(name);
      },
      isRTL() {
        return this.$rtl.isRTL;
      }
    },
    data() {
      return {
        activeNotifications: false,
        showMenu: false,
        isCorp: (process.env.VUE_APP_IS_CORP || false),
        siteType: (process.env.VUE_APP_SITE_TYPE || 'PHARMACY-NOENV'),
        storeId: (process.env.VUE_APP_STORE_ID || 'REDMOND-NOENV'),
      };
    },
    methods: {
      capitalizeFirstLetter(string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
      },
      toggleNotificationDropDown() {
        this.activeNotifications = !this.activeNotifications;
      },
      closeDropDown() {
        this.activeNotifications = false;
      },
      toggleSidebar() {
        this.$sidebar.displaySidebar(!this.$sidebar.showSidebar);
      },
      hideSidebar() {
        this.$sidebar.displaySidebar(false);
      },
      toggleMenu() {
        this.showMenu = !this.showMenu;
      }
    },
    mounted(){
      // SET PAGE TITLE
      document.title = process.env.VUE_APP_SITE_TITLE

      // SET BODY STYLE
      if (process.env.VUE_APP_IS_CORP === true || process.env.VUE_APP_IS_CORP === 'true'){
        document.body.className = 'corp'
      }else{
        document.body.className = 'branch'
      }
    },
    created() {
    }
  };
</script>
<style>
</style>
