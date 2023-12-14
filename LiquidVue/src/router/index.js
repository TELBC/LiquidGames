import { createRouter, createWebHistory } from 'vue-router'
import App from '../App.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: App
    },
    {
      path: '/:catchAll(.*)', 
      redirect: '/' 
    }
  ]
})

export default router
