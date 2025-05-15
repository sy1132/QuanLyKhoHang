<template>
  <div class="container mt-4">
    <h1>Quản lý nhà cung cấp</h1>

    <!-- Form thêm nhà cung cấp -->
    <div class="mb-4">
      <h3>Thêm nhà cung cấp</h3>
      <form @submit.prevent="createSupplier">
        <div class="form-group mb-2">
          <label for="name">Tên:</label>
          <input type="text" id="name" v-model="newSupplier.name" class="form-control" required />
        </div>
        <div class="form-group mb-2">
          <label for="address">Địa chỉ:</label>
          <input type="text" id="address" v-model="newSupplier.address" class="form-control" />
        </div>
        <div class="form-group mb-2">
          <label for="phone">Số điện thoại:</label>
          <input type="text" id="phone" v-model="newSupplier.phone" class="form-control" />
        </div>
        <div class="form-group mb-2">
          <label for="email">Email:</label>
          <input type="email" id="email" v-model="newSupplier.email" class="form-control" />
        </div>
        <div class="form-group mb-2">
          <label for="warehouseId">Mã kho:</label>
          <input type="number" id="warehouseId" v-model="newSupplier.warehouseId" class="form-control" required />
        </div>
        <button type="submit" class="btn btn-primary">Thêm</button>
      </form>
      <div v-if="createMessage" :class="['alert', createSuccess ? 'alert-success' : 'alert-danger']" class="mt-2">
        {{ createMessage }}
      </div>
    </div>

    <!-- Form tìm kiếm -->
    <div class="mb-4">
      <h3>Tìm kiếm nhà cung cấp</h3>
      <div class="form-group">
        <input type="text" v-model="searchString" class="form-control" placeholder="Tìm kiếm theo tên..." />
        <button @click="searchSuppliers" class="btn btn-info mt-2">Tìm kiếm</button>
      </div>
    </div>

    <!-- Danh sách nhà cung cấp -->
    <div class="mb-4">
      <h3>Danh sách nhà cung cấp</h3>
      <table class="table table-bordered">
        <thead>
          <tr>
            <th>Tên</th>
            <th>Địa chỉ</th>
            <th>Số điện thoại</th>
            <th>Email</th>
            <th>Mã kho</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="supplier in suppliers" :key="supplier.id">
            <td>{{ supplier.name || '' }}</td>
            <td>{{ supplier.address || '' }}</td>
            <td>{{ supplier.phone || '' }}</td>
            <td>{{ supplier.email || '' }}</td>
            <td>{{ supplier.warehouseId }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Form nhập file Excel -->
    <div class="mb-4">
      <h3>Nhập file Excel</h3>
      <form @submit.prevent="importSuppliers">
        <div class="form-group mb-2">
          <input type="file" ref="fileInput" @change="onFileChange" accept=".xlsx" class="form-control" />
        </div>
        <button type="submit" class="btn btn-success">Nhập Excel</button>
      </form>
      <div v-if="importMessage" :class="['alert', importSuccess ? 'alert-success' : 'alert-danger']" class="mt-2">
        {{ importMessage }}
      </div>
    </div>

    <!-- Nút xuất file Excel -->
    <div>
      <h3>Xuất file Excel</h3>
      <button @click="exportSuppliers" class="btn btn-secondary">Xuất Excel</button>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
      suppliers: [],
      searchString: "",
      newSupplier: {
        name: "",
        address: "",
        phone: "",
        email: "",
        warehouseId: 0,
      },
      createMessage: "",
      createSuccess: false,
      importMessage: "",
      importSuccess: false,
      selectedFile: null,
    };
  },
  mounted() {
    this.loadSuppliers(); // Tải danh sách khi component được mount
  },
  methods: {
    async loadSuppliers(searchString = "") {
      try {
        const response = await axios.get("/api/supplier/list", {
          params: { searchString },
        });
        this.suppliers = response.data;
      } catch (error) {
        alert("Lỗi khi tải danh sách nhà cung cấp: " + error.message);
      }
    },
    async searchSuppliers() {
      await this.loadSuppliers(this.searchString);
    },
    async createSupplier() {
      try {
        const response = await axios.post("/api/supplier/create", this.newSupplier);
        this.createMessage = response.data.message;
        this.createSuccess = true;
        this.newSupplier = { name: "", address: "", phone: "", email: "", warehouseId: 0 }; // Reset form
        await this.loadSuppliers(); // Cập nhật danh sách
      } catch (error) {
        this.createMessage = error.response?.data?.message || "Lỗi khi thêm nhà cung cấp.";
        this.createSuccess = false;
      }
    },
    onFileChange(event) {
      this.selectedFile = event.target.files[0];
    },
    async importSuppliers() {
      if (!this.selectedFile) {
        this.importMessage = "Vui lòng chọn file Excel.";
        this.importSuccess = false;
        return;
      }

      const formData = new FormData();
      formData.append("file", this.selectedFile);

      try {
        const response = await axios.post("/api/supplier/import", formData, {
          headers: { "Content-Type": "multipart/form-data" },
        });
        this.importMessage = response.data.message;
        this.importSuccess = true;
        this.selectedFile = null;
        this.$refs.fileInput.value = null; // Reset input file
        await this.loadSuppliers(); // Cập nhật danh sách
      } catch (error) {
        this.importMessage = error.response?.data?.message || "Lỗi khi nhập file.";
        this.importSuccess = false;
      }
    },
    exportSuppliers() {
      window.location.href = "/api/supplier/export";
    },
  },
};
</script>

<style scoped>
.container {
  padding: 20px;
}
</style>