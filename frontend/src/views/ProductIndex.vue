<!-- filepath: d:\QLKHang\QuanLyKhoHang\frontend\src\views\ProductIndex.vue -->
<template>
  <div class="product-index-outer">
    <!-- Header lớn -->
    <div>
      <div class="header-bar">
        <div class="page-title">Hàng hóa</div>
        <div class="table-actions">
          <input v-model="search" class="search-bar" placeholder="Theo mã, tên hàng" @input="filterProducts" />
          <router-link to="/ProductAdd" class="btn blue"><span class="plus-btn">+</span> Thêm mới</router-link>
          <button class="btn green" @click="triggerFileInput"><span class="icon">&#128190;</span> Import</button>
          <button class="btn gray" @click="exportExcel"><span class="icon">&#128190;</span> Xuất file</button>
          <div class="column-toggle">
            <button class="btn cyan" @click="showColumnMenu = !showColumnMenu">
              <span style="font-size: 18px">&#9776;</span>
            </button>
            <div v-if="showColumnMenu" class="column-menu">
              <div class="column-menu-grid">
                <label v-for="col in columns" :key="col.key">
                  <input type="checkbox" v-model="col.visible" />
                  <span>{{ col.label }}</span>
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div class="content-layout">
      <!-- Sidebar -->
      <div class="sidebar">
        <div class="filter-box">
          <div class="filter-title">
            Loại hàng <span class="arrow">&#9660;</span>
          </div>
          <div class="filter-option">
            <input type="checkbox" id="product-type-goods" v-model="filters.productTypes.goods"
              @change="applyFilters" />
            <label for="product-type-goods">Hàng hóa</label>
          </div>
          <div class="filter-option">
            <input type="checkbox" id="product-type-service" v-model="filters.productTypes.service"
              @change="applyFilters" />
            <label for="product-type-service">Dịch vụ</label>
          </div>
          <div class="filter-option">
            <input type="checkbox" id="product-type-combo" v-model="filters.productTypes.other"
              @change="applyFilters" />
            <label for="product-type-combo">Combo - Đóng gói</label>
          </div>
        </div>
      </div>
      <!-- Main content -->
      <div class="main-content">
        <div class="table-container">
          <table>
            <thead>
              <tr>
                <th>
                  <input type="checkbox" v-model="allSelected" @change="toggleAll" class="styled-checkbox" />
                </th>
                <th v-if="isColVisible('image')">Hình ảnh</th>
                <th v-if="isColVisible('barcode')">Mã vạch</th>
                <th v-if="isColVisible('name')">Tên hàng</th>
                <th v-if="isColVisible('categoryID')">Nhóm hàng</th>
                <th v-if="isColVisible('brand')">Thương hiệu</th>
                <th v-if="isColVisible('cost')">Giá vốn</th>
                <th v-if="isColVisible('price')">Giá bán</th>
                <th v-if="isColVisible('location')">Vị trí</th>
                <th v-if="isColVisible('warehouseId')">Kho</th>
                <th v-if="isColVisible('status')">Trạng thái</th>
                <th v-if="isColVisible('createdDate')">Ngày tạo</th>
                <th v-if="isColVisible('finaldDate')">Ngày hết hạn</th>
                <th v-if="isColVisible('description')">Mô tả</th>
                <th v-if="isColVisible('num')">Số Lượng</th>

              </tr>
            </thead>
            <tbody>
              <tr v-for="product in filteredProducts" :key="product.id"a
                :class="{ selected: selectedRows.includes(product.id) }" @click="goToDetail(product.id)"
                style="cursor:pointer">
                <td>
                  <input type="checkbox" class="styled-checkbox" :checked="selectedRows.includes(product.id)"
                    @change="selectRow(product.id, $event)" @click.stop />
                </td>
                <td v-if="isColVisible('image')">
                  <img :src="product.image && typeof product.image === 'string' ? product.image : defaultImg" alt="img"
                    style="width: 40px; height: 40px; object-fit: cover; border-radius: 4px;" />
                </td>
                <td v-if="isColVisible('barcode')">{{ product.barcode }}</td>
                <td v-if="isColVisible('name')">{{ product.name }}</td>
                <td v-if="isColVisible('categoryID')">{{ getCategoryName(product.categoryID) }}</td>
                <td v-if="isColVisible('brand')">{{ product.brand }}</td>
                <td v-if="isColVisible('cost')">{{ formatCurrency(product.cost) }}</td>
                <td v-if="isColVisible('price')">{{ formatCurrency(product.price) }}</td>
                <td v-if="isColVisible('location')">{{ product.location }}</td>
                <td v-if="isColVisible('warehouseId')">{{ getWarehouseName(product.warehouseId) }}</td>
                <td v-if="isColVisible('status')">
                  <span v-if="product.status == 1 || product.status == '1'" style="color:#19c37d;">Đang bán</span>
                  <span v-else style="color:#e74c3c;">Ngừng bán</span>
                </td>
                <td v-if="isColVisible('createdDate')">{{ formatDate(product.createdDate) }}</td>
                <td v-if="isColVisible('finaldDate')">{{ formatDate(product.finaldDate) }}</td>
                <td v-if="isColVisible('description')">{{ product.description }}</td>
                <td v-if="isColVisible('num')">{{ product.num }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <input ref="fileInput" type="file" accept=".xlsx,.xls" style="display: none" @change="importExcel" />
  </div>
</template>

<script>
import axios from "axios";
import ProductAdd from './ProductAdd.vue';
const defaultImg = "https://via.placeholder.com/40x40?text=IMG";
export default {
  name: "ProductIndex",
  components: { ProductAdd },
  data() {
    return {
      products: [],
      filteredProducts: [],
      search: "",
      showColumnMenu: false,
      defaultImg,
      columns: [
        { key: "select", label: "", visible: true }, // Checkbox chọn dòng
        { key: "image", label: "Hình ảnh", visible: true },
        { key: "barcode", label: "Mã vạch", visible: true },
        { key: "name", label: "Tên hàng", visible: true },
        { key: "categoryID", label: "Nhóm hàng", visible: true },
        { key: "brand", label: "Thương hiệu", visible: true },
        { key: "cost", label: "Giá vốn", visible: true },
        { key: "price", label: "Giá bán", visible: true },
        { key: "location", label: "Vị trí", visible: true },
        { key: "warehouseId", label: "Kho", visible: true },
        { key: "status", label: "Trạng thái", visible: true },
        { key: "createdDate", label: "Ngày tạo", visible: false },
        { key: "finaldDate", label: "Ngày hết hạn", visible: false },
        { key: "description", label: "Mô tả", visible: false },
        { key: "num", label: "Số Lượng", visible: true }
      ],
      filters: {
        productTypes: {
          goods: true, // Mặc định hiển thị hàng hóa (CategoryID = 1)
          service: true, // Mặc định hiển thị dịch vụ (CategoryID = 2)
          other: true, // Mặc định hiển thị loại khác (CategoryID = 3)
        },
      },
      selectedProduct: null,
      showDetail: false,
      allSelected: false,
      selectedRows: [],
      warehouses: [],
      categories: [],
    };
  },
  methods: {
    goToDetail(id) {
      this.$router.push({ name: "ProductDetail", params: { id } });
    },
    async fetchProducts() {
      try {
        const res = await axios.get("https://localhost:7189/api/Products/list");

        // Log toàn bộ response data để kiểm tra
        console.log("API Response:", res.data);

        // Sửa đoạn này: kiểm tra nếu trả về là mảng thì dùng luôn
        if (Array.isArray(res.data)) {
          this.products = res.data.map((p) => ({
            ...p,
            price: p.price || p.cost * 1.2,
          }));
        }
        // Nếu trả về dạng { result: { data: [...] } }
        else if (
          res.data &&
          res.data.result &&
          Array.isArray(res.data.result.data)
        ) {
          this.products = res.data.result.data.map((p) => ({
            ...p,
            price: p.price || p.cost * 1.2,
          }));
        } else {
          this.products = [];
          console.error("Định dạng dữ liệu không đúng:", res.data);
        }

        this.applyFilters();
      } catch (err) {
        console.error("Lỗi tải sản phẩm:", err);
        this.products = [];
        this.filteredProducts = [];
      }
    },
    async fetchWarehouses() {
      try {
        const res = await axios.get("https://localhost:7189/api/warehouse");
        this.warehouses = res.data?.result?.data || [];
      } catch {
        this.warehouses = [];
      }
    },
    async fetchCategories() {
      try {
        const res = await axios.get("http://localhost:7189/api/categories");
        this.categories = res.data;
      } catch {
        this.categories = [
          { id: 1, name: "Hàng hóa" },
          { id: 2, name: "Dịch vụ" },
          { id: 3, name: "Combo" }
        ];
      }
    },
    filterProducts() {
      const s = this.search.trim().toLowerCase();
      if (!s) {
        this.filteredProducts = this.products;
        return;
      }
      this.filteredProducts = this.products.filter(
        (p) =>
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
      return (
        d.toLocaleDateString("vi-VN") +
        " " +
        d.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" })
      );
    },
    isColVisible(key) {
      return this.columns.find((col) => col.key === key)?.visible;
    },
    async exportExcel() {
      try {
        const response = await axios.get(
          "https://localhost:7189/api/Products/export-excel",
          {
            responseType: "blob",
          }
        );
        // Lấy tên file từ header nếu có, nếu không thì đặt mặc định
        let fileName = "products.xlsx";
        const disposition = response.headers["content-disposition"];
        if (disposition && disposition.includes("filename=")) {
          fileName = decodeURIComponent(
            disposition.split("filename=")[1].replace(/"/g, "")
          );
        }
        // Tạo link và tải file
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement("a");
        link.href = url;
        link.setAttribute("download", fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
      } catch (err) {
        alert("Xuất file thất bại!");
        console.error(err);
      }
    },
    importExcel() {
      alert("Chức năng import sẽ được bổ sung sau!");
    },
    triggerFileInput() {
      this.$refs.fileInput.click();
    },
    async importExcel(event) {
      const file = event.target.files[0];
      if (!file) return;
      const formData = new FormData();
      formData.append("file", file);

      try {
        const res = await axios.post(
          "https://localhost:7189/api/Products/import-excel",
          formData,
          { headers: { "Content-Type": "multipart/form-data" } }
        );
        alert(res.data?.Message || "Import thành công!");
        this.fetchProducts(); // Refresh lại danh sách
      } catch (err) {
        alert("Import thất bại!");
        console.error(err);
      } finally {
        event.target.value = ""; // Reset input để chọn lại file nếu cần
      }
    },
    // Thêm phương thức mới để lọc sản phẩm theo bộ lọc đã chọn
    applyFilters() {
      const s = this.search.trim().toLowerCase();

      // Lọc theo search text trước (nếu có)
      let filtered = this.products;
      if (s) {
        filtered = filtered.filter(
          (p) =>
            (p.name && p.name.toLowerCase().includes(s)) ||
            (p.barcode && p.barcode.toLowerCase().includes(s))
        );
      }

      // Áp dụng bộ lọc loại hàng
      filtered = this.filterByProductType(filtered);

      this.filteredProducts = filtered;
    },

    filterByProductType(products) {
      // Nếu không có bộ lọc nào được chọn hoặc tất cả đều được chọn, trả về tất cả sản phẩm
      if (
        (!this.filters.productTypes.goods &&
          !this.filters.productTypes.service &&
          !this.filters.productTypes.other) ||
        (this.filters.productTypes.goods &&
          this.filters.productTypes.service &&
          this.filters.productTypes.other)
      ) {
        return products;
      }

      // Lọc theo loại CategoryID
      return products.filter((product) => {
        const categoryId = parseInt(product.categoryID) || 0;

        if (this.filters.productTypes.goods && categoryId === 1) {
          return true;
        }
        if (this.filters.productTypes.service && categoryId === 2) {
          return true;
        }
        if (this.filters.productTypes.other && categoryId === 3) {
          return true;
        }
        return false;
      });
    },
    toggleAll(event) {
      const checked = event.target.checked;
      this.selectedRows = checked ? this.filteredProducts.map(p => p.id) : [];
    },
    selectRow(id, event) {
      const checked = event.target.checked;
      if (checked) {
        this.selectedRows.push(id);
      } else {
        this.selectedRows = this.selectedRows.filter(i => i !== id);
      }
      // Cập nhật trạng thái "Chọn tất cả" nếu cần
      this.allSelected = this.filteredProducts.length > 0 && this.selectedRows.length === this.filteredProducts.length;
    },
    getWarehouseName(id) {
      const wh = this.warehouses.find(w => w.id == id);
      return wh ? wh.name : id;
    },
    getCategoryName(id) {
      const cat = this.categories.find(c => c.id == id);
      return cat ? cat.name : id;
    },
  },
  mounted() {
    this.fetchProducts();
    this.fetchWarehouses();
    this.fetchCategories();
  },
};
</script>

<style scoped>
body {
  background: #f4f6fa;
  font-family: "Segoe UI", Arial, sans-serif;
}

.product-index-outer {
  max-width: 100vw;
  min-height: 100vh;
  background: #f4f6fa;
}

.header-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 32px 40px 0 40px;
  background: #f4f6fa;
  gap: 24px;
}

.page-title {
  font-size: 2.5rem;
  font-weight: 900;
  color: #1976d2;
  margin: 0;
  letter-spacing: -1px;
  flex-shrink: 0;
}

.table-actions {
  display: flex;
  align-items: center;
  gap: 18px;
  background: none;
  box-shadow: none;
  border-radius: 0;
  padding: 0;
  margin: 0;
}

.search-bar {
  padding: 0.7rem 1.3rem;
  border-radius: 8px;
  border: 1.5px solid #b0bec5;
  width: 320px;
  font-size: 18px;
  background: #fff;
  color: #222;
  transition: border 0.2s;
  margin-right: 10px;
}

.search-bar:focus {
  border: 1.5px solid #1976d2;
  outline: none;
}

.btn {
  background: linear-gradient(90deg, #00c853 0%, #43e97b 100%);
  border: none;
  color: #fff;
  padding: 0.6rem 1.7rem;
  border-radius: 12px;
  cursor: pointer;
  font-weight: 700;
  font-size: 18px;
  transition: background 0.18s, box-shadow 0.18s, transform 0.12s;
  box-shadow: 0 2px 12px rgba(67, 233, 123, 0.13);
  outline: none;
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 120px;
  justify-content: center;
  letter-spacing: 0.5px;
}

.btn.cyan {
  background: #00cfe8;
  color: #fff;
}

.btn.cyan:hover {
  background: #00b6cb;
}

.btn.blue {
  background: #1976d2;
  color: #fff;
}

.btn.blue:hover {
  background: #1565c0;
}

.btn.green {
  background: #219653;
  color: #fff;
}

.btn.green:hover {
  background: #1e874a;
}

.btn.gray {
  background: #757575;
  color: #fff;
}

.btn.gray:hover {
  background: #616161;
}

.plus-btn {
  font-size: 22px;
  font-weight: bold;
  margin-right: 4px;
  margin-left: -4px;
  color: #fff;
}

.icon {
  font-size: 18px;
  margin-right: 4px;
  color: #fff;
}

.column-toggle {
  position: relative;
  display: inline-block;
}

.column-menu {
  position: absolute;
  right: 0;
  top: 45px;
  background: #fff;
  border: 1.5px solid #1976d2;
  border-radius: 14px;
  box-shadow: 0 8px 32px rgba(25, 118, 210, 0.13);
  padding: 1.3rem 1.7rem;
  z-index: 10;
  min-width: 340px;
  max-width: 420px;
  max-height: 500px;
  overflow-y: auto;
}

.column-menu-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.8rem 1.3rem;
}

.column-menu label {
  display: flex;
  align-items: center;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  user-select: none;
  color: #222;
}

.column-menu input[type="checkbox"] {
  accent-color: #1976d2;
  width: 18px;
  height: 18px;
  margin-right: 0.5rem;
  border-radius: 4px;
  border: 1.5px solid #1976d2;
  box-shadow: 0 1px 4px rgba(25, 118, 210, 0.08);
}

.content-layout {
  display: flex;
  gap: 28px;
  padding: 0 40px;
  max-width: 100vw;
}

.sidebar {
  width: 300px;
  min-width: 220px;
  background: transparent;
  padding-left: 0;
  display: flex;
  flex-direction: column;
  gap: 28px;
}

.filter-box {
  background: #fff;
  border-radius: 14px;
  box-shadow: 0 2px 12px rgba(25, 118, 210, 0.07);
  padding: 22px 24px 20px 24px;
}

.filter-title {
  font-weight: 700;
  font-size: 19px;
  margin-bottom: 13px;
  color: #1976d2;
  letter-spacing: 0.5px;
}

.filter-option {
  margin-bottom: 12px;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 10px;
  color: #222;
}

.filter-option input[type="checkbox"],
.filter-option input[type="radio"] {
  accent-color: #1976d2;
  width: 18px;
  height: 18px;
}

.group-search {
  width: 100%;
  margin-bottom: 13px;
  padding: 8px 13px;
  border-radius: 8px;
  border: 1.5px solid #b0bec5;
  font-size: 16px;
  background: #f8fafc;
  color: #222;
  transition: border 0.2s;
}

.group-search:focus {
  border: 1.5px solid #1976d2;
  outline: none;
}

.group-list {
  margin-top: 7px;
}

.group-item {
  padding: 9px 13px;
  border-radius: 8px;
  font-size: 16px;
  cursor: pointer;
  margin-bottom: 4px;
  transition: background 0.15s, color 0.15s;
  color: #222;
  font-weight: 500;
}

.group-item.active,
.group-item:hover {
  background: #e3f2fd;
  color: #1976d2;
  font-weight: 700;
}

.arrow,
.plus {
  font-size: 20px;
  color: #bbb;
  margin-left: 8px;
  cursor: pointer;
}

.main-content {
  flex: 1;
  max-width: 100vw;
  overflow: hidden;
  padding: 0 0 0 0;
  display: flex;
  flex-direction: column;
}

.table-container {
  width: 100%;
  overflow-x: auto;
  margin-bottom: 24px;
  box-shadow: 0 4px 24px rgba(25, 118, 210, 0.08);
  border-radius: 12px;
  background: #fff;
  padding: 0px 0 0px 0;
}

table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  min-width: 900px;
  color: #333;
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
}

th {
  color: #1976d2;
  font-weight: 700;
  font-size: 17px;
  background: #e3f2fd;
  padding: 13px 8px;
  border-bottom: 2px solid #bbdefb;
  text-align: left;
}

td {
  color: #333;
  font-size: 16px;
  padding: 12px 8px;
  vertical-align: middle;
  background: #fff;
}

tr {
  background: #fff;
  transition: background 0.2s;
}

tr:nth-child(even) {
  background: #f8fafc;
}

tr:hover {
  background: #e3f2fd;
}

td img {
  display: block;
  margin: 0 auto;
  border-radius: 50%;
  border: 2px solid #bbdefb;
  background: #fafbfc;
  width: 44px;
  height: 44px;
  object-fit: cover;
  box-shadow: 0 2px 8px rgba(25, 118, 210, 0.07);
}

td[v-if*="name"],
td[v-if*="description"] {
  white-space: normal;
  max-width: 220px;
}

td[v-if*="barcode"],
td[v-if*="brand"],
td[v-if*="status"] {
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
}

td[v-if*="cost"],
td[v-if*="price"],
td[v-if*="num"] {
  max-width: 100px;
  text-align: right;
  color: #1976d2;
  font-weight: 600;
}

.styled-checkbox {
  accent-color: #1976d2;
  width: 18px;
  height: 18px;
  border-radius: 4px;
  border: 1.5px solid #1976d2;
  box-shadow: 0 1px 4px rgba(25, 118, 210, 0.08);
}

.selected {
  background: #e3f2fd !important;
}

@media (max-width: 1200px) {
  .sidebar {
    width: 120px;
    min-width: 100px;
    padding-left: 0;
  }

  .content-layout {
    padding: 0 10px;
  }

  .main-content {
    padding-right: 0;
    padding-left: 0;
  }

  .table-container {
    margin-left: 0;
    padding: 8px 0 6px 0;
  }

  .header-bar {
    padding: 18px 10px 0 10px;
  }
}
</style>