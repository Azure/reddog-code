<template>
<div>
  <header class="container-fluid">
    <div class="text-right">
    <button class="btn btn-link">
      <i class="tim-icons icon-cart"></i>
    </button>
      </div>
  </header>
  <div class="row">
    <order-item v-for="orderItem in orderItems" :key="orderItem.orderItemId" :orderItem="orderItem"></order-item>
  </div>
</div>
</template>

<script>
import OrderItem from "./Kiosk/OrderItem";

export default {
  data() {
    return {
      orderItems: [],
    };
  },
  components: {
    OrderItem,
  },
  async mounted() {
    fetch("https://dapralgibbon.westus.cloudapp.azure.com/menuitem")
      .then((response) => response.json())
      .then((data) => {
        this.orderItems = data;
      });
  },
};
</script>