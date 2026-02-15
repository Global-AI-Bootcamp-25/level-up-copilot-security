<script setup lang="ts">
// Clean home view with just the logos
</script>

<template>
  <main>
    <h1>Welcome to the Vue 3 + .NET Application</h1>
    
    <div class="hero">
      <a href="https://vite.dev" target="_blank">
        <img src="/vite.svg" class="logo" alt="Vite logo" />
      </a>
      <a href="https://vuejs.org/" target="_blank">
        <img src="../assets/vue.svg" class="logo vue" alt="Vue logo" />
      </a>
    </div>
    
    <p class="description">
      A modern (insecure) web application built with Vue 3 and Vite
    </p>
  </main>
</template>

<style scoped>
h1 {
  font-size: 2rem;
  color: #2c3e50;
  margin-bottom: 2rem;
  text-align: center;
}

.hero {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 2rem;
  margin-bottom: 2rem;
}

.description {
  text-align: center;
  color: #666;
  font-size: 1.2rem;
}

.logo {
  height: 6em;
  padding: 1.5em;
  will-change: filter;
  transition: filter 300ms;
}
.logo:hover {
  filter: drop-shadow(0 0 2em #646cffaa);
}
.logo.vue:hover {
  filter: drop-shadow(0 0 2em #42b883aa);
}
</style>
