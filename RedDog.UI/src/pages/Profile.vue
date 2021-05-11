<template>
  <div class="row">
    <div class="col-md-8">
      <edit-profile-form :model="model">
      </edit-profile-form>
    </div>
    <div class="col-md-4">
      <user-card :user="user"></user-card>
    </div>
  </div>
</template>
<script>
  import EditProfileForm from './Profile/EditProfileForm';
  import UserCard from './Profile/UserCard'
  export default {
    components: {
      EditProfileForm,
      UserCard
    },
    data() {
      return {
        model: {
          company: 'Black Gold Cooperative',
          email: 'max.lord@bgcoop.com',
          username: 'dreamstone',
          firstName: 'Max',
          lastName: 'Lord',
          address: 'Bld Mihail Kogalniceanu, nr. 8 Bl 1, Sc 1, Ap 09',
          city: 'Washington, DC',
          country: 'USA',
          about: 'Motivational-esque speaker.'
        },
        user: {
          fullName: 'Max Lord',
          title: 'Ceo/Co-Founder',
          description: `Life is good, but it can be better. Make a wish.`,
        }
      }
    }
  }
</script>
<style>
</style>
