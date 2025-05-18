import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";
import LoginView from "../views/LoginView.vue";
import SupplierManagementView from "@/views/SupplierManagementView.vue";
import WarehouseView from "@/views/WarehouseView.vue";
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
      path: "/Supplier",
      name: "Supplier",
      component: SupplierManagementView,
    },
    {
      path: "/Warehouse",
      name: "Warehouse",
      component: WarehouseView,
    },
  ],
});

export default router;
