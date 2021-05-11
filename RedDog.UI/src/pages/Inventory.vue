<template>
  <div class="card">
    <div class="col-lg-8 col-md-12">
      <card class="card" :header-classes="{'text-right': isRTL}">
        <h4 slot="header" class="card-title">Inventory</h4>
          <div class="table-responsive">
            <inventory-table></inventory-table>
          </div>
      </card>
    </div>
  </div>
</template>

<script>

import InventoryTable from './Inventory/InventoryTable';
export default {
  components: {
      InventoryTable
  }
};

</script>
<style>
</style>
