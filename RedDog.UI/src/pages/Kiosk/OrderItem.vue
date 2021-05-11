<template>
<div :span="6" style="margin:10px">
    <card style="width: 20rem" >
      <img
        slot="image"
        class="card-img-top"
        :src="orderItem.imageUrl"
        alt="Card image cap"
      />
      <h4 class="card-title">{{ orderItem.name }}</h4>
      <p class="card-text">{{ "$" + orderItem.price }}</p>
      <a href="#" class="btn btn-primary">{{this.buttonText}}</a>
    </card>
</div>
</template>

<script>
export default {
  props: ["orderItem"],
  data() {
    return {
      buttonText: "Add To Cart",
    };
  },
  methods: {
    addToCart(orderItem) {},
    //
  },
};
</script>