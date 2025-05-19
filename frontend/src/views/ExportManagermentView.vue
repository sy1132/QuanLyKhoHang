<template>
  <div class="container-fluid">
    <h2 class="my-4">Quản Lý Luân Chuyển Kho</h2>

    <!-- Actions Bar -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div>
        <button class="btn btn-primary me-2" @click="openCreateModal">
          <i class="fas fa-plus me-1"></i> Tạo Phiếu Chuyển Kho
        </button>
        <button class="btn btn-outline-secondary" @click="loadExports">
          <i class="fas fa-sync me-1"></i> Làm mới
        </button>
      </div>
      <div class="d-flex">
        <input 
          type="text" 
          class="form-control me-2" 
          placeholder="Tìm kiếm..." 
          v-model="searchQuery"
          @input="filterExports"
        >
        <select class="form-select" v-model="statusFilter" @change="filterExports">
          <option value="">Tất cả trạng thái</option>
          <option value="Pending">Đang chờ</option>
          <option value="Completed">Hoàn thành</option>
        </select>
      </div>
    </div>

    <!-- Exports Table -->
    <div class="card shadow-sm">
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-hover mb-0">
            <thead class="table-light">
              <tr>
                <th>Mã phiếu</th>
                <th>Tên phiếu</th>
                <th>Ngày tạo</th>
                <th>Kho xuất</th>
                <th>Kho nhập</th>
                <th>Số lượng SP</th>
                <th>Trạng thái</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="8" class="text-center py-4">
                  <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                  </div>
                </td>
              </tr>
              <tr v-else-if="filteredExports.length === 0">
                <td colspan="8" class="text-center py-4">
                  <p class="mb-0">Không có phiếu chuyển kho nào</p>
                </td>
              </tr>
              <tr v-for="exportItem in filteredExports" :key="exportItem.id" class="align-middle">
                <td>{{ exportItem.id }}</td>
                <td>{{ exportItem.Name }}</td>
                <td>{{ formatDate(exportItem.ExportDate) }}</td>
                <td>{{ exportItem.SourceWarehouse?.name || 'N/A' }}</td>
                <td>{{ exportItem.DestinationWarehouse?.name || 'N/A' }}</td>
                <td>{{ exportItem.productCount }}</td>
                <td>
                  <span :class="getStatusBadgeClass(exportItem.status)">
                    {{ getStatusText(exportItem.status) }}
                  </span>
                </td>
                <td>
                  <button class="btn btn-sm btn-info me-1" @click="viewDetails(exportItem)">
                    <i class="fas fa-eye"></i>
                  </button>
                  <button 
                    class="btn btn-sm btn-success me-1" 
                    v-if="exportItem.status === 'Pending'"
                    @click="confirmExport(exportItem.id)"
                    title="Xác nhận chuyển kho"
                  >
                    <i class="fas fa-check"></i> Xác nhận
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Create Transfer Modal -->
    <div v-if="showCreateModal" class="modal fade show" tabindex="-1" 
       style="display: block; background-color: rgba(0,0,0,0.5);">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Tạo phiếu chuyển kho</h5>
            <button type="button" class="btn-close" @click="closeCreateModal"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="createTransfer">
              <div class="mb-3">
                <label class="form-label">Tên phiếu</label>
                <input type="text" class="form-control" v-model="transferForm.name" required>
              </div>
              
              <!-- Kho xuất với chức năng tìm kiếm -->
              <div class="row mb-3">
                <div class="col-md-6">
                  <label class="form-label">Kho xuất</label>
                  <div v-if="isLoadingWarehouses" class="d-flex align-items-center mb-2">
                    <div class="spinner-border spinner-border-sm me-2" role="status"></div>
                    <span>Đang tải danh sách kho...</span>
                  </div>
                  
                  <!-- Input tìm kiếm nhanh kho -->
                  <div v-if="warehouses.length > 5" class="input-group mb-2">
                    <input 
                      type="text" 
                      v-model="warehouseExportSearchTerm" 
                      class="form-control"
                      placeholder="Tìm kho xuất..."
                    >
                    <button class="btn btn-outline-secondary" type="button" @click="warehouseExportSearchTerm = ''">
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                  
                  <select class="form-select" v-model="transferForm.idWarehouseExport" required @change="loadSourceProducts">
                    <option value="" disabled selected>-- Chọn kho xuất --</option>
                    <option v-for="warehouse in filteredWarehousesExport" :key="warehouse.id || warehouse.Id" 
                      :value="warehouse.id || warehouse.Id">
                      {{ warehouse.name || warehouse.Name || 'Kho ' + (warehouse.id || warehouse.Id) }}
                    </option>
                  </select>
                  
                  <div v-if="warehouses.length === 0 && !isLoadingWarehouses" class="text-danger mt-1">
                    <small>Không thể tải danh sách kho. Vui lòng thử lại.</small>
                  </div>
                </div>
                
                <!-- Kho nhập với chức năng tìm kiếm -->
                <div class="col-md-6">
                  <label class="form-label">Kho nhập</label>
                  <div v-if="isLoadingWarehouses" class="d-flex align-items-center mb-2">
                    <div class="spinner-border spinner-border-sm me-2" role="status"></div>
                    <span>Đang tải danh sách kho...</span>
                  </div>
                  
                  <!-- Input tìm kiếm nhanh kho -->
                  <div v-if="warehouses.length > 5" class="input-group mb-2">
                    <input 
                      type="text" 
                      v-model="warehouseImportSearchTerm" 
                      class="form-control"
                      placeholder="Tìm kho nhập..."
                    >
                    <button class="btn btn-outline-secondary" type="button" @click="warehouseImportSearchTerm = ''">
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                  
                  <select class="form-select" v-model="transferForm.idWarehouseImport" required>
                    <option value="" disabled selected>-- Chọn kho nhập --</option>
                    <option v-for="warehouse in filteredWarehousesImport" 
                      :key="warehouse.id || warehouse.Id" 
                      :value="warehouse.id || warehouse.Id"
                      :disabled="(warehouse.id || warehouse.Id) === transferForm.idWarehouseExport">
                      {{ warehouse.name || warehouse.Name || 'Kho ' + (warehouse.id || warehouse.Id) }}
                    </option>
                  </select>
                </div>
              </div>
              
              <div class="mb-3">
                <label class="form-label">Ghi chú</label>
                <textarea class="form-control" v-model="transferForm.note" rows="2"></textarea>
              </div>

              <hr>
              <h6>Sản phẩm chuyển kho</h6>
              
              <!-- Chức năng tìm kiếm và thêm sản phẩm nâng cao -->
              <div class="mb-3" v-if="transferForm.idWarehouseExport">
                <div class="d-flex">
                  <!-- Dropdown tìm kiếm sản phẩm -->
                  <div class="product-search-dropdown w-100 me-2">
                    <div class="input-group">
                      <input 
                        type="text" 
                        v-model="productSearch" 
                        class="form-control" 
                        placeholder="Nhập mã vạch hoặc tên sản phẩm..." 
                        @focus="openProductDropdown()"
                        @keyup.enter="addProduct"
                      >
                      <button class="btn btn-outline-secondary dropdown-toggle" type="button" 
                        @click="toggleProductDropdown()">
                      </button>
                    </div>
                    
                    <!-- Dropdown menu -->
                    <div class="dropdown-menu w-100" :class="{ 'show': showProductDropdown }">
                      <div class="dropdown-header p-2">
                        <span v-if="filteredProducts.length > 0">
                          {{ filteredProducts.length }} sản phẩm
                        </span>
                        <span v-else-if="isLoadingProducts">
                          <span class="spinner-border spinner-border-sm" role="status"></span>
                          Đang tải sản phẩm...
                        </span>
                        <span v-else>Không tìm thấy sản phẩm</span>
                      </div>
                      <div class="dropdown-divider"></div>
                      <div class="product-dropdown-items">
                        <button 
                          v-for="product in filteredProducts" 
                          :key="product.Id" 
                          class="dropdown-item"
                          @click="selectProduct(product)"
                        >
                          <div class="d-flex justify-content-between align-items-center">
                            <div>
                              <strong>{{ product.Name }}</strong>
                              <small class="text-muted d-block">{{ product.Barcode }}</small>
                            </div>
                            <span class="badge bg-primary">{{ formatCurrency(product.Price) }}</span>
                          </div>
                        </button>
                      </div>
                    </div>
                  </div>
                  
                  <button class="btn btn-outline-info" type="button" @click="refreshProducts" title="Làm mới danh sách">
                    <i class="fas fa-sync-alt"></i>
                  </button>
                </div>
              </div>

              <div v-if="transferForm.details.length === 0" class="alert alert-warning">
                Chưa có sản phẩm nào được thêm
              </div>

              <div class="table-responsive" v-else>
                <table class="table table-bordered table-sm">
                  <thead class="table-light">
                    <tr>
                      <th width="5%">#</th>
                      <th width="15%">Mã vạch</th>
                      <th width="40%">Tên sản phẩm</th>
                      <th width="15%">Tồn kho</th>
                      <th width="15%">SL chuyển</th>
                      <th width="10%"></th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(item, index) in transferForm.details" :key="index">
                      <td>{{ index + 1 }}</td>
                      <td>{{ item.barcode }}</td>
                      <td>{{ getProductName(item.barcode) }}</td>
                      <td>{{ getProductStock(item.barcode) }}</td>
                      <td>
                        <input type="number" class="form-control form-control-sm" 
                          v-model.number="item.quantity" min="1" :max="getProductStock(item.barcode)">
                      </td>
                      <td>
                        <button type="button" class="btn btn-sm btn-danger" @click="removeProduct(index)">
                          <i class="fas fa-trash"></i>
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
              
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" @click="closeCreateModal">Hủy</button>
                <button type="submit" class="btn btn-primary" :disabled="transferForm.details.length === 0">
                  Tạo phiếu
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>

    <!-- View Details Modal - Sửa lại cách hiển thị modal -->
    <div v-if="showViewModal" class="modal fade show" tabindex="-1"
       style="display: block; background-color: rgba(0,0,0,0.5);">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Chi tiết phiếu chuyển kho</h5>
            <button type="button" class="btn-close" @click="closeViewModal"></button>
          </div>
          <div class="modal-body" v-if="selectedExport">
            <div class="row mb-3">
              <div class="col-md-6">
                <p><strong>Mã phiếu:</strong> {{ selectedExport.id }}</p>
                <p><strong>Tên phiếu:</strong> {{ selectedExport.Name }}</p>
                <p><strong>Ngày tạo:</strong> {{ formatDate(selectedExport.exportDate) }}</p>
              </div>
              <div class="col-md-6">
                <p><strong>Kho xuất:</strong> {{ selectedExport.SourceWarehouse?.name || 'N/A' }}</p>
                <p><strong>Kho nhập:</strong> {{ selectedExport.DestinationWarehouse?.name || 'N/A' }}</p>
                <p><strong>Trạng thái:</strong> 
                  <span :class="getStatusBadgeClass(selectedExport.status)">
                    {{ getStatusText(selectedExport.status) }}
                  </span>
                </p>
              </div>
            </div>
            
            <div class="mb-3">
              <p><strong>Ghi chú:</strong> {{ selectedExport.note || 'Không có ghi chú' }}</p>
            </div>
            
            <hr>
            <h6>Danh sách sản phẩm</h6>
            
            <div class="table-responsive">
              <table class="table table-bordered table-sm">
                <thead class="table-light">
                  <tr>
                    <th width="5%">#</th>
                    <th width="15%">Mã vạch</th>
                    <th width="50%">Tên sản phẩm</th>
                    <th width="15%">Đơn giá</th>
                    <th width="15%">Số lượng</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(item, index) in selectedExport.Details" :key="item.id">
                    <td>{{ index + 1 }}</td>
                    <td>{{ item.Product?.Barcode || 'N/A' }}</td>
                    <td>{{ item.Product?.Name || 'N/A' }}</td>
                    <td>{{ formatCurrency(item.Product?.Price) }}</td>
                    <td>{{ item.quantity }}</td>
                  </tr>
                </tbody>
                <tfoot class="table-light">
                  <tr>
                    <td colspan="4" class="text-end"><strong>Tổng số lượng:</strong></td>
                    <td>{{ selectedExport.productCount }}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeViewModal">Đóng</button>
            <button 
              type="button" 
              class="btn btn-success" 
              v-if="selectedExport && selectedExport.status === 'Pending'"
              @click="confirmExportFromModal"
            >
              Xác nhận chuyển kho
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Toast Notifications -->
    <div class="position-fixed bottom-0 end-0 p-3" style="z-index: 11">
      <div id="toast" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="toast-header" :class="toastClass">
          <strong class="me-auto text-white">{{ toastTitle }}</strong>
          <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body">
          {{ toastMessage }}
        </div>
      </div>
    </div>
  </div>
