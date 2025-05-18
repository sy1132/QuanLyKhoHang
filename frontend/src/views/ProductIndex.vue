<!-- filepath: d:\QLKHang\QuanLyKhoHang\frontend\src\views\ProductIndex.vue -->
<template>
  <div class="product-index-outer">
    <!-- Header lớn -->
    <div>
      <div class="header-bar">
        <div class="page-title">Hàng hóa</div>
        <div class="table-actions">
          <input
            v-model="search"
            class="search-bar"
            placeholder="Theo mã, tên hàng"
            @input="filterProducts"
          />
          <router-link to="/ProductAdd" class="btn green"
            ><span class="plus-btn">+</span> Thêm mới</router-link
          >
          <button class="btn green" @click="triggerFileInput">
            <span class="icon">&#128190;</span> Import
          </button>
          <button class="btn green" @click="exportExcel">
            <span class="icon">&#128190;</span> Xuất file
          </button>
          <div class="column-toggle">
            <button class="btn green" @click="showColumnMenu = !showColumnMenu">
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
            <input
              type="checkbox"
              id="product-type-goods"
              v-model="filters.productTypes.goods"
              @change="applyFilters"
            />
            <label for="product-type-goods">Hàng hóa</label>
          </div>
          <div class="filter-option">
            <input
              type="checkbox"
              id="product-type-service"
              v-model="filters.productTypes.service"
              @change="applyFilters"
            />
            <label for="product-type-service">Dịch vụ</label>
          </div>
          <div class="filter-option">
            <input
              type="checkbox"
              id="product-type-combo"
              v-model="filters.productTypes.other"
              @change="applyFilters"
            />
            <label for="product-type-combo">Combo - Đóng gói</label>
          </div>
        </div>
        <div class="filter-box">
          <div class="filter-title">Nhóm hàng <span class="plus">+</span></div>
          <input class="group-search" placeholder="Tìm kiếm nhóm hàng" />
          <div class="group-list">
            <div class="group-item active"><strong>Tất cả</strong></div>
            <div class="group-item">Hàng mất nhãn</div>
            <div class="group-item">Hàng mới</div>
            <div class="group-item">Kỳ cựu</div>
          </div>
        </div>
        <div class="filter-box">
          <div class="filter-title">
            Tồn kho <span class="arrow">&#9660;</span>
          </div>
          <div class="filter-option">
            <input type="radio" name="tonkho" checked /> Tất cả
          </div>
          <div class="filter-option">
            <input type="radio" name="tonkho" /> Dưới định mức tồn
          </div>
        </div>
      </div>
      <!-- Main content -->
      <div class="main-content">
        <div class="table-container">
          <table>
            <thead>
              <tr>
                <th><input type="checkbox" /></th>
                <th v-if="isColVisible('image')">Hình ảnh</th>
                <th v-if="isColVisible('barcode')">Mã hàng</th>
                <th v-if="isColVisible('name')">Tên hàng</th>
                <th v-if="isColVisible('category')">Loại hàng</th>
                <th v-if="isColVisible('brand')">Thương hiệu</th>
                <th v-if="isColVisible('cost')">Giá vốn</th>
                <th v-if="isColVisible('price')">Giá bán</th>
                <th v-if="isColVisible('num')">Tồn kho</th>
                <th v-if="isColVisible('location')">Vị trí</th>
                <th v-if="isColVisible('status')">Trạng thái</th>
                <th v-if="isColVisible('createdDate')">Thời gian tạo</th>
                <th v-if="isColVisible('modifiedDate')">Thời gian sửa</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="product in filteredProducts"
                :key="product.id"
                @click="$router.push(`/ProductDetail/${product.id}`)"
              >
                <td><input type="checkbox" /></td>
                <td v-if="isColVisible('image')">
                  <img
                    :src="product.image || defaultImg"
                    alt="img"
                    style="
                      width: 40px;
                      height: 40px;
                      object-fit: cover;
                      border-radius: 4px;
                    "
                  />
                </td>
                <td v-if="isColVisible('barcode')">{{ product.barcode }}</td>
                <td v-if="isColVisible('name')">{{ product.name }}</td>
                <td v-if="isColVisible('category')">
                  {{ product.categoryID }}
                </td>
                <td v-if="isColVisible('brand')">{{ product.brand }}</td>
                <td v-if="isColVisible('cost')">
                  {{ formatCurrency(product.cost) }}
                </td>
                <td v-if="isColVisible('price')">
                  {{ formatCurrency(product.price) }}
                </td>
                <td v-if="isColVisible('num')">{{ product.num }}</td>
                <td v-if="isColVisible('location')">{{ product.location }}</td>
                <td v-if="isColVisible('status')">{{ product.status }}</td>
                <td v-if="isColVisible('createdDate')">
                  {{ formatDate(product.createdDate) }}
                </td>
                <td v-if="isColVisible('modifiedDate')">
                  {{ formatDate(product.modifiedDate) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
    <input
      ref="fileInput"
      type="file"
      accept=".xlsx,.xls"
      style="display: none"
      @change="importExcel"
    />
  </div>
</template>

<script>
import axios from "axios";
const defaultImg = "https://via.placeholder.com/40x40?text=IMG";
export default {
  name: "ProductIndex",
  data() {
    return {
      products: [],
      filteredProducts: [],
      search: "",
      showColumnMenu: false,
      defaultImg,
      columns: [
        { key: "image", label: "Hình ảnh", visible: true },
        { key: "barcode", label: "Mã hàng", visible: true },
        { key: "name", label: "Tên hàng", visible: true },
        { key: "category", label: "Loại hàng", visible: true },
        { key: "brand", label: "Thương hiệu", visible: true },
        { key: "cost", label: "Giá vốn", visible: true },
        { key: "price", label: "Giá bán", visible: true },
        { key: "num", label: "Tồn kho", visible: true },
        { key: "location", label: "Vị trí", visible: false },
        { key: "status", label: "Trạng thái", visible: true },
        { key: "createdDate", label: "Thời gian tạo", visible: true },
        { key: "modifiedDate", label: "Thời gian sửa", visible: false },
      ],
      filters: {
        productTypes: {
          goods: true, // Mặc định hiển thị hàng hóa (CategoryID = 1)
          service: true, // Mặc định hiển thị dịch vụ (CategoryID = 2)
          other: true, // Mặc định hiển thị loại khác (CategoryID = 3)
        },
      },
    };
  },
  methods: {
    async fetchProducts() {
      try {
        const res = await axios.get("https://localhost:7189/api/Products/list");

        // Log toàn bộ response data để kiểm tra
        console.log("API Response:", res.data);

        // Xử lý dữ liệu có cấu trúc { result: { data: [...], message, isSuccess } }
        if (
          res.data &&
          res.data.result &&
          Array.isArray(res.data.result.data)
        ) {
          this.products = res.data.result.data.map((p) => ({
            ...p,
            price: p.price || p.cost * 1.2,
          }));
          console.log("Sản phẩm đã được tải:", this.products.length);
        } else {
          // Trường hợp dữ liệu không như mong đợi
          this.products = [];
          console.error("Định dạng dữ liệu không đúng:", res.data);
        }

        // Áp dụng bộ lọc sau khi lấy dữ liệu
        this.applyFilters();
      } catch (err) {
        console.error("Lỗi tải sản phẩm:", err);
        this.products = [];
        this.filteredProducts = [];
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
  },
  mounted() {
    this.fetchProducts();
  },
};
</script>

<style scoped>
body {
  background: #f4f6fa;
  font-family: "Segoe UI", Arial, sans-serif;
}

.product-index-outer {
  max-width: 100%;
  overflow-x: hidden;
}

.header-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24px 32px 0 32px;
  background: #f4f6fa;
}

.page-title {
  font-size: 2.3rem;
  font-weight: 800;
  color: #222;
  margin: 0;
  letter-spacing: -1px;
  flex-shrink: 0;
}

.table-actions {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  background: none;
  box-shadow: none;
  border-radius: 0;
  padding: 0;
  margin: 0;
}

.content-layout {
  display: flex;
  gap: 24px;
  padding: 0 24px;
  max-width: 100%; /* Đảm bảo không vượt quá chiều rộng của container */
}

.sidebar {
  width: 320px;
  min-width: 320px;
  background: transparent;
  padding-left: 32px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.filter-box {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(10, 116, 230, 0.07);
  padding: 20px 22px 18px 22px;
}

.filter-title {
  font-weight: 700;
  font-size: 18px;
  margin-bottom: 12px;
  color: #222;
}

.filter-option {
  margin-bottom: 10px;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 10px;
  color: #222;
}

.filter-option input[type="checkbox"],
.filter-option input[type="radio"] {
  accent-color: #19c37d;
  width: 18px;
  height: 18px;
}

.group-search {
  width: 100%;
  margin-bottom: 12px;
  padding: 7px 12px;
  border-radius: 7px;
  border: 1px solid #ddd;
  font-size: 16px;
  background: #f8fafc;
  color: #222;
}

.group-list {
  margin-top: 6px;
}

.group-item {
  padding: 8px 12px;
  border-radius: 7px;
  font-size: 16px;
  cursor: pointer;
  margin-bottom: 3px;
  transition: background 0.15s, color 0.15s;
  color: #222;
  font-weight: 500;
}

.group-item.active,
.group-item:hover {
  background: #e6f9f1;
  color: #19c37d;
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
  max-width: 100%; /* Đảm bảo không vượt quá chiều rộng của container */
  overflow: hidden; /* Ẩn nội dung tràn ra ngoài */
  padding: 0 32px 0 32px;
  display: flex;
  flex-direction: column;
}

.table-actions {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  background: none;
  box-shadow: none;
  border-radius: 0;
  padding: 0;
  margin: 0;
}

.search-bar {
  padding: 0.6rem 1.2rem;
  border-radius: 7px;
  border: 1px solid #bbb;
  width: 340px;
  font-size: 17px;
  background: #fff;
  color: #222;
}

.btn {
  background: #19c37d;
  border: none;
  color: #fff;
  padding: 0.55rem 1.3rem;
  border-radius: 9px;
  cursor: pointer;
  font-weight: 700;
  font-size: 17px;
  transition: background 0.2s, color 0.2s;
  box-shadow: 0 2px 8px rgba(25, 195, 125, 0.08);
  outline: none;
  display: flex;
  align-items: center;
  gap: 7px;
}

.btn.green {
  background: #00b63e;
  color: #fff;
}

.btn:hover,
.btn.green:hover {
  background: #13a76a;
  color: #fff;
}

.plus-btn {
  font-size: 22px;
  font-weight: bold;
  margin-right: 4px;
}

.icon {
  font-size: 18px;
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
  border: 1.5px solid #19c37d;
  border-radius: 12px;
  box-shadow: 0 6px 24px rgba(25, 195, 125, 0.12);
  padding: 1.2rem 1.5rem;
  z-index: 10;
  min-width: 340px;
  max-width: 420px;
  max-height: 500px;
  overflow-y: auto;
}

.column-menu-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.7rem 1.2rem;
}

.column-menu label {
  display: flex;
  align-items: center;
  font-size: 15px;
  font-weight: 500;
  cursor: pointer;
  user-select: none;
  color: #222;
}

.column-menu input[type="checkbox"] {
  accent-color: #19c37d;
  width: 18px;
  height: 18px;
  margin-right: 0.5rem;
  border-radius: 4px;
  border: 1.5px solid #19c37d;
  box-shadow: 0 1px 4px rgba(25, 195, 125, 0.08);
}

.column-menu input[type="checkbox"]:hover {
  box-shadow: 0 0 0 2px #e6f9f1;
}

.table-container {
  width: 100%;
  overflow-x: auto; /* Thêm thanh cuộn ngang khi bảng quá rộng */
  margin-bottom: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  border-radius: 8px;
}

table {
  width: 100%;
  border-collapse: collapse;
  min-width: 800px; /* Đặt chiều rộng tối thiểu để đảm bảo bảng không bị nén quá nhỏ */
  color: #333; /* Đậm hơn so với #666 */
}

th {
  color: #222;
  font-weight: 600;
}

td {
  color: #333;
}

/* Cho phép một số cột có nội dung dài được xuống dòng */
td[v-if*="name"],
td[v-if*="description"] {
  white-space: normal;
  max-width: 200px; /* Giới hạn chiều rộng tối đa */
}

/* Cải thiện giao diện cho các ô có ít nội dung */
td[v-if*="barcode"],
td[v-if*="brand"],
td[v-if*="status"] {
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Cải thiện giao diện cho các ô số */
td[v-if*="cost"],
td[v-if*="price"],
td[v-if*="num"] {
  max-width: 100px;
  text-align: right;
  color: #222;
  font-weight: 500;
}

tr {
  background: #fff;
}

tr:nth-child(even) {
  background: #f8fafc;
}

tr:hover {
  background: #e6f9f1;
}

td img {
  display: block;
  margin: 0 auto;
  border-radius: 4px;
  border: 1px solid #eee;
  background: #fafbfc;
}

@media (max-width: 1200px) {
  .sidebar {
    width: 220px;
    min-width: 220px;
    padding-left: 10px;
  }

  .main-content {
    padding-right: 10px;
    padding-left: 10px;
  }

  .table-container {
    margin-left: 0;
  }
}
</style>
