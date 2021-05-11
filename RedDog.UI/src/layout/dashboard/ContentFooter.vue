<template>
  <footer class="footer">
    <div class="container-fluid">
      <ul class="nav">
        <li class="nav-item pl-2">
          <a class="nav-link" href="https://github.com/cloudnativegbb/paas-vnext" target="_blank">
            steal this code <img src="img/GitHub-Mark-Light-64px.png" height="30">
          </a>
        </li>
        <li class="nav-item pl-2">
          <a class="nav-link" href="https://dapr.io" target="_blank">
            made with &nbsp;<img src="img/mw-dapr.png" height="30">
          </a>
        </li>
      </ul>
      <div class="copyright">
        <a class="nav-link" href="https://github.com/orgs/CloudNativeGBB/people" target="_blank">
        <span class="align-text-top pb-2">CREATED BY THE </span><img src="img/cngbb-wide.png" height="24"> TEAM
        </a>
      </div>
    </div>
  </footer>
</template>
<script>
  export default {
    data() {
      return {
        year: new Date().getFullYear()
      }
    }
  };
</script>
<style>
</style>
