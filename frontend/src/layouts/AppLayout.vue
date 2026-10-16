<script setup lang="ts">
import AppHeader from '@/components/AppHeader.vue'
import AppLauncherModal from '@/components/AppLauncherModal.vue'
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex flex-col">
    <AppHeader />
    <main class="flex-1 p-6 max-w-7xl w-full mx-auto">
      <router-view />
    </main>
    <AppLauncherModal />
  </div>
</template>

