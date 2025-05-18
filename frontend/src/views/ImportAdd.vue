<template>
  <div>
    <button @click="showModal" class="btn btn-success">
      <i class="fas fa-plus"></i> {{ buttonLabel || 'Tạo phiếu nhập mới' }}
    </button>

    <!-- Create Modal -->
    <div v-if="showImportModal" class="modal fade show" tabindex="-1" style="display: block; background-color: rgba(0,0,0,0.5);">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Tạo phiếu nhập mới</h5>
            <button type="button" class="btn-close" @click="cancelModal"></button>
          </div>
          <div class="modal-body">
            <!-- Hiển thị Loading khi đang tải dữ liệu -->
            <div v-if="isLoading" class="text-center my-3">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang tải...</span>
              </div>
              <p class="mt-2">Đang xử lý dữ liệu...</p>
            </div>
            
            <!-- Nội dung form -->
            <div v-else>
              <div class="form-group mb-3">
                <label>Kho nhập:</label>
                <select v-model="currentImport.warehouseId" class="form-select">
                  <option v-if="warehouses.length === 0" value="" disabled>Đang tải danh sách kho...</option>
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
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="cancelModal">Hủy</button>
            <button type="button" class="btn btn-primary" @click="saveImport" :disabled="isLoading">
              <span v-if="isLoading" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
              {{ isLoading ? 'Đang lưu...' : 'Lưu' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { importApi } from '../api/import.api';

const BASE_URL = 'https://localhost:7189/api';

export default {
  name: 'ImportAdd',
  props: {
    buttonLabel: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      showImportModal: false,
      isLoading: false,
      errorMessage: '',
      
      // Dữ liệu danh mục
      warehouses: [],
      products: [],
      suppliers: [],
      
      // Dữ liệu phiếu nhập mới
      currentImport: {
        warehouseId: null,
        details: []
      }
    };
  },
  
  methods: {
    showModal() {
      this.showImportModal = true;
      this.loadData();
      
      // Khởi tạo phiếu nhập mới với giá trị mặc định
      this.currentImport = {
        warehouseId: this.warehouses.length > 0 ? this.warehouses[0].id : null,
        details: [this.createEmptyDetail()]
      };
    },
    
    async loadData() {
      this.isLoading = true;
      
      try {
        // Tải danh sách kho
        if (this.warehouses.length === 0) {
          await this.loadWarehouses();
        }
        
        // Tải danh sách sản phẩm
        if (this.products.length === 0) {
          await this.loadProducts();
        }
        
        // Tải danh sách nhà cung cấp
        if (this.suppliers.length === 0) {
          await this.loadSuppliers();
        }
        
        // Cập nhật warehouseId nếu danh sách kho đã được tải
        if (this.warehouses.length > 0 && !this.currentImport.warehouseId) {
          this.currentImport.warehouseId = this.warehouses[0].id;
        }
        
        // Tạo chi tiết mặc định nếu chưa có
        if (this.currentImport.details.length === 0) {
          this.currentImport.details.push(this.createEmptyDetail());
        }
      } catch (error) {
        console.error('Error loading data:', error);
        this.errorMessage = 'Không thể tải dữ liệu. Vui lòng thử lại sau.';
      } finally {
        this.isLoading = false;
      }
    },
    
    async loadWarehouses() {
      try {
        const response = await axios.get(`${BASE_URL}/warehouse`);
        if (response.data && (response.data.data || response.data.result?.data)) {
          this.warehouses = response.data.data || response.data.result.data;
          console.log('Warehouses loaded:', this.warehouses);
        } else {
          console.error('Invalid warehouse data format:', response.data);
          this.warehouses = [];
        }
      } catch (error) {
        console.error('Error loading warehouses:', error);
        this.warehouses = [];
      }
    },
    
    async loadProducts() {
      try {
        const response = await axios.get(`${BASE_URL}/Products`);
        if (response.data && (response.data.data || response.data.result?.data)) {
          this.products = response.data.data || response.data.result.data;
          console.log('Products loaded:', this.products);
        } else {
          console.error('Invalid product data format:', response.data);
          this.products = [];
        }
      } catch (error) {
        console.error('Error loading products:', error);
        this.products = [];
      }
    },
    
    async loadSuppliers() {
      try {
        const response = await axios.get(`${BASE_URL}/supplier`);
        if (response.data && (response.data.data || response.data.result?.data)) {
          this.suppliers = response.data.data || response.data.result.data;
          console.log('Suppliers loaded:', this.suppliers);
        } else {
          console.error('Invalid supplier data format:', response.data);
          this.suppliers = [];
        }
      } catch (error) {
        console.error('Error loading suppliers:', error);
        this.suppliers = [];
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
    
    validateImport() {
      // Kiểm tra kho
      if (!this.currentImport.warehouseId) {
        this.errorMessage = 'Vui lòng chọn kho nhập';
        return false;
      }
      
      // Kiểm tra xem có chi tiết nào không
      if (this.currentImport.details.length === 0) {
        this.errorMessage = 'Phiếu nhập phải có ít nhất một sản phẩm';
        return false;
      }
      
      // Kiểm tra từng chi tiết
      for (let i = 0; i < this.currentImport.details.length; i++) {
        const detail = this.currentImport.details[i];
        
        if (!detail.idProduct) {
          this.errorMessage = `Vui lòng chọn sản phẩm ở dòng ${i+1}`;
          return false;
        }
        
        if (!detail.idSupplier) {
          this.errorMessage = `Vui lòng chọn nhà cung cấp ở dòng ${i+1}`;
          return false;
        }
        
        if (!detail.quantity || isNaN(detail.quantity) || detail.quantity <= 0) {
          this.errorMessage = `Số lượng sản phẩm ở dòng ${i+1} phải lớn hơn 0`;
          return false;
        }
        
        if (detail.cost < 0 || isNaN(detail.cost)) {
          this.errorMessage = `Đơn giá ở dòng ${i+1} không hợp lệ`;
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
    
    async saveImport() {
      try {
        if (!this.validateImport()) {
          return;
        }
        
        this.isLoading = true;
        
        const importData = {
          warehouseId: this.currentImport.warehouseId,
          details: this.currentImport.details.map(detail => ({
            idProduct: detail.idProduct,
            idSupplier: detail.idSupplier,
            quantity: Number(detail.quantity),
            cost: Number(detail.cost),
            note: detail.note || ''
          }))
        };
        
        await importApi.createImport(importData);
        alert('Tạo phiếu nhập thành công');
        
        this.showImportModal = false;
        this.$emit('import-added'); // Bắn sự kiện để parent component biết và tải lại dữ liệu
      } catch (error) {
        console.error('Error saving import:', error);
        this.errorMessage = error.response?.data?.message || 
                          'Không thể lưu phiếu nhập. Vui lòng kiểm tra kết nối và thử lại.';
      } finally {
        this.isLoading = false;
      }
    },
    
    cancelModal() {
      if (confirm('Bạn có chắc chắn muốn hủy? Các thay đổi sẽ không được lưu.')) {
        this.showImportModal = false;
      }
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

.table-light th {
  background-color: #f8f9fa;
}
</style>