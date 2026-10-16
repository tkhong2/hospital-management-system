<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    status: string
    variant?: 'default' | 'success' | 'warning' | 'danger' | 'info'
  }>(),
  {
    variant: 'default'
  }
)

const variantClasses = computed(() => {
  switch (props.variant) {
    case 'success':
      return 'bg-emerald-100 text-emerald-800 border-emerald-200'
    case 'warning':
      return 'bg-amber-100 text-amber-800 border-amber-200'
    case 'danger':
      return 'bg-rose-100 text-rose-800 border-rose-200'
    case 'info':
      return 'bg-blue-100 text-blue-800 border-blue-200'
    default:
      return 'bg-slate-100 text-slate-700 border-slate-200'
  }
})
</script>

<template>
  <span
    :class="[
      'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold border',
      variantClasses
    ]"
  >
    <slot>{{ status }}</slot>
  </span>
</template>
