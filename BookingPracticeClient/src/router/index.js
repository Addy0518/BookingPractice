import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'loginView',
      meta: { requiresAuth: true },
      component: () => import('@/views/auth/LoginView.vue'),
    },
  ],
})

router.beforeEach((to, from, next) => {
  const auth = localStorage.getItem('auth')
  const token = auth ? JSON.parse(auth)?.token : null

  if (to.meta.requiresAuth && !token) {
    next({ name: 'loginView' })
  } else {
    next()
  }
})

export default router
