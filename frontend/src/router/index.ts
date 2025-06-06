import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";
import LoginView from "../views/LoginView.vue";
import SupplierManagementView from "@/views/SupplierManagementView.vue";
import WarehouseView from "@/views/WarehouseView.vue";
import ProductIndex from "@/views/ProductIndex.vue";
import ProductAdd from "@/views/ProductAdd.vue"; // Thêm dòng này
import ProductDetail from "@/views/ProductDetail.vue";
import ImportManagementView from "@/views/ImportManagementView.vue";
import ImportReportView from '../views/ImportReportView.vue' // Thêm import cho component báo cáo
import ExportManagermentView from "@/views/ExportManagermentView.vue";
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
    {
      path: "/products", // Thêm route này
      name: "products",
      component: ProductIndex,
    },
    {
      path: "/ProductAdd", // Thêm route này
      name: "products-add",
      component: ProductAdd,
    },
    { path: "/ProductDetail/:id", 
      name: "ProductDetail",
      component: ProductDetail
     },
      
    {
      path: "/import",
      name: "import",
      component: ImportManagementView,
    },
    {
      path: '/import/report',
      name: 'import-report',
      component: ImportReportView,
      meta: {
        requiresAuth: true
      }
    },
    {
      path: "/export",
      name: "export",
      component: ExportManagermentView,
    }
  ],
});

export default router;
