import type { Directive, DirectiveBinding } from 'vue'
import { useAuthStore } from '@/stores/auth'

export const permissionDirective: Directive = {
  mounted(el: HTMLElement, binding: DirectiveBinding<string | string[]>) {
    const { value } = binding
    const authStore = useAuthStore()

    if (!value) return

    let hasAccess = false

    if (Array.isArray(value)) {
      hasAccess = value.some(perm => authStore.hasPermission(perm))
    } else if (typeof value === 'string') {
      hasAccess = authStore.hasPermission(value)
    }

    if (!hasAccess) {
      el.parentNode?.removeChild(el)
    }
  },
  updated(el: HTMLElement, binding: DirectiveBinding<string | string[]>) {
    const { value } = binding
    const authStore = useAuthStore()

    if (!value) return

    let hasAccess = false

    if (Array.isArray(value)) {
      hasAccess = value.some(perm => authStore.hasPermission(perm))
    } else if (typeof value === 'string') {
      hasAccess = authStore.hasPermission(value)
    }

    if (!hasAccess && el.parentNode) {
      el.parentNode.removeChild(el)
    }
  }
}

export default permissionDirective

