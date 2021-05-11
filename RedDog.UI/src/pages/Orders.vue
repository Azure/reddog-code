<template>
  <div class="card">
    <div class="col-lg-8 col-md-12">
      <card class="card" :header-classes="{'text-right': isRTL}">
        <h4 slot="header" class="card-title">Orders</h4>
          <div class="table-full-width table-responsive">
            <orders-table></orders-table>
          </div>
      </card>
    </div>
  </div>
</template>

<script>

import OrdersTable from './Orders/OrdersTable';
export default {
  components: {
      OrdersTable
  }
};

</script>
<style>
</style>
