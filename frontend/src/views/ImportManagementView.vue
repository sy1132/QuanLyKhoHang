<template>
  <div class="container mt-4">
    <h1 class="title-black mb-3">Quản lý phiếu nhập kho</h1>
    
    <!-- Filter section -->
    <div class="card mb-4">
      <div class="card-header py-2 px-3">
        <strong>Bộ lọc tìm kiếm</strong>
      </div>
      <div class="card-body py-2 px-3">
        <div class="row">
          <div class="col-md-3">
            <label>Trạng thái:</label>
            <select v-model="filter.status" class="form-select">
              <option value="">Tất cả</option>
              <option value="Pending">Chờ duyệt</option>
              <option value="Approved">Đã duyệt</option>
              <option value="Rejected">Đã từ chối</option>
            </select>
          </div>
          <div class="col-md-3">
            <label>Từ ngày:</label>
            <input type="date" v-model="filter.fromDate" class="form-control" />
          </div>
          <div class="col-md-3">
            <label>Đến ngày:</label>
            <input type="date" v-model="filter.toDate" class="form-control" />
          </div>
          <div class="col-md-3 d-flex align-items-end">
            <button @click="loadImports" class="btn btn-primary me-2">Tìm kiếm</button>
            <button @click="resetFilter" class="btn btn-secondary">Đặt lại</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Action buttons -->
    <div class="d-flex justify-content-between mb-3">
      <div>
        <router-link to="/import-report" class="btn btn-info me-2">
          <i class="fas fa-chart-bar"></i> Báo cáo nhập hàng
        </router-link>
      </div>
      <div>
        <button @click="showCreateModal" class="btn btn-success">
          <i class="fas fa-plus"></i> Tạo phiếu nhập mới
        </button>
      </div>
    </div>

    <!-- Imports table -->
    <div class="table-responsive">
      <table class="table table-bordered table-hover">
        <thead class="table-primary">
          <tr>
            <th>ID</th>
            <th>Kho</th>
            <th>Ngày nhập</th>
            <th>Tổng giá trị</th>
            <th>Trạng thái</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="import_ in imports" :key="import_.id">
            <td>{{ import_.id }}</td>
            <td>{{ getWarehouseName(import_.warehouseId) }}</td>
            <td>{{ formatDate(import_.dateInput) }}</td>
            <td>{{ formatCurrency(import_.cost) }}</td>
            <td>
              <span :class="getStatusBadgeClass(import_.status)">
                {{ getStatusText(import_.status) }}
              </span>
            </td>
            <td>
              <button @click="viewDetails(import_.id)" class="btn btn-info btn-sm me-1" title="Xem chi tiết">
                <i class="fas fa-eye"></i>
              </button>
              <button 
                v-if="import_.status === 'Pending'" 
                @click="editImport(import_.id)" 
                class="btn btn-warning btn-sm me-1" 
                title="Chỉnh sửa"
              >
                <i class="fas fa-edit"></i>
              </button>
              <button 
                v-if="import_.status === 'Pending'" 
                @click="approveImport(import_.id)" 
                class="btn btn-success btn-sm me-1" 
                title="Duyệt"
              >
                <i class="fas fa-check"></i>
              </button>
              <button 
                v-if="import_.status === 'Pending'" 
                @click="rejectImport(import_.id)" 
                class="btn btn-danger btn-sm me-1" 
                title="Từ chối"
              >
                <i class="fas fa-times"></i>
              </button>
              <button @click="confirmDelete(import_.id)" class="btn btn-danger btn-sm" title="Xóa">
                <i class="fas fa-trash"></i>
              </button>
            </td>
          </tr>
          <tr v-if="imports.length === 0">
            <td colspan="6" class="text-center">Không có dữ liệu</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showImportModal" class="modal fade show" tabindex="-1" style="display: block; background-color: rgba(0,0,0,0.5);">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ isEditing ? 'Chỉnh sửa phiếu nhập' : 'Tạo phiếu nhập mới' }}</h5>
            <button type="button" class="btn-close" @click="cancelModal"></button>
          </div>
          <div class="modal-body">
            <div class="form-group mb-3">
              <label>Kho nhập:</label>
              <select v-model="currentImport.warehouseId" class="form-select">
                <option v-for="warehouse in warehouses" :key="warehouse.id" :value="warehouse.id">
                  {{ warehouse.name }}
                </option>
              </select>
            </div>
            
            <h5 class="mt-4">Chi tiết phiếu nhập</h5>
            <div class="table-responsive">
              <table class="table table-bordered">
                <thead class="table-light">
                  <tr>
                    <th>Sản phẩm</th>
                    <th>Nhà cung cấp</th>
                    <th>Số lượng</th>
                    <th>Đơn giá</th>
                    <th>Thành tiền</th>
                    <th>Ghi chú</th>
                    <th>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(detail, index) in currentImport.details" :key="index">
                    <td>
                      <select v-model="detail.idProduct" class="form-select form-select-sm">
                        <option v-for="product in products" :key="product.id" :value="product.id">
                          {{ product.name }}
                        </option>
                      </select>
                    </td>
                    <td>
                      <select v-model="detail.idSupplier" class="form-select form-select-sm">
                        <option v-for="supplier in suppliers" :key="supplier.id" :value="supplier.id">
                          {{ supplier.name }}
                        </option>
                      </select>
                    </td>
                    <td>
                      <input type="number" v-model="detail.quantity" min="1" class="form-control form-control-sm">
                    </td>
                    <td>
                      <input type="number" v-model="detail.cost" min="0" class="form-control form-control-sm">
                    </td>
                    <td>{{ formatCurrency(detail.quantity * detail.cost) }}</td>
                    <td>
                      <input type="text" v-model="detail.note" class="form-control form-control-sm">
                    </td>
                    <td>
                      <button @click="removeDetail(index)" class="btn btn-danger btn-sm">
                        <i class="fas fa-trash"></i>
                      </button>
                    </td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr>
                    <td colspan="7">
                      <button @click="addDetail" class="btn btn-primary btn-sm">
                        <i class="fas fa-plus"></i> Thêm sản phẩm
                      </button>
                    </td>
                  </tr>
                  <tr>
                    <td colspan="4" class="text-end"><strong>Tổng giá trị:</strong></td>
                    <td colspan="3">{{ formatCurrency(calculateTotal()) }}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
            
            <div v-if="errorMessage" class="alert alert-danger mt-3">
              {{ errorMessage }}
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="cancelModal">Hủy</button>
            <button type="button" class="btn btn-primary" @click="saveImport">Lưu</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Details Modal -->
    <div v-if="showDetailsModal" class="modal fade show" tabindex="-1" style="display: block; background-color: rgba(0,0,0,0.5);">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Chi tiết phiếu nhập #{{ selectedImport.id }}</h5>
            <button type="button" class="btn-close" @click="showDetailsModal = false"></button>
          </div>
          <div class="modal-body">
            <div class="row mb-3">
              <div class="col-md-6">
                <p><strong>Kho nhập:</strong> {{ getWarehouseName(selectedImport.warehouseId) }}</p>
                <p><strong>Ngày nhập:</strong> {{ formatDate(selectedImport.dateInput) }}</p>
              </div>
              <div class="col-md-6">
                <p>
                  <strong>Trạng thái:</strong>
                  <span :class="getStatusBadgeClass(selectedImport.status)">
                    {{ getStatusText(selectedImport.status) }}
                  </span>
                </p>
                <p><strong>Tổng giá trị:</strong> {{ formatCurrency(selectedImport.cost) }}</p>
              </div>
            </div>
            
            <h5 class="mt-4">Danh sách sản phẩm</h5>
            <div class="table-responsive">
              <table class="table table-bordered">
                <thead class="table-light">
                  <tr>
                    <th>Sản phẩm</th>
                    <th>Nhà cung cấp</th>
                    <th>Số lượng</th>
                    <th>Đơn giá</th>
                    <th>Thành tiền</th>
                    <th>Ghi chú</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(detail, index) in importDetails" :key="index">
                    <td>{{ getProductName(detail.idProduct) }}</td>
                    <td>{{ getSupplierName(detail.idSupplier) }}</td>
                    <td>{{ detail.quantity }}</td>
                    <td>{{ formatCurrency(detail.cost) }}</td>
                    <td>{{ formatCurrency(detail.quantity * detail.cost) }}</td>
                    <td>{{ detail.note || '-' }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showDetailsModal = false">Đóng</button>
            <template v-if="selectedImport.status === 'Pending'">
              <button type="button" class="btn btn-warning" @click="editImport(selectedImport.id)">Chỉnh sửa</button>
              <button type="button" class="btn btn-success" @click="approveImport(selectedImport.id)">Duyệt</button>
              <button type="button" class="btn btn-danger" @click="rejectImport(selectedImport.id)">Từ chối</button>
            </template>
          </div>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="modal fade show" tabindex="-1" style="display: block; background-color: rgba(0,0,0,0.5);">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Xác nhận xóa</h5>
            <button type="button" class="btn-close" @click="showDeleteModal = false"></button>
          </div>
          <div class="modal-body">
            <p>Bạn có chắc chắn muốn xóa phiếu nhập kho này?</p>
            <p class="text-danger"><strong>Lưu ý:</strong> Hành động này không thể hoàn tác.</p>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showDeleteModal = false">Hủy</button>
            <button type="button" class="btn btn-danger" @click="deleteImport">Xác nhận xóa</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { importApi } from '../api/improt.api';

export default {
  name: 'ImportManagementView',
  data() {
    return {
      imports: [],
      warehouses: [],
      products: [],
      suppliers: [],
      filter: {
        status: '',
        fromDate: '',
        toDate: ''
      },
      currentImport: {
        id: null,
        warehouseId: null,
        details: []
      },
      selectedImport: {},
      importDetails: [],
      isEditing: false,
      importIdToDelete: null,
      
      // Modal visibility
      showImportModal: false,
      showDetailsModal: false,
      showDeleteModal: false,
      
      // Error message
      errorMessage: '',
      
      // Loading states
      isLoading: false
    };
  },
  mounted() {
    console.log('ImportManagementView mounted');
    this.loadImports();
    this.loadWarehouses();
    this.loadProducts();
    this.loadSuppliers();
  },
  methods: {
    // Data loading methods
    async loadImports() {
      try {
        this.isLoading = true;
        let imports = [];
        
        if (this.filter.status || this.filter.fromDate || this.filter.toDate) {
          imports = await importApi.getFilteredImports({
            status: this.filter.status,
            fromDate: this.filter.fromDate,
            toDate: this.filter.toDate
          });
        } else {
          imports = await importApi.getAllImports();
        }
        
        this.imports = imports;
        console.log('Imports loaded:', this.imports);
      } catch (error) {
        console.error('Error loading imports:', error);
        alert('Không thể tải danh sách phiếu nhập');
      } finally {
        this.isLoading = false;
      }
    },
    
    async loadWarehouses() {
      try {
        const response = await axios.get('/api/warehouse');
        if (response.data && response.data.data) {
          this.warehouses = response.data.data;
          console.log('Warehouses loaded:', this.warehouses);
        }
      } catch (error) {
        console.error('Error loading warehouses:', error);
      }
    },
    
    async loadProducts() {
      try {
        const response = await axios.get('/api/product');
        if (response.data && response.data.data) {
          this.products = response.data.data;
          console.log('Products loaded:', this.products);
        }
      } catch (error) {
        console.error('Error loading products:', error);
      }
    },
    
    async loadSuppliers() {
      try {
        const response = await axios.get('/api/supplier');
        if (response.data && response.data.data) {
          this.suppliers = response.data.data;
          console.log('Suppliers loaded:', this.suppliers);
        }
      } catch (error) {
        console.error('Error loading suppliers:', error);
      }
    },
    
    resetFilter() {
      this.filter = {
        status: '',
        fromDate: '',
        toDate: ''
      };
      this.loadImports();
    },

    // Details view methods
    async viewDetails(id) {
      try {
        const response = await importApi.getImportDetails(id);
        this.selectedImport = response.import;
        this.importDetails = response.details;
        this.showDetailsModal = true;
      } catch (error) {
        console.error('Error loading import details:', error);
        alert('Không thể tải chi tiết phiếu nhập');
      }
    },
    
    // Create/Edit methods
    showCreateModal() {
      console.log('Showing create modal');
      this.isEditing = false;
      this.currentImport = {
        warehouseId: this.warehouses.length > 0 ? this.warehouses[0].id : null,
        details: [this.createEmptyDetail()]
      };
      this.errorMessage = '';
      this.showImportModal = true;
      console.log('Modal state:', this.showImportModal);
    },
    
    async editImport(id) {
      try {
        const response = await importApi.getImportDetails(id);
        this.isEditing = true;
        
        const importData = response.import;
        const detailsData = response.details;
        
        this.currentImport = {
          id: importData.id,
          warehouseId: importData.warehouseId,
          details: detailsData.map(d => ({
            idProduct: d.idProduct,
            idSupplier: d.idSupplier,
            quantity: d.quantity,
            cost: d.cost,
            note: d.note
          }))
        };
        
        this.showDetailsModal = false;
        this.errorMessage = '';
        this.showImportModal = true;
      } catch (error) {
        console.error('Error loading import for edit:', error);
        alert('Không thể tải thông tin phiếu nhập để chỉnh sửa');
      }
    },
    
    createEmptyDetail() {
      return {
        idProduct: this.products.length > 0 ? this.products[0].id : null,
        idSupplier: this.suppliers.length > 0 ? this.suppliers[0].id : null,
        quantity: 1,
        cost: 0,
        note: ''
      };
    },
    
    addDetail() {
      this.currentImport.details.push(this.createEmptyDetail());
    },
    
    removeDetail(index) {
      if (this.currentImport.details.length > 1) {
        this.currentImport.details.splice(index, 1);
      } else {
        alert('Phiếu nhập phải có ít nhất một sản phẩm');
      }
    },
    
    calculateTotal() {
      return this.currentImport.details.reduce((sum, detail) => {
        return sum + (detail.quantity * detail.cost);
      }, 0);
    },
    
    async saveImport() {
      try {
        if (!this.validateImport()) {
          return;
        }
        
        this.isLoading = true;
        
        if (this.isEditing) {
          await importApi.updateImport(this.currentImport.id, {
            id: this.currentImport.id,
            warehouseId: this.currentImport.warehouseId,
            details: this.currentImport.details
          });
          
          alert('Cập nhật phiếu nhập thành công');
        } else {
          await importApi.createImport({
            warehouseId: this.currentImport.warehouseId,
            details: this.currentImport.details
          });
          
          alert('Tạo phiếu nhập thành công');
        }
        
        this.showImportModal = false;
        this.loadImports();
      } catch (error) {
        console.error('Error saving import:', error);
        this.errorMessage = error.response?.data?.message || 'Không thể lưu phiếu nhập';
      } finally {
        this.isLoading = false;
      }
    },
    
    validateImport() {
      if (!this.currentImport.warehouseId) {
        this.errorMessage = 'Vui lòng chọn kho nhập';
        return false;
      }
      
      for (const detail of this.currentImport.details) {
        if (!detail.idProduct) {
          this.errorMessage = 'Vui lòng chọn sản phẩm';
          return false;
        }
        
        if (!detail.idSupplier) {
          this.errorMessage = 'Vui lòng chọn nhà cung cấp';
          return false;
        }
        
        if (!detail.quantity || detail.quantity <= 0) {
          this.errorMessage = 'Số lượng sản phẩm phải lớn hơn 0';
          return false;
        }
        
        if (detail.cost < 0) {
          this.errorMessage = 'Đơn giá không được âm';
          return false;
        }
      }
      
      // Kiểm tra trùng sản phẩm
      const productIds = this.currentImport.details.map(d => d.idProduct);
      const uniqueProductIds = [...new Set(productIds)];
      if (productIds.length !== uniqueProductIds.length) {
        this.errorMessage = 'Không được chọn trùng sản phẩm trong một phiếu nhập';
        return false;
      }
      
      this.errorMessage = '';
      return true;
    },
    
    cancelModal() {
      if (confirm('Bạn có chắc chắn muốn hủy? Các thay đổi sẽ không được lưu.')) {
        this.showImportModal = false;
      }
    },
    
    // Approve/Reject methods
    async approveImport(id) {
      try {
        if (confirm('Bạn có chắc chắn muốn duyệt phiếu nhập này?')) {
          await importApi.approveImport(id);
          alert('Phiếu nhập đã được duyệt thành công');
          this.showDetailsModal = false;
          this.loadImports();
        }
      } catch (error) {
        console.error('Error approving import:', error);
        alert(error.response?.data?.message || 'Không thể duyệt phiếu nhập');
      }
    },
    
    async rejectImport(id) {
      try {
        if (confirm('Bạn có chắc chắn muốn từ chối phiếu nhập này?')) {
          await importApi.rejectImport(id);
          alert('Phiếu nhập đã bị từ chối thành công');
          this.showDetailsModal = false;
          this.loadImports();
        }
      } catch (error) {
        console.error('Error rejecting import:', error);
        alert(error.response?.data?.message || 'Không thể từ chối phiếu nhập');
      }
    },
    
    // Delete methods
    confirmDelete(id) {
      this.importIdToDelete = id;
      this.showDeleteModal = true;
    },
    
    async deleteImport() {
      try {
        await importApi.deleteImport(this.importIdToDelete);
        alert('Xóa phiếu nhập thành công');
        this.showDeleteModal = false;
        this.loadImports();
      } catch (error) {
        console.error('Error deleting import:', error);
        alert(error.response?.data?.message || 'Không thể xóa phiếu nhập');
      }
    },
    
    // Helper methods
    getWarehouseName(id) {
      const warehouse = this.warehouses.find(w => w.id === id);
      return warehouse ? warehouse.name : '';
    },
    
    getProductName(id) {
      const product = this.products.find(p => p.id === id);
      return product ? product.name : '';
    },
    
    getSupplierName(id) {
      const supplier = this.suppliers.find(s => s.id === id);
      return supplier ? supplier.name : '';
    },
    
    getStatusText(status) {
      switch (status) {
        case 'Pending': return 'Chờ duyệt';
        case 'Approved': return 'Đã duyệt';
        case 'Rejected': return 'Đã từ chối';
        default: return status;
      }
    },
    
    getStatusBadgeClass(status) {
      switch (status) {
        case 'Pending': return 'badge bg-warning text-dark';
        case 'Approved': return 'badge bg-success';
        case 'Rejected': return 'badge bg-danger';
        default: return 'badge bg-secondary';
      }
    },
    
    formatDate(dateString) {
      if (!dateString) return '';
      const date = new Date(dateString);
      return date.toLocaleDateString('vi-VN');
    },
    
    formatCurrency(value) {
      if (value === null || value === undefined) return '0 ₫';
      return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
      }).format(value);
    }
  }
};
</script>