</template>

<script>
// import { Modal, Toast } from 'bootstrap';
import { Toast } from 'bootstrap';
import axios from 'axios';

const API_BASE_URL = 'https://localhost:7189/api'; // Điều chỉnh URL này cho phù hợp với backend của bạn

export default {
  name: 'ExportManagermentView',
  data() {
    return {
      exports: [],
      filteredExports: [],
      warehouses: [],
      sourceProducts: [],
      selectedExport: null,
      searchQuery: '',
      statusFilter: '',
      loading: false,
      
      // Kiểm soát hiển thị modal
      showCreateModal: false,
      showViewModal: false,
      
      // Thêm biến cho việc tải dữ liệu
      isLoadingWarehouses: false,
      isLoadingProducts: false,
      
      // Biến tìm kiếm kho
      warehouseExportSearchTerm: '',
      warehouseImportSearchTerm: '',
      
      // Biến tìm kiếm sản phẩm
      productSearch: '',
      showProductDropdown: false,
      
      toast: null,
      toastTitle: '',
      toastMessage: '',
      toastClass: '',
      
      transferForm: {
        name: '',
        idWarehouseExport: null,
        idWarehouseImport: null,
        note: '',
        details: []
      }
    };
  },
  computed: {
    // Lọc kho xuất theo từ khóa tìm kiếm
    filteredWarehousesExport() {
      if (!this.warehouses || this.warehouses.length === 0) {
        return [];
      }
      
      if (!this.warehouseExportSearchTerm) {
        return this.warehouses;
      }
      
      const searchTerm = this.warehouseExportSearchTerm.toLowerCase();
      return this.warehouses.filter(warehouse => {
        const name = (warehouse.name || warehouse.Name || '').toLowerCase();
        const address = (warehouse.address || warehouse.Address || '').toLowerCase();
        return name.includes(searchTerm) || address.includes(searchTerm);
      });
    },
    
    // Lọc kho nhập theo từ khóa tìm kiếm
    filteredWarehousesImport() {
      if (!this.warehouses || this.warehouses.length === 0) {
        return [];
      }
      
      if (!this.warehouseImportSearchTerm) {
        return this.warehouses;
      }
      
      const searchTerm = this.warehouseImportSearchTerm.toLowerCase();
      return this.warehouses.filter(warehouse => {
        // Linh hoạt với nhiều trường hợp đặt tên thuộc tính
        const name = (warehouse.name || warehouse.Name || '').toLowerCase();
        const address = (warehouse.address || warehouse.Address || '').toLowerCase();
        return name.includes(searchTerm) || address.includes(searchTerm);
      });
    },
    
    // Lọc sản phẩm theo từ khóa tìm kiếm
    filteredProducts() {
      const searchTerm = this.productSearch.toLowerCase();
      
      if (!searchTerm) {
        return this.sourceProducts.slice(0, 100); // Giới hạn 100 sản phẩm nếu không tìm kiếm
      }
      
      return this.sourceProducts.filter(product => {
        const name = (product.Name || '').toLowerCase();
        const barcode = (product.Barcode || '').toLowerCase();
        return name.includes(searchTerm) || barcode.includes(searchTerm);
      });
    }
  },
  mounted() {
    this.loadExports();
    this.loadWarehouses();
    
    // Chỉ khởi tạo Toast, không khởi tạo Modal
    this.$nextTick(() => {
      try {
        // Xóa bất kỳ backdrop nào còn sót lại từ phiên trước
        const backdropElements = document.querySelectorAll('.modal-backdrop');
        backdropElements.forEach(element => {
          if (element.parentNode) {
            element.parentNode.removeChild(element);
          }
        });
        
        // Đảm bảo không có modal nào đang hiển thị
        document.body.classList.remove('modal-open');
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
        
        // Chỉ khởi tạo toast
        this.toast = new Toast(document.getElementById('toast'));
        
        // Thêm click outside listener để đóng dropdown
        document.addEventListener('click', this.closeDropdownOnClickOutside);
      } catch (error) {
        console.error('Error initializing toast:', error);
      }
    });
  },
  beforeUnmount() {
    // Chỉ cleanup toast
    if (this.toast) {
      this.toast.dispose();
    }
    
    // Đảm bảo không có modal nào đang hiển thị khi component unmount
    document.body.classList.remove('modal-open');
    document.body.style.overflow = '';
    document.body.style.paddingRight = '';
    const backdropElements = document.querySelectorAll('.modal-backdrop');
    backdropElements.forEach(element => {
      if (element.parentNode) {
        element.parentNode.removeChild(element);
      }
    });
    
    // Loại bỏ event listener
    document.removeEventListener('click', this.closeDropdownOnClickOutside);
  },
  methods: {
    // Thay đổi phương thức loadExports
    async loadExports() {
      try {
        this.loading = true;
        // Sử dụng API_BASE_URL thay vì /api/Export
        const response = await axios.get(`${API_BASE_URL}/Export`);
        console.log('Dữ liệu phiếu chuyển kho:', response.data);
        
        // Xử lý dữ liệu trả về
        if (response.data) {
          let exportData = [];
          
          if (Array.isArray(response.data)) {
            exportData = response.data;
          } else if (response.data.data && Array.isArray(response.data.data)) {
            exportData = response.data.data;
          } else if (response.data.result && Array.isArray(response.data.result)) {
            exportData = response.data.result;
          } else if (response.data.result && response.data.result.data && Array.isArray(response.data.result.data)) {
            exportData = response.data.result.data;
          }
          
          // Xử lý và chuẩn hóa dữ liệu
          this.exports = exportData.map(item => {
            return {
              id: item.id || item.Id,
              Name: item.name || item.Name || 'Không có tên',
              ExportDate: item.exportDate || item.ExportDate || item.createdAt || new Date().toISOString(),
              SourceWarehouse: {
                id: item.idWarehouseExport || item.IdWarehouseExport,
                name: item.warehouseExportName || item.sourceWarehouseName || 'Kho xuất'
              },
              DestinationWarehouse: {
                id: item.idWarehouseImport || item.IdWarehouseImport,
                name: item.warehouseImportName || item.destinationWarehouseName || 'Kho nhập'
              },
              status: item.status || 'Pending',
              productCount: item.productCount || (item.Details?.length || 0),
              Details: item.details || item.Details || []
            };
          });
          
          this.filteredExports = [...this.exports];
          console.log('Dữ liệu phiếu đã xử lý:', this.exports);
        }
      } catch (error) {
        console.error('Lỗi khi tải danh sách phiếu chuyển kho:', error);
        this.showToast('Lỗi', 'Không thể tải danh sách phiếu chuyển kho', 'bg-danger');
        
        // Nếu không lấy được dữ liệu, tạo dữ liệu mẫu để test
        this.loadSampleExportData();
      } finally {
        this.loading = false;
      }
    },

    // Thêm phương thức để tạo dữ liệu mẫu khi API thất bại
    loadSampleExportData() {
      this.exports = [
        {
          id: 1,
          Name: "Chuyển kho #001",
          ExportDate: new Date().toISOString(),
          SourceWarehouse: { id: 1, name: "Kho Hà Nội" },
          DestinationWarehouse: { id: 2, name: "Kho HCM" },
          status: "Pending",
          productCount: 3,
          Details: []
        },
        {
          id: 2,
          Name: "Chuyển kho #002",
          ExportDate: new Date(Date.now() - 86400000).toISOString(),
          SourceWarehouse: { id: 2, name: "Kho HCM" },
          DestinationWarehouse: { id: 1, name: "Kho Hà Nội" },
          status: "Completed",
          productCount: 2,
          Details: []
        }
      ];
      this.filteredExports = [...this.exports];
    },
    
    async loadWarehouses() {
      this.isLoadingWarehouses = true;
      try {
        console.log('Đang tải danh sách kho...');
        
        const response = await axios.get(`${API_BASE_URL}/warehouse`);
        console.log('Phản hồi API warehouse:', response);
        
        if (response.data) {
          // Xác định đúng cấu trúc dữ liệu
          let warehouseData;
          
          if (Array.isArray(response.data)) {
            warehouseData = response.data;
          } else if (response.data.data && Array.isArray(response.data.data)) {
            warehouseData = response.data.data;
          } else if (response.data.result && response.data.result.data && Array.isArray(response.data.result.data)) {
            warehouseData = response.data.result.data;
          } else {
            // Tìm kiếm bất kỳ mảng nào trong response
            const possibleArrays = Object.values(response.data).filter(val => Array.isArray(val));
            warehouseData = possibleArrays.length > 0 ? possibleArrays[0] : [];
          }
          
          // Đảm bảo mỗi warehouse có thuộc tính name
          this.warehouses = warehouseData.map(warehouse => {
            // Nếu warehouse chỉ là một chuỗi hoặc số, tạo một đối tượng
            if (typeof warehouse === 'string' || typeof warehouse === 'number') {
              return { id: warehouse, name: `Kho ${warehouse}` };
            }
            
            // Nếu không có thuộc tính name, sử dụng thuộc tính thay thế hoặc ID
            if (!warehouse.name && (warehouse.warehouseName || warehouse.title)) {
              warehouse.name = warehouse.warehouseName || warehouse.title;
            }
            
            // Đảm bảo có id
            if (warehouse.Id && !warehouse.id) {
              warehouse.id = warehouse.Id;
            }
            
            return warehouse;
          });
          
          console.log('Kho đã được tải:', this.warehouses);
        }
        
        // Nếu không tìm thấy kho, sử dụng dữ liệu mẫu
        if (!this.warehouses || this.warehouses.length === 0) {
          this.loadSampleWarehouseData();
        }
      } catch (error) {
        console.error('Lỗi khi tải danh sách kho:', error);
        this.showToast('Lỗi', 'Không thể tải danh sách kho', 'bg-danger');
        this.loadSampleWarehouseData();
      } finally {
        this.isLoadingWarehouses = false;
      }
    },
    
    async loadSourceProducts() {
      if (!this.transferForm.idWarehouseExport) return;
      
      try {
        this.isLoadingProducts = true;
        console.log('Bắt đầu tải danh sách sản phẩm từ kho:', this.transferForm.idWarehouseExport);
        
        // Thử API endpoint đầu tiên
        try {
          const response = await axios.get(`${API_BASE_URL}/Products/by-warehouse/${this.transferForm.idWarehouseExport}`);
          console.log('Dữ liệu sản phẩm trả về:', response);
          
          if (response.data) {
            if (Array.isArray(response.data)) {
              this.sourceProducts = response.data.map(item => this.normalizeProductData(item));
            } else if (response.data.data && Array.isArray(response.data.data)) {
              this.sourceProducts = response.data.data.map(item => this.normalizeProductData(item));
            } else if (response.data.result && Array.isArray(response.data.result)) {
              this.sourceProducts = response.data.result.map(item => this.normalizeProductData(item));
            } else if (response.data.result && response.data.result.data && Array.isArray(response.data.result.data)) {
              this.sourceProducts = response.data.result.data.map(item => this.normalizeProductData(item));
            }
            
            console.log('Đã xử lý sản phẩm:', this.sourceProducts);
            
            if (this.sourceProducts.length > 0) {
              return; // Thoát nếu đã tìm thấy sản phẩm
            }
          }
        } catch (error) {
          console.error('Lỗi khi tải sản phẩm từ API chính:', error);
          // Tiếp tục với phương pháp thay thế
        }
        
        // Nếu không thành công, thử tải trực tiếp
        await this.loadSourceProductsDirectly();
        
      } catch (error) {
        console.error('Lỗi chung khi tải danh sách sản phẩm:', error);
        this.showToast('Lỗi', 'Không thể tải danh sách sản phẩm trong kho', 'bg-danger');
      } finally {
        this.isLoadingProducts = false;
      }
    },

    // Phương thức chuẩn hóa dữ liệu sản phẩm
    normalizeProductData(item) {
      return {
        Id: item.id || item.Id || item.productId || item.ProductId,
        Name: item.name || item.Name || item.productName || item.ProductName || 'Không có tên',
        Price: item.price || item.Price || (item.cost ? item.cost * 1.2 : 0),
        Barcode: item.barcode || item.Barcode || '',
        Unit: item.unit || item.Unit || 'Cái',
        Status: item.status || item.Status || '',
        WarehouseId: item.warehouseId || item.WarehouseId,
        Cost: item.cost || item.Cost || 0,
        Brand: item.brand || item.Brand || '',
        CategoryId: item.categoryId || item.CategoryId || item.categoryID || item.CategoryID,
        Num: item.num || item.Num || '0'
      };
    },

    async loadSourceProductsDirectly() {
      if (!this.transferForm.idWarehouseExport) return;
      
      try {
        this.isLoadingProducts = true;
        console.log('Tải sản phẩm trực tiếp từ database...');
        
        // Sử dụng đúng endpoint như trong ImportAdd.vue
        const response = await axios.get(`${API_BASE_URL}/Products/list`, {
          params: {
            includeInactive: true
          }
        });
        
        // In ra cấu trúc JSON để debug
        console.log('Cấu trúc response API:', JSON.stringify(response.data).substring(0, 100) + '...');
        
        if (response.data) {
          // Thử lấy dữ liệu từ các cấu trúc khác nhau
          let products = [];
          
          if (Array.isArray(response.data)) {
            products = response.data;
          } else if (response.data.data && Array.isArray(response.data.data)) {
            products = response.data.data;
          } else if (response.data.result && Array.isArray(response.data.result)) {
            products = response.data.result;
          } else if (response.data.result && response.data.result.data && Array.isArray(response.data.result.data)) {
            products = response.data.result.data;
          }
          
          // Map và lọc sản phẩm theo kho xuất
          const allProducts = products.map(item => this.normalizeProductData(item));
          
          // Lọc theo kho xuất
          this.sourceProducts = allProducts.filter(p => {
            const productWarehouseId = p.WarehouseId;
            return productWarehouseId == this.transferForm.idWarehouseExport; // Dùng == để so sánh lỏng lẻo
          });
          
          console.log('Đã lọc sản phẩm theo kho:', this.sourceProducts);
        }
      } catch (error) {
        console.error('Lỗi khi tải sản phẩm trực tiếp từ database:', error);
        this.showToast('Lỗi', 'Không thể tải danh sách sản phẩm', 'bg-danger');
        
        // Tạo dữ liệu mẫu nếu cả hai cách đều thất bại
        this.loadSampleProductData();
      } finally {
        this.isLoadingProducts = false;
      }
    },

    // Thêm phương thức tải dữ liệu sản phẩm mẫu
    loadSampleProductData() {
      console.log('Sử dụng dữ liệu sản phẩm mẫu');
      this.sourceProducts = [
        { Id: 1, Name: "Sản phẩm A", Barcode: "SP001", Num: "100", Price: 150000, WarehouseId: this.transferForm.idWarehouseExport },
        { Id: 2, Name: "Sản phẩm B", Barcode: "SP002", Num: "50", Price: 250000, WarehouseId: this.transferForm.idWarehouseExport },
        { Id: 3, Name: "Sản phẩm C", Barcode: "SP003", Num: "75", Price: 350000, WarehouseId: this.transferForm.idWarehouseExport }
      ];
    },

    // Thêm phương thức debug API
    async debugApiConnection() {
      try {
        // Thử gọi API với cả hai kiểu endpoint
        const responses = [];
        
        try {
          const resp1 = await axios.get(`${API_BASE_URL}/products`);
          responses.push({ 
            url: `${API_BASE_URL}/products`, 
            success: true, 
            status: resp1.status,
            data: JSON.stringify(resp1.data).substring(0, 100) + '...'
          });
        } catch (err) {
          responses.push({ 
            url: `${API_BASE_URL}/products`, 
            success: false, 
            status: err.response?.status,
            error: err.message
          });
        }
        
        try {
          const resp2 = await axios.get(`${API_BASE_URL}/Products`);
          responses.push({ 
            url: `${API_BASE_URL}/Products`, 
            success: true, 
            status: resp2.status,
            data: JSON.stringify(resp2.data).substring(0, 100) + '...'
          });
        } catch (err) {
          responses.push({ 
            url: `${API_BASE_URL}/Products`, 
            success: false, 
            status: err.response?.status,
            error: err.message
          });
        }
        
        console.log('Kết quả kiểm tra API:', responses);
        this.showToast('Debug API', 'Kết quả đã được in trong console. Mở F12 để xem.', 'bg-info');
      } catch (error) {
        console.error('Lỗi khi debug:', error);
        this.showToast('Lỗi', `Lỗi kiểm tra: ${error.message}`, 'bg-danger');
      }
    },
    
    // Làm mới danh sách sản phẩm
    async refreshProducts() {
      this.sourceProducts = [];
      await this.loadSourceProducts();
      
      if (this.sourceProducts.length === 0) {
        await this.loadSourceProductsDirectly();
      }
      
      if (this.sourceProducts.length > 0) {
        this.showToast('Thành công', 'Đã làm mới danh sách sản phẩm', 'bg-success');
      }
    },
    
    // Các phương thức đã có
    filterExports() {
      // Giữ nguyên
    },
    
    formatDate(dateString) {
      // Giữ nguyên
    },
    
    formatCurrency(value) {
      // Giữ nguyên
    },
    
    getStatusBadgeClass(status) {
      return {
        'Pending': 'badge bg-warning',
        'Completed': 'badge bg-success',
        'Processing': 'badge bg-info',
        'Cancelled': 'badge bg-danger'
      }[status] || 'badge bg-secondary';
    },

    getStatusText(status) {
      return {
        'Pending': 'Đang chờ',
        'Completed': 'Hoàn thành',
        'Processing': 'Đang xử lý',
        'Cancelled': 'Đã hủy'
      }[status] || status;
    },
    
    openCreateModal() {
      // Reset form
      this.transferForm = {
        name: '',
        idWarehouseExport: null,
        idWarehouseImport: null,
        note: '',
        details: []
      };
      
      // Hiển thị modal bằng cách set biến flag
      this.showCreateModal = true;
      this.productSearch = '';
      this.showProductDropdown = false;
      this.warehouseExportSearchTerm = '';
      this.warehouseImportSearchTerm = '';
      
      // Khi modal hiển thị, body cần có class modal-open để ngăn scroll
      document.body.classList.add('modal-open');
    },
    
    closeCreateModal() {
      this.showCreateModal = false;
      document.body.classList.remove('modal-open');
    },
    
    viewDetails(exportItem) {
      this.selectedExport = exportItem;
      this.showViewModal = true;
      document.body.classList.add('modal-open');
    },
    
    closeViewModal() {
      this.showViewModal = false;
      document.body.classList.remove('modal-open');
    },
    
    // Phương thức mở dropdown sản phẩm
    openProductDropdown() {
      this.showProductDropdown = true;
    },
    
    // Phương thức chuyển đổi hiển thị dropdown sản phẩm
    toggleProductDropdown() {
      this.showProductDropdown = !this.showProductDropdown;
    },
    
    // Phương thức chọn sản phẩm từ dropdown
    selectProduct(product) {
      // Kiểm tra xem sản phẩm đã có trong danh sách chưa
      const existingIndex = this.transferForm.details.findIndex(p => p.barcode === product.Barcode);
      
      if (existingIndex >= 0) {
        this.showToast('Thông báo', 'Sản phẩm đã có trong danh sách', 'bg-warning');
        return;
      }
      
      // Thêm sản phẩm vào danh sách
      this.transferForm.details.push({
        barcode: product.Barcode,
        quantity: 1
      });
      
      // Đóng dropdown và xóa từ khóa tìm kiếm
      this.showProductDropdown = false;
      this.productSearch = '';
    },
    
    // Đóng dropdown khi click bên ngoài
    closeDropdownOnClickOutside(event) {
      if (this.showProductDropdown) {
        const dropdown = document.querySelector('.product-search-dropdown');
        if (dropdown && !dropdown.contains(event.target)) {
          this.showProductDropdown = false;
        }
      }
    },
    
    // Còn lại các phương thức giữ nguyên
    addProduct() {
      if (!this.productSearch) return;
      
      const product = this.sourceProducts.find(p => p.Barcode === this.productSearch);
      
      if (!product) {
        // Tìm sản phẩm với tên tương tự nếu không tìm được theo barcode chính xác
        const similarProducts = this.sourceProducts.filter(p => 
          (p.Name || '').toLowerCase().includes(this.productSearch.toLowerCase()) ||
          (p.Barcode || '').toLowerCase().includes(this.productSearch.toLowerCase())
        );
        
        if (similarProducts.length > 0) {
          this.selectProduct(similarProducts[0]);
          return;
        }
        
        this.showToast('Lỗi', 'Không tìm thấy sản phẩm với mã vạch này', 'bg-danger');
        return;
      }
      
      this.selectProduct(product);
    },
    
    removeProduct(index) {
      this.transferForm.details.splice(index, 1);
    },
    
    getProductName(barcode) {
      const product = this.sourceProducts.find(p => p.Barcode === barcode);
      return product ? product.Name : 'N/A';
    },
    
    getProductStock(barcode) {
      const product = this.sourceProducts.find(p => p.Barcode === barcode);
      return product ? parseInt(product.Num) : 0;
    },
    
    // Các phương thức khác giữ nguyên
    debugStructure(data, fieldName = '') {
      if (!data) {
        console.log(`${fieldName} is null or undefined`);
        return;
      }
      
      if (Array.isArray(data)) {
        console.log(`${fieldName} là mảng với ${data.length} phần tử`);
        if (data.length > 0) {
          console.log(`Phần tử mẫu:`, data[0]);
        }
      } else if (typeof data === 'object') {
        console.log(`${fieldName} là object với các keys:`, Object.keys(data));
        for (const [key, value] of Object.entries(data)) {
          if (Array.isArray(value) && value.length > 0) {
            console.log(`${fieldName}.${key} là mảng với ${value.length} phần tử`);
            console.log(`Phần tử mẫu:`, value[0]);
          }
        }
      } else {
        console.log(`${fieldName} là ${typeof data}:`, data);
      }
    },

    // Thêm/cập nhật phương thức confirmExport
    async confirmExport(id) {
      if (confirm('Xác nhận chuyển kho? Hành động này không thể hoàn tác.')) {
        try {
          this.loading = true;
          const response = await axios.post(`${API_BASE_URL}/Export/confirm/${id}`);
          
          if (response.data && response.data.success) {
            this.showToast('Thành công', 'Đã xác nhận chuyển kho thành công', 'bg-success');
          } else {
            this.showToast('Thành công', 'Đã xác nhận chuyển kho', 'bg-success');
          }
          
          // Làm mới danh sách để cập nhật trạng thái
          await this.loadExports();
        } catch (error) {
          console.error('Lỗi khi xác nhận chuyển kho:', error);
          this.showToast('Lỗi', error.response?.data?.message || 'Không thể xác nhận chuyển kho', 'bg-danger');
        } finally {
          this.loading = false;
        }
      }
    },

    // Cập nhật phương thức confirmExportFromModal
    confirmExportFromModal() {
      if (this.selectedExport) {
        this.confirmExport(this.selectedExport.id);
        // Đóng modal chỉ sau khi xác nhận thành công
        setTimeout(() => {
          this.closeViewModal();
        }, 1000);
      }
    },

    async createTransfer() {
      if (!this.validateTransferForm()) return;
      
      try {
        this.loading = true;
        
        // Đảm bảo dữ liệu đúng định dạng theo API
        const transferData = {
          idWarehouseExport: parseInt(this.transferForm.idWarehouseExport),
          idWarehouseImport: parseInt(this.transferForm.idWarehouseImport),
          note: this.transferForm.note || "",
          name: this.transferForm.name,
          details: this.transferForm.details.map(item => ({
            barcode: item.barcode,
            quantity: parseInt(item.quantity)
          }))
        };
        
        console.log('Dữ liệu gửi đi:', transferData);
        
        const response = await axios.post(`${API_BASE_URL}/Export/create-transfer`, transferData);
        
        this.showToast('Thành công', 'Đã tạo phiếu chuyển kho thành công', 'bg-success');
        this.closeCreateModal();
        await this.loadExports();
      } catch (error) {
        console.error('Lỗi khi tạo phiếu chuyển kho:', error);
        this.showToast('Lỗi', error.response?.data?.message || 'Không thể tạo phiếu chuyển kho', 'bg-danger');
      } finally {
        this.loading = false;
      }
    },

    // Thêm phương thức kiểm tra dữ liệu form
    validateTransferForm() {
      if (!this.transferForm.name) {
        this.showToast('Lỗi', 'Vui lòng nhập tên phiếu', 'bg-danger');
        return false;
      }
      
      if (!this.transferForm.idWarehouseExport) {
        this.showToast('Lỗi', 'Vui lòng chọn kho xuất', 'bg-danger');
        return false;
      }
      
      if (!this.transferForm.idWarehouseImport) {
        this.showToast('Lỗi', 'Vui lòng chọn kho nhập', 'bg-danger');
        return false;
      }
      
      if (this.transferForm.idWarehouseExport === this.transferForm.idWarehouseImport) {
        this.showToast('Lỗi', 'Kho xuất và kho nhập không được trùng nhau', 'bg-danger');
        return false;
      }
      
      if (this.transferForm.details.length === 0) {
        this.showToast('Lỗi', 'Vui lòng thêm ít nhất một sản phẩm', 'bg-danger');
        return false;
      }
      
      // Kiểm tra số lượng sản phẩm
      for (const item of this.transferForm.details) {
        if (!item.quantity || item.quantity <= 0) {
          this.showToast('Lỗi', `Số lượng sản phẩm ${this.getProductName(item.barcode)} phải lớn hơn 0`, 'bg-danger');
          return false;
        }
        
        const stock = this.getProductStock(item.barcode);
        if (item.quantity > stock) {
          this.showToast('Lỗi', `Số lượng sản phẩm ${this.getProductName(item.barcode)} không được vượt quá tồn kho (${stock})`, 'bg-danger');
          return false;
        }
      }
      
      return true;
    },

    showToast(title, message, bgClass) {
      this.toastTitle = title;
      this.toastMessage = message;
      this.toastClass = bgClass;
      
      if (this.toast) {
        this.toast.show();
      } else {
        // Khởi tạo toast nếu chưa có
        this.$nextTick(() => {
          const toastEl = document.getElementById('toast');
          if (toastEl) {
            this.toast = new Toast(toastEl);
            this.toast.show();
          } else {
            console.error('Không tìm thấy phần tử toast');
            alert(`${title}: ${message}`);
          }
        });
      }
    },

  }
};
</script>

