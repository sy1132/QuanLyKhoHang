import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";
import LoginView from "../views/LoginView.vue";
import AddProduct from "@/views/Add-Product.vue";
import ProductIndex from "@/views/ProductIndex.vue"; // Thêm dòng này

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
      path: "/add-product",
      name: "add-product",
      component: AddProduct,
    },
    {
      path: "/products", // Thêm route này
      name: "products",
      component: ProductIndex,
    },
  ],
});

export default router;
