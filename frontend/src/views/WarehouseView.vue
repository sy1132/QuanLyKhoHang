<template>
  <div class="container mt-4">
    <!-- Header: Tiêu đề + tìm kiếm + nút chức năng cùng 1 hàng -->
    <div
      class="d-flex align-items-center gap-2 mb-4 flex-wrap justify-content-center"
    >
      <h1 class="title-black mb-0" style="font-size: 1.7rem; min-width: 220px">
        Quản lý kho hàng
      </h1>
      <input
        type="text"
        v-model="searchString"
        class="form-control"
        style="max-width: 300px"
        placeholder="Tìm kiếm theo tên, địa chỉ, quản lý..."
        @keyup.enter="searchWarehouses"
      />
      <button
        @click="searchWarehouses"
        class="btn btn-info fw-bold custom-btn-size"
      >
        Tìm kiếm
      </button>
      <button
        class="btn btn-primary custom-btn-size"
        @click="showAddModal = true"
      >
        <i class="fa fa-plus"></i> Thêm kho
      </button>
    </div>

    <!-- Bộ lọc và danh sách -->
    <div class="row mb-3">
      <!-- Bộ lọc bên trái -->
      <div class="col-md-3">
        <!-- Trạng thái kho -->
        <div class="card mb-2">
          <div class="card-header py-2 px-3">
            <strong>Trạng thái kho</strong>
          </div>
          <div class="card-body py-2 px-3">
            <div class="form-check mb-2">
              <input
                class="form-check-input"
                type="checkbox"
                v-model="filters.status.active"
                id="status-active"
                @change="applyFilters"
              />
              <label class="form-check-label" for="status-active">
                <span class="badge bg-success">Hoạt động</span>
              </label>
            </div>
            <div class="form-check mb-2">
              <input
                class="form-check-input"
                type="checkbox"
                v-model="filters.status.full"
                id="status-full"
                @change="applyFilters"
              />
              <label class="form-check-label" for="status-full">
                <span class="badge bg-warning">Đầy hàng</span>
              </label>
            </div>
            <div class="form-check">
              <input
                class="form-check-input"
                type="checkbox"
                v-model="filters.status.inactive"
                id="status-inactive"
                @change="applyFilters"
              />
              <label class="form-check-label" for="status-inactive">
                <span class="badge bg-danger">Không hoạt động</span>
              </label>
            </div>
          </div>
        </div>
        <!-- Thời gian tạo -->
        <div class="card">
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
              <button type="submit" class="btn btn-info btn-sm me-2">
                Lọc
              </button>
              <button
                type="button"
                class="btn btn-secondary btn-sm"
                @click="resetDateFilter"
              >
                Xóa
              </button>
            </form>
          </div>
        </div>
      </div>

      <!-- Danh sách kho hàng (bảng) -->
      <div class="col-md-9">
        <div class="table-responsive">
          <table class="table table-bordered table-hover">
            <thead class="table-primary">
              <tr>
                <th>Tên kho</th>
                <th>Địa chỉ</th>
                <th>Quản lý</th>
                <th>Số lượng hàng</th>
                <th>Trạng thái</th>
                <th>Ngày tạo</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="filteredWarehouses.length === 0">
                <td colspan="7" class="text-center text-muted">
                  Không có kho nào phù hợp
                </td>
              </tr>
              <tr v-for="warehouse in filteredWarehouses" :key="warehouse.id">
                <td>{{ warehouse.name }}</td>
                <td>{{ warehouse.address }}</td>
                <td>{{ warehouse.manager }}</td>
                <td class="text-center">{{ warehouse.productCount }}</td>
                <td class="text-center">
                  <span
                    :class="getStatusClass(warehouse.status)"
                    style="
                      padding: 5px 10px;
                      border-radius: 4px;
                      font-size: 0.85rem;
                    "
                  >
                    {{ getStatusText(warehouse.status) }}
                  </span>
                </td>
                <td>{{ formatDate(warehouse.createdAt) }}</td>
                <td class="text-center">
                  <div class="btn-group">
                    <button
                      class="btn btn-sm btn-info me-1"
                      @click="editWarehouse(warehouse)"
                    >
                      <i class="fa fa-edit"></i>
                    </button>
                    <button
                      class="btn btn-sm btn-danger"
                      @click="confirmDelete(warehouse)"
                    >
                      <i class="fa fa-trash"></i>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal thêm kho -->
    <div
      class="modal fade"
      :class="{ show: showAddModal }"
      tabindex="-1"
      style="display: block"
      v-if="showAddModal"
    >
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ isEditing ? "Cập nhật kho" : "Thêm kho mới" }}
            </h5>
            <button
              type="button"
              class="btn-close"
              @click="closeAddModal"
            ></button>
          </div>
          <form @submit.prevent="saveWarehouse">
            <div class="modal-body">
              <div class="row">
                <div class="col-md-6">
                  <div class="mb-2">
                    <label for="name">Tên kho:</label>
                    <input
                      type="text"
                      id="name"
                      v-model="newWarehouse.name"
                      class="form-control"
                      required
                    />
                  </div>
                  <div class="mb-2">
                    <label for="address">Địa chỉ:</label>
                    <input
                      type="text"
                      id="address"
                      v-model="newWarehouse.address"
                      class="form-control"
                      required
                    />
                  </div>
                  <div class="mb-2">
                    <label for="manager">Người quản lý:</label>
                    <input
                      type="text"
                      id="manager"
                      v-model="newWarehouse.manager"
                      class="form-control"
                      required
                    />
                  </div>
                </div>
                <div class="col-md-6">
                  <div class="mb-2">
                    <label for="phone">Điện thoại:</label>
                    <input
                      type="text"
                      id="phone"
                      v-model="newWarehouse.phone"
                      class="form-control"
                    />
                  </div>
                  <div class="mb-2">
                    <label for="email">Email:</label>
                    <input
                      type="email"
                      id="email"
                      v-model="newWarehouse.email"
                      class="form-control"
                    />
                  </div>
                  <div class="mb-2">
                    <label for="status">Trạng thái:</label>
                    <select
                      id="status"
                      v-model="newWarehouse.status"
                      class="form-control"
                      required
                    >
                      <option value="Active">Hoạt động</option>
                      <option value="Full">Đầy hàng</option>
                      <option value="Inactive">Không hoạt động</option>
                    </select>
                  </div>
                </div>
                <div class="col-12">
                  <div class="mb-2">
                    <label for="description">Mô tả:</label>
                    <textarea
                      id="description"
                      v-model="newWarehouse.description"
                      class="form-control"
                      rows="3"
                    ></textarea>
                  </div>
                </div>
              </div>
              <div
                v-if="formMessage"
                :class="[
                  'alert',
                  formSuccess ? 'alert-success' : 'alert-danger',
                  'mt-2',
                ]"
              >
                {{ formMessage }}
              </div>
            </div>
            <div class="modal-footer">
              <button type="submit" class="btn btn-success">
                {{ isEditing ? "Cập nhật" : "Lưu" }}
              </button>
              <button
                type="button"
                class="btn btn-secondary"
                @click="closeAddModal"
              >
                Bỏ qua
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal xác nhận xóa -->
    <div
      class="modal fade"
      :class="{ show: showDeleteModal }"
      tabindex="-1"
      style="display: block"
      v-if="showDeleteModal"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Xác nhận xóa</h5>
            <button
              type="button"
              class="btn-close"
              @click="showDeleteModal = false"
            ></button>
          </div>
          <div class="modal-body">
            <p>
              Bạn có chắc chắn muốn xóa kho "{{ warehouseToDelete?.name }}"
              không?
            </p>
            <p class="text-danger">Lưu ý: Hành động này không thể hoàn tác!</p>
          </div>
          <div class="modal-footer">
            <button
              type="button"
              class="btn btn-danger"
              @click="deleteWarehouse"
            >
              Xóa
            </button>
            <button
              type="button"
              class="btn btn-secondary"
              @click="showDeleteModal = false"
            >
              Hủy
            </button>
          </div>
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
      warehouses: [],
      filteredWarehouses: [],
      searchString: "",
      showAddModal: false,
      showDeleteModal: false,
      isEditing: false,
      warehouseToDelete: null,
      newWarehouse: this.getEmptyWarehouse(),
      formMessage: "",
      formSuccess: false,
      fromDate: "",
      toDate: "",
      filters: {
        status: {
          active: true,
          full: true,
          inactive: true,
        },
      },
    };
  },
  mounted() {
    this.loadWarehouses();
  },
  methods: {
    getEmptyWarehouse() {
      return {
        id: 0,
        name: "",
        address: "",
        manager: "",
        productCount: 0,
        status: "Active",
        phone: "",
        email: "",
        description: "",
        isActive: true,
      };
    },

    async loadWarehouses(searchString = "") {
      try {
        let url = "https://localhost:7189/api/warehouse";
        if (searchString) {
          url =
            "https://localhost:7189/api/warehouse/list?searchString=" +
            encodeURIComponent(searchString);
        }

        const response = await axios.get(url);
        if (
          response.data &&
          response.data.result &&
          Array.isArray(response.data.result.data)
        ) {
          this.warehouses = response.data.result.data;
        } else {
          this.warehouses = [];
        }

        this.applyFilters();
      } catch (error) {
        console.error("Lỗi khi tải danh sách kho:", error);
        alert(
          "Lỗi khi tải danh sách kho: " +
            (error.response?.data?.message || error.message)
        );
      }
    },

    async searchWarehouses() {
      await this.loadWarehouses(this.searchString);
    },

    formatDate(dateString) {
      if (!dateString) return "---";
      const date = new Date(dateString);
      return date.toLocaleString("vi-VN", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      });
    },

    getStatusText(status) {
      switch (status) {
        case "Active":
          return "Hoạt động";
        case "Full":
          return "Đầy hàng";
        case "Inactive":
          return "Không hoạt động";
        default:
          return status;
      }
    },

    getStatusClass(status) {
      switch (status) {
        case "Active":
          return "bg-success text-white";
        case "Full":
          return "bg-warning text-dark";
        case "Inactive":
          return "bg-danger text-white";
        default:
          return "bg-secondary text-white";
      }
    },

    editWarehouse(warehouse) {
      this.isEditing = true;
      this.newWarehouse = { ...warehouse };
      this.showAddModal = true;
    },

    closeAddModal() {
      this.showAddModal = false;
      this.isEditing = false;
      this.newWarehouse = this.getEmptyWarehouse();
      this.formMessage = "";
    },

    async saveWarehouse() {
      try {
        let response;
        if (this.isEditing) {
          response = await axios.put(
            `https://localhost:7189/api/warehouse/${this.newWarehouse.id}`,
            this.newWarehouse
          );
        } else {
          response = await axios.post(
            "https://localhost:7189/api/warehouse/create",
            this.newWarehouse
          );
        }

        this.formMessage = this.isEditing
          ? "Cập nhật kho thành công!"
          : "Thêm kho thành công!";
        this.formSuccess = true;

        // Reload warehouse list
        await this.loadWarehouses();

        // Close modal after short delay
        setTimeout(() => {
          this.closeAddModal();
        }, 1500);
      } catch (error) {
        this.formMessage =
          "Lỗi: " + (error.response?.data?.message || error.message);
        this.formSuccess = false;
      }
    },

    confirmDelete(warehouse) {
      this.warehouseToDelete = warehouse;
      this.showDeleteModal = true;
    },

    async deleteWarehouse() {
      if (!this.warehouseToDelete) return;

      try {
        await axios.delete(
          `https://localhost:7189/api/warehouse/${this.warehouseToDelete.id}`
        );
        await this.loadWarehouses();
        this.showDeleteModal = false;
        this.warehouseToDelete = null;
      } catch (error) {
        alert(
          "Lỗi khi xóa kho: " + (error.response?.data?.message || error.message)
        );
      }
    },

    applyFilters() {
      // Filter warehouses based on status checkboxes
      this.filteredWarehouses = this.warehouses.filter((warehouse) => {
        if (warehouse.status === "Active" && !this.filters.status.active)
          return false;
        if (warehouse.status === "Full" && !this.filters.status.full)
          return false;
        if (warehouse.status === "Inactive" && !this.filters.status.inactive)
          return false;
        return true;
      });
    },

    async filterByDate() {
      if (!this.fromDate && !this.toDate) {
        this.applyFilters();
        return;
      }

      try {
        // Assuming backend supports date filtering - this would need to be implemented
        // For now, we'll filter on the client side
        const from = this.fromDate ? new Date(this.fromDate) : null;
        const to = this.toDate ? new Date(this.toDate) : null;

        this.filteredWarehouses = this.warehouses.filter((warehouse) => {
          const createdDate = new Date(warehouse.createdAt);
          if (from && createdDate < from) return false;
          if (to) {
            // Set to end of day for "to" date
            const endOfDay = new Date(to);
            endOfDay.setHours(23, 59, 59, 999);
            if (createdDate > endOfDay) return false;
          }
          return true;
        });
      } catch (error) {
        alert("Lỗi khi lọc theo ngày: " + error.message);
      }
    },

    resetDateFilter() {
      this.fromDate = "";
      this.toDate = "";
      this.applyFilters();
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
  font-weight: 700;
}
.table-primary th {
  background-color: #007bff !important;
  color: #fff !important;
  text-align: center;
  vertical-align: middle;
}
.table-bordered th,
.table-bordered td {
  vertical-align: middle;
}
.gap-2 > * + * {
  margin-left: 0.5rem;
}
.card {
  border: 1px solid #e3e3e3;
  border-radius: 6px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
  margin-bottom: 1rem;
}
.card-header {
  background: #f8f9fa;
  font-size: 1rem;
  border-bottom: 1px solid #e3e3e3;
}
.modal {
  display: block;
  background: rgba(0, 0, 0, 0.4);
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
.form-check-label {
  cursor: pointer;
}
.table-responsive {
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.05);
}
.btn-group .btn {
  margin-right: 5px;
}
</style>
