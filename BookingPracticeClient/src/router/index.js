import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'bookingView',
      component: () => import('@/views/BookingView.vue'),
    },
  ],
})

export default router
