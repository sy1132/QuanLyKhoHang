<template>
  <div class="spinner-container">
    <div class="spinner"></div>
  </div>
</template>

<style scoped>
.spinner-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(0, 221, 235, 0.3);
  border-radius: 50%;
  border-top: 3px solid #00ddeb;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>
