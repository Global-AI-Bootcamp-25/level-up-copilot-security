<script setup lang="ts">
</script>

<template>
  <div class="about">
    <h1>About This Application</h1>
    <p>
      This is a modern Vue 3 application built with Vite, using TypeScript for type safety. 
      It demonstrates integration with a .NET 7 backend and uses Vue Router for navigation 
      and Pinia for state management.
    </p>
    <p>
      The application showcases the latest features of Vue 3 including the Composition API 
      and demonstrates how to structure a modern Vue application with best practices.
    </p>
    <p>
      For API calls, we're using the native Fetch API which provides an efficient way to 
      make HTTP requests without additional dependencies.
    </p>
  </div>
</template>

<style scoped>
.about {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
  text-align: left;
}

h1 {
  margin-bottom: 1.5rem;
  color: #42b883;
}

p {
  margin-bottom: 1rem;
  line-height: 1.6;
}
</style>
