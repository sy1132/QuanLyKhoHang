<template>
  <div class="container mt-4">
    <!-- Header: Tiêu đề + tìm kiếm + nút chức năng cùng 1 hàng -->
    <div class="d-flex align-items-center gap-2 mb-4 flex-wrap justify-content-center">
      <h1 class="title-black mb-0" style="font-size: 1.7rem; min-width: 220px;">Nhà cung cấp</h1>
      <input
        type="text"
        v-model="searchString"
        class="form-control"
        style="max-width: 300px"
        placeholder="Tìm kiếm theo tên, điện thoại, email..."
        @keyup.enter="searchSuppliers"
      />
      <button @click="searchSuppliers" class="btn btn-info fw-bold custom-btn-size">Tìm kiếm</button>
      <button class="btn btn-primary custom-btn-size" @click="showAddModal = true">
        <i class="fa fa-plus"></i> Nhà cung cấp
      </button>
      <button class="btn btn-success custom-btn-size" @click="showImportModal = true">
        <i class="fa fa-file-import"></i> Import
      </button>
      <button class="btn btn-secondary custom-btn-size" @click="exportSuppliers">
        <i class="fa fa-file-export"></i> Xuất file
      </button>
    </div>

    <!-- 2 bảng nhỏ filter (nếu muốn để bên trái, có thể dùng col-3) -->
    <div class="row mb-3">
      <div class="col-md-3">
        <!-- Bảng Thời gian tạo -->
        <div class="card mb-2">
          <div class="card-header py-2 px-3">
            <strong>Thời gian tạo</strong>
          </div>
          <div class="card-body py-2 px-3">
            <form @submit.prevent="filterByDate">
              <div class="mb-2">
                <label class="form-label mb-0">Từ ngày</label>
                <input type="date" v-model="fromDate" class="form-control" />
              </div>
              <div class="mb-2">
                <label class="form-label mb-0">Đến ngày</label>
                <input type="date" v-model="toDate" class="form-control" />
              </div>
              <button type="submit" class="btn btn-info btn-sm me-2">Lọc</button>
              <button type="button" class="btn btn-secondary btn-sm" @click="resetDateFilter">Xóa</button>
            </form>
          </div>
        </div>
        <!-- Bảng Kho nào -->
        <div class="card">
          <div class="card-header py-2 px-3">
            <strong>Kho nào</strong>
          </div>
          <div class="card-body py-2 px-3">
            <form @submit.prevent="filterByWarehouse">
              <div class="mb-2">
                <label class="form-label mb-0">Chọn kho</label>
                <select v-model="selectedWarehouse" class="form-select">
                  <option value="">Tất cả</option>
                  <option v-for="wid in uniqueWarehouses" :key="wid" :value="wid">
                    Kho {{ wid }}
                  </option>
                </select>
              </div>
              <button type="submit" class="btn btn-info btn-sm me-2">Lọc</button>
              <button type="button" class="btn btn-secondary btn-sm" @click="resetWarehouseFilter">Xóa</button>
            </form>
          </div>
        </div>
      </div>
      <div class="col-md-9">
        <!-- Danh sách nhà cung cấp (bảng) -->
        <div class="table-responsive">
          <table class="table table-bordered table-hover">
            <thead class="table-primary">
              <tr>
                <th>Tên nhà cung cấp</th>
                <th>Địa chỉ</th>
                <th>Điện thoại</th>
                <th>Email</th>
                <th>Mã kho</th>
                <th>Ngày tạo</th>
                <th>Ngày cập nhật</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="suppliers.length === 0">
                <td colspan="7" class="text-center text-muted">Không có nhà cung cấp nào phù hợp</td>
              </tr>
              <tr v-for="s in suppliers" :key="s.id">
                <td>{{ s.name }}</td>
                <td>{{ s.address }}</td>
                <td>{{ s.phone }}</td>
                <td>{{ s.email }}</td>
                <td>{{ s.warehouseId }}</td>
                <td>{{ new Date(s.createdAt).toLocaleString() }}</td>
                <td>{{ new Date(s.updatedAt).toLocaleString() }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal thêm nhà cung cấp -->
    <div class="modal fade" :class="{ show: showAddModal }" tabindex="-1" style="display: block;" v-if="showAddModal">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Thêm nhà cung cấp</h5>
            <button type="button" class="btn-close" @click="showAddModal = false"></button>
          </div>
          <form @submit.prevent="createSupplier">
            <div class="modal-body">
              <div class="row">
                <div class="col-md-6">
                  <div class="mb-2">
                    <label for="name">Tên nhà cung cấp:</label>
                    <input type="text" id="name" v-model="newSupplier.name" class="form-control" required />
                  </div>
                  <div class="mb-2">
                    <label for="phone">Điện thoại:</label>
                    <input type="text" id="phone" v-model="newSupplier.phone" class="form-control" />
                  </div>
                  <div class="mb-2">
                    <label for="address">Địa chỉ:</label>
                    <input type="text" id="address" v-model="newSupplier.address" class="form-control" />
                  </div>
                  <div class="mb-2">
                    <label for="warehouseId">Mã kho:</label>
                    <input type="number" id="warehouseId" v-model="newSupplier.warehouseId" class="form-control" required />
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-2">
                    <label for="email">Email:</label>
                    <input type="email" id="email" v-model="newSupplier.email" class="form-control" />
                  </div>
                </div>
              </div>
              <div v-if="createMessage" :class="['alert', createSuccess ? 'alert-success' : 'alert-danger', 'mt-2']">
                {{ createMessage }}
              </div>
            </div>
            <div class="modal-footer">
              <button type="submit" class="btn btn-success">Lưu</button>
              <button type="button" class="btn btn-secondary" @click="showAddModal = false">Bỏ qua</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal nhập Excel -->
    <div class="modal fade" :class="{ show: showImportModal }" tabindex="-1" style="display: block;" v-if="showImportModal">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Nhập file Excel</h5>
            <button type="button" class="btn-close" @click="showImportModal = false"></button>
          </div>
          <form @submit.prevent="importSuppliers">
            <div class="modal-body">
              <div class="form-group mb-2">
                <input type="file" ref="fileInput" @change="onFileChange" accept=".xlsx" class="form-control" />
              </div>
              <div v-if="importMessage" :class="['alert', importSuccess ? 'alert-success' : 'alert-danger', 'mt-2']">
                {{ importMessage }}
              </div>
            </div>
            <div class="modal-footer">
              <button type="submit" class="btn btn-success">Nhập Excel</button>
              <button type="button" class="btn btn-secondary" @click="showImportModal = false">Bỏ qua</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
export default {
  data() {
    return {
      showAddModal: false,
      showImportModal: false,
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
      fromDate: "",
      toDate: "",
      selectedWarehouse: "",
      allWarehouses: [],
    };
  },
  computed: {
    uniqueWarehouses() {
      return this.allWarehouses;
    }
  },
  mounted() {
    this.loadSuppliers();
  },
  methods: {
    async loadSuppliers(searchString = "") {
      try {
        let response;
        if (searchString) {
          response = await axios.get("https://localhost:7189/api/supplier/search", { params: { searchString } });
        } else {
          response = await axios.get("https://localhost:7189/api/supplier");
        }
        this.suppliers = response.data.data || response.data;
        // Lưu lại tất cả mã kho duy nhất khi load dữ liệu gốc
        if (!searchString) {
          const ids = this.suppliers.map(s => s.warehouseId).filter(id => id !== null && id !== undefined);
          this.allWarehouses = [...new Set(ids)];
        }
      } catch (error) {
        alert("Lỗi khi tải danh sách nhà cung cấp: " + (error.response?.data?.message || error.message));
      }
    },
    async searchSuppliers() {
      await this.loadSuppliers(this.searchString);
    },
    resetSearch() {
      this.searchString = "";
      this.loadSuppliers();
    },
    async createSupplier() {
      try {
        const response = await axios.post("https://localhost:7189/api/supplier/create", this.newSupplier);
        this.createMessage = response.data.message || "Thêm thành công!";
        this.createSuccess = true;
        this.newSupplier = { name: "", address: "", phone: "", email: "", warehouseId: 0 };
        await this.loadSuppliers();
        setTimeout(() => {
          this.showAddModal = false;
          this.createMessage = "";
        }, 1000);
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
        const response = await axios.post("https://localhost:7189/api/supplier/import", formData, {
          headers: { "Content-Type": "multipart/form-data" },
        });
        this.importMessage = response.data.message || "Nhập thành công!";
        this.importSuccess = true;
        this.selectedFile = null;
        if (this.$refs.fileInput) this.$refs.fileInput.value = null;
        await this.loadSuppliers();
        setTimeout(() => {
          this.showImportModal = false;
          this.importMessage = "";
        }, 1000);
      } catch (error) {
        this.importMessage = error.response?.data?.message || "Lỗi khi nhập file.";
        this.importSuccess = false;
      }
    },
    exportSuppliers() {
      window.location.href = "https://localhost:7189/api/supplier/export";
    },
    async filterByDate() {
      try {
        const response = await axios.get("https://localhost:7189/api/supplier/search-by-date", {
          params: { fromDate: this.fromDate, toDate: this.toDate },
        });
        this.suppliers = response.data.data || response.data;
      } catch (error) {
        alert("Lỗi khi lọc theo ngày: " + (error.response?.data?.message || error.message));
      }
    },
    resetDateFilter() {
      this.fromDate = "";
      this.toDate = "";
      this.loadSuppliers();
    },
    async filterByWarehouse() {
      if (!this.selectedWarehouse) {
        this.loadSuppliers();
        return;
      }
      try {
        const response = await axios.get("https://localhost:7189/api/supplier/search-by-warehouse", {
          params: { warehouseId: this.selectedWarehouse },
        });
        this.suppliers = response.data.data || response.data;
      } catch (error) {
        alert("Lỗi khi lọc theo kho: " + (error.response?.data?.message || error.message));
      }
    },
    resetWarehouseFilter() {
      this.selectedWarehouse = "";
      this.loadSuppliers();
    },
  },
};
</script>

<style scoped>
.container {
  padding: 20px;
}
.title-black {
  color: #111 !important;
}
.table-primary th {
  background-color: #007bff !important;
  color: #fff !important;
  text-align: center;
  vertical-align: middle;
}
.table-bordered th, .table-bordered td {
  vertical-align: middle;
}
.gap-2 > * + * {
  margin-left: 0.5rem;
}
.card {
  border: 1px solid #e3e3e3;
  border-radius: 6px;
  box-shadow: 0 1px 2px rgba(0,0,0,0.03);
}
.card-header {
  background: #f8f9fa;
  font-size: 1rem;
}
.badge {
  font-size: 1rem;
  padding: 0.5em 1em;
}
.modal {
  display: block;
  background: rgba(0,0,0,0.4);
}
.modal.fade:not(.show) {
  opacity: 0;
  pointer-events: none;
}
.custom-btn-size {
  min-width: 110px;
  min-height: 44px;
  font-size: 1rem;
  padding: 0.5rem 1.5rem;
  border-radius: 8px;
}
</style>