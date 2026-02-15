<script setup lang="ts">
// Simple header component
</script>

<template>
  <div class="navbar">
    <div class="logo">Level Up With GitHub Copilot</div>
    <slot></slot>
  </div>
</template>

<style scoped>
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  margin-bottom: 2rem;
  border-bottom: 1px solid #eaeaea;
}

.logo {
  font-size: 1.5rem;
  font-weight: bold;
  color: #42b883;
}
</style>
