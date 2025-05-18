<!-- filepath: d:\QLKHang\QuanLyKhoHang\frontend\src\views\ProductIndex.vue -->
<template>
  <div class="product-index">
    <div class="header">
      <h1>Hàng hóa</h1>
      <div class="search-bar">
        <input v-model="search" placeholder="Theo mã, tên hàng" @input="filterProducts" />
      </div>
      <div class="actions">
        <router-link to="/add-product" class="btn green">+ Thêm mới</router-link>
        <button class="btn" @click="importExcel">Import</button>
        <button class="btn" @click="exportExcel">Xuất file</button>
      </div>
    </div>
    <div class="filters">
      <div class="filter-box">
        <div class="filter-title">Loại hàng</div>
        <div><input type="checkbox" /> Hàng hóa</div>
        <div><input type="checkbox" /> Dịch vụ</div>
        <div><input type="checkbox" /> Combo - Đóng gói</div>
      </div>
      <div class="filter-box">
        <div class="filter-title">Nhóm hàng</div>
        <input class="group-search" placeholder="Tìm kiếm nhóm hàng" />
        <div><strong>Tất cả</strong></div>
        <div>Hàng mất nhãn</div>
        <div>Hàng mới</div>
        <div>Kỳ cựu</div>
      </div>
      <div class="filter-box">
        <div class="filter-title">Tồn kho</div>
        <div><input type="radio" checked /> Tất cả</div>
        <div><input type="radio" /> Dưới định mức tồn</div>
      </div>
    </div>
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th></th>
            <th>Mã hàng</th>
            <th>Tên hàng</th>
            <th>Giá bán</th>
            <th>Giá vốn</th>
            <th>Tồn kho</th>
            <th>Khách đặt</th>
            <th>Thời gian tạo</th>
            <th>Dự kiến hết hàng</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="product in filteredProducts" :key="product.id">
            <td><span class="star">&#9734;</span></td>
            <td>{{ product.barcode }}</td>
            <td>{{ product.name }}</td>
            <td>{{ formatCurrency(product.cost) }}</td>
            <td>{{ formatCurrency(product.cost) }}</td>
            <td>{{ product.num }}</td>
            <td>0</td>
            <td>{{ formatDate(product.createdAt) }}</td>
            <td>---</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "ProductIndex",
  data() {
    return {
      products: [],
      filteredProducts: [],
      search: "",
    };
  },
  methods: {
    async fetchProducts() {
      try {
        const res = await axios.get("https://localhost:7189/api/Products/list");
        this.products = res.data;
        this.filteredProducts = res.data;
      } catch (err) {
        console.error("Lỗi tải sản phẩm:", err);
      }
    },
    filterProducts() {
      const s = this.search.trim().toLowerCase();
      if (!s) {
        this.filteredProducts = this.products;
        return;
      }
      this.filteredProducts = this.products.filter(
        p =>
          (p.name && p.name.toLowerCase().includes(s)) ||
          (p.barcode && p.barcode.toLowerCase().includes(s))
      );
    },
    formatCurrency(val) {
      if (!val) return "0";
      return Number(val).toLocaleString("vi-VN");
    },
    formatDate(date) {
      if (!date) return "---";
      const d = new Date(date);
      return d.toLocaleDateString("vi-VN") + " " + d.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" });
    },
    async exportExcel() {
      try {
        const res = await productApi.exportExcel();
        const url = window.URL.createObjectURL(new Blob([res.data]));
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", "products.xlsx");
        document.body.appendChild(link);
        link.click();
        link.remove();
      } catch (err) {
        alert("Xuất file thất bại!");
      }
    },
    importExcel() {
      alert("Chức năng import sẽ được bổ sung sau!");
    },
  },
  mounted() {
    this.fetchProducts();
  },
};
</script>

<style scoped>
.product-index {
  padding: 1rem 2rem;
}
.header {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.header h1 {
  flex: 1;
  margin: 0;
}
.search-bar input {
  padding: 0.5rem 1rem;
  border-radius: 4px;
  border: 1px solid #bbb;
  width: 250px;
}
.actions {
  display: flex;
  gap: 0.5rem;
}
.btn {
  background: #f5f5f5;
  border: 1px solid #0a74e6;
  color: #0a74e6;
  padding: 0.5rem 1.2rem;
  border-radius: 4px;
  cursor: pointer;
  font-weight: 500;
  transition: background 0.2s;
}
.btn.green {
  background: #19c37d;
  color: #fff;
  border: none;
}
.btn:hover {
  background: #e3f0ff;
}
.filters {
  display: flex;
  gap: 1rem;
  margin: 1rem 0;
}
.filter-box {
  background: #fff;
  border-radius: 6px;
  padding: 1rem;
  min-width: 180px;
  box-shadow: 0 1px 4px rgba(10,116,230,0.08);
}
.filter-title {
  font-weight: bold;
  margin-bottom: 0.5rem;
}
.group-search {
  width: 100%;
  margin-bottom: 0.5rem;
  padding: 0.3rem;
  border-radius: 4px;
  border: 1px solid #ddd;
}
.table-container {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.07);
  overflow-x: auto;
}
table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
}
th, td {
  padding: 0.7rem 0.5rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}
th {
  background: #f5faff;
  font-weight: 600;
}
.star {
  color: #bbb;
  font-size: 1.2rem;
}
</style>