<style scoped>
.title-black {
  color: #111 !important;
  margin-bottom: 1rem;
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

.card {
  border: 1px solid #e3e3e3;
  border-radius: 6px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.card-header {
  background: #f8f9fa;
  font-size: 1rem;
}

.badge {
  padding: 0.5em 0.75em;
  font-size: 85%;
  font-weight: 600;
  border-radius: 0.25rem;
}

/* Modal styling */
.modal {
  position: fixed;
  top: 0;
  left: 0;
  z-index: 1050;
  width: 100%;
  height: 100%;
  overflow: hidden;
  outline: 0;
}

.modal-dialog {
  position: relative;
  width: auto;
  margin: 1.75rem auto;
  max-width: 800px;
}

.modal-content {
  position: relative;
  display: flex;
  flex-direction: column;
  width: 100%;
  background-color: #fff;
  background-clip: padding-box;
  border: 1px solid rgba(0, 0, 0, 0.2);
  border-radius: 0.3rem;
  outline: 0;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-bottom: 1px solid #dee2e6;
}

.modal-body {
  position: relative;
  flex: 1 1 auto;
  padding: 1rem;
  max-height: 70vh;
  overflow-y: auto;
}

.modal-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  padding: 1rem;
  border-top: 1px solid #dee2e6;
}

.btn-close {
  background: transparent;
  border: 0;
  font-size: 1.5rem;
  padding: 0.25rem;
  color: #000;
  opacity: 0.5;
}

.btn-close:hover {
  opacity: 1;
}
</style>