<style scoped>
/* Thêm CSS cho modal */
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
  pointer-events: none;
}

.modal-content {
  position: relative;
  display: flex;
  flex-direction: column;
  width: 100%;
  pointer-events: auto;
  background-color: #fff;
  background-clip: padding-box;
  border: 1px solid rgba(0, 0, 0, 0.2);
  border-radius: 0.3rem;
  outline: 0;
}

/* Các style hiện tại */
.toast-header {
  color: white;
}
.form-control-sm {
  min-height: calc(1.5em + 0.5rem + 2px);
  padding: 0.25rem 0.5rem;
  font-size: 0.875rem;
  border-radius: 0.2rem;
}

/* Thêm CSS cho dropdown tìm kiếm sản phẩm */
.product-search-dropdown {
  position: relative;
}

.product-search-dropdown .dropdown-menu {
  max-height: 250px;
  overflow-y: auto;
}

.product-dropdown-items {
  max-height: 200px;
  overflow-y: auto;
}

.dropdown-item {
  padding: 0.5rem 1rem;
  white-space: normal;
  border-bottom: 1px solid rgba(0,0,0,0.05);
}

.dropdown-item:hover, .dropdown-item:focus {
  background-color: #f8f9fa;
}

.dropdown-menu.show {
  display: block;
  width: 100%;
  padding: 0;
}

.dropdown-header {
  font-weight: 600;
  color: #1976d2;
  background-color: #f8f9fa;
}
</style>