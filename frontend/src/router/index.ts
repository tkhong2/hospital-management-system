import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AppLayout from '@/layouts/AppLayout.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/',
      component: AppLayout,
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'dashboard',
          component: () => import('@/views/DashboardView.vue')
        },
        {
          path: 'healthexam',
          name: 'healthexam',
          component: () => import('@/modules/healthexam/views/HealthExamHome.vue'),
          meta: { appCode: 'healthexam' }
        },
        {
          path: 'outpatient',
          name: 'outpatient',
          component: () => import('@/modules/outpatient/views/OutpatientHome.vue'),
          meta: { appCode: 'outpatient' }
        },
        {
          path: 'inpatient',
          name: 'inpatient',
          component: () => import('@/modules/inpatient/views/InpatientHome.vue'),
          meta: { appCode: 'inpatient' }
        },
        {
          path: 'pharmacy',
          name: 'pharmacy',
          component: () => import('@/modules/pharmacy/views/PharmacyHome.vue'),
          meta: { appCode: 'pharmacy' }
        },
        {
          path: 'patient',
          name: 'patient',
          component: () => import('@/modules/patient/views/PatientHome.vue'),
          meta: { appCode: 'patient' }
        },
        {
          path: 'catalog',
          name: 'catalog',
          component: () => import('@/modules/catalog/views/CatalogHome.vue'),
          meta: { appCode: 'catalog' }
        },
        {
          path: 'admin',
          name: 'admin',
          component: () => import('@/modules/admin/views/AdminHome.vue'),
          meta: { appCode: 'admin', requiredPermission: 'users.view' }
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue')
    }
  ]
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth !== false)

  if (requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } })
    return
  }

  if (to.name === 'login' && authStore.isAuthenticated) {
    next({ name: 'dashboard' })
    return
  }

  // App-level RBAC Check
  const targetAppCode = to.meta.appCode as string | undefined
  if (targetAppCode && !authStore.canAccessApp(targetAppCode)) {
    next({ name: 'dashboard' })
    return
  }

  // Permission-level RBAC Check
  const requiredPermission = to.meta.requiredPermission as string | undefined
  if (requiredPermission && !authStore.hasPermission(requiredPermission)) {
    next({ name: 'dashboard' })
    return
  }

  next()
})

export default router
