import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";
import LoginView from "../views/LoginView.vue";
import SupplierManagementView from "@/views/SupplierManagementView.vue";
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: HomeView,
    },
    {
      path: "/login",
      name: "login",
      component: LoginView,
    },
    {
      path:"/sup",
      name:"sup",
      component : SupplierManagementView
    },
    {
      path:"/SupplierManagementView",
      name:"SupplierManagementView",
      component : SupplierManagementView
    }
  ],
});

export default router;
