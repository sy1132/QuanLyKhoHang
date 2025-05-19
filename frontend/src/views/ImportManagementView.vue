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
    <div class="d-flex justify-content-between align-items-center">
      <!-- Nút báo cáo nhập hàng -->
      <router-link to="/import/report" class="btn btn-primary">
        <i class="fas fa-chart-bar me-1"></i> Báo cáo nhập hàng
      </router-link>
      
      <!-- Nút tạo phiếu nhập mới -->
      <div>
        <ImportAdd @import-added="loadImports" />
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

    <!-- Details Modal -->
    <div v-if="showDetailsModal" class="modal fade show" tabindex="-1" style="display: block; background-color: rgba(0,0,0,0.5);">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Chi tiết phiếu nhập #{{ selectedImport.id }}</h5>
            <button type="button" class="btn-close" @click="showDetailsModal = false"></button>
          </div>
          <div class="modal-body">
            <!-- Hiển thị loading khi đang tải dữ liệu -->
            <div v-if="isLoading" class="text-center my-3">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang tải...</span>
              </div>
              <p class="mt-2">Đang tải chi tiết phiếu nhập...</p>
            </div>
            
            <div v-else-if="importDetails.length === 0" class="alert alert-info">
              <p>Phiếu nhập này không có chi tiết hoặc không thể tải được dữ liệu chi tiết.</p>
            </div>
            
            <div v-else>
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
import { importApi } from '../api/import.api';
import ImportAdd from './ImportAdd.vue';

const BASE_URL = 'https://localhost:7189/api';

export default {
  name: 'ImportManagementView',
  components: {
    ImportAdd
  },
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
        
        // Lấy dữ liệu từ API
        let response;
        
        if (this.filter.status || this.filter.fromDate || this.filter.toDate) {
          console.log('Loading filtered imports with:', this.filter);
          response = await importApi.getFilteredImports({
            status: this.filter.status,
            fromDate: this.filter.fromDate,
            toDate: this.filter.toDate
          });
        } else {
          console.log('Loading all imports');
          response = await importApi.getAllImports();
        }
        
        console.log('API response:', response);
        
        // Xử lý đa dạng cấu trúc phản hồi
        let importData = [];
        
        if (Array.isArray(response)) {
          // Nếu phản hồi là mảng trực tiếp
          importData = response;
        } else if (response && typeof response === 'object') {
          // Nếu phản hồi là object
          if (response.data && Array.isArray(response.data)) {
            importData = response.data;
          } else if (response.result && Array.isArray(response.result)) {
            importData = response.result;
          } else if (response.result && response.result.data && Array.isArray(response.result.data)) {
            importData = response.result.data;
          } else {
            // Tìm bất kỳ mảng nào trong phản hồi
            const possibleArrays = Object.values(response).filter(val => Array.isArray(val));
            importData = possibleArrays.length > 0 ? possibleArrays[0] : [];
          }
        }
        
        // Đảm bảo các trường cần thiết tồn tại
        this.imports = importData.map(item => {
          return {
            id: item.id || item.importId || item.Id,
            warehouseId: item.warehouseId || item.WarehouseId,
            dateInput: item.dateInput || item.DateInput || item.date || item.createdAt,
            cost: item.cost || item.totalCost || item.amount || 0,
            status: item.status || item.Status || 'Pending'
          };
        });
        
        console.log('Imports processed:', this.imports);
        
        if (this.imports.length === 0) {
          console.log('No imports found after processing');
        }
      } catch (error) {
        console.error('Error loading imports:', error);
        alert('Không thể tải danh sách phiếu nhập: ' + (error.message || 'Unknown error'));
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
        console.log('Fetching details for import ID:', id);
        this.isLoading = true;
        this.showDetailsModal = true; // Hiển thị modal ngay từ đầu với trạng thái loading
        
        // Thêm timeout để tránh lỗi mạng
        const timeoutPromise = new Promise((_, reject) =>
          setTimeout(() => reject(new Error('Timeout fetching import details')), 10000)
        );
        
        // Race giữa API call và timeout
        const response = await Promise.race([
          importApi.getImportDetails(id),
          timeoutPromise
        ]);
        
        console.log('Import details response:', response);
        
        // Không có phản hồi
        if (!response) {
          throw new Error('Không nhận được phản hồi từ server');
        }
        
        // Xử lý dữ liệu trả về từ API
        let importData = null;
        let detailsData = [];
        
        // Kiểm tra xem response có phải là lỗi không
        if (response.error || (response.success === false)) {
          throw new Error(response.message || 'API trả về lỗi');
        }
        
        // Truy cập trực tiếp vào các trường để tìm dữ liệu
        if (response.import && response.details) {
          importData = response.import;
          detailsData = response.details;
        } else if (response.data) {
          // API có thể trả về nested data object
          if (response.data.import && response.data.details) {
            importData = response.data.import;
            detailsData = response.data.details;
          } else if (Array.isArray(response.data)) {
            // Nếu data là mảng, có thể đây là chi tiết phiếu nhập
            detailsData = response.data;
            
            // Cố gắng lấy thông tin phiếu nhập từ API riêng
            const importResponse = await importApi.getImportById(id);
            if (importResponse && importResponse.data) {
              importData = importResponse.data;
            }
          } else {
            // Nếu data là object, có thể đây là thông tin phiếu nhập
            importData = response.data;
            
            // Kiểm tra xem object có thuộc tính details không
            if (response.data.details) {
              detailsData = response.data.details;
            }
          }
        } else if (response.id) {
          // Nếu response có id, giả sử đây là phiếu nhập
          importData = response;
          
          // Kiểm tra xem object có thuộc tính details không
          if (response.details) {
            detailsData = response.details;
          }
        }
        
        // Nếu không tìm thấy dữ liệu phiếu nhập
        if (!importData) {
          // Cố gắng dùng response làm importData
          importData = {
            id: id,
            warehouseId: response.warehouseId || null,
            dateInput: response.dateInput || response.date || new Date().toISOString(),
            cost: response.cost || response.amount || 0,
            status: response.status || 'Pending'
          };
        }
        
        // Nếu vẫn không có chi tiết, thử gọi API riêng để lấy
        if (detailsData.length === 0) {
          try {
            const detailsResponse = await axios.get(`${BASE_URL}/import/${id}/details`);
            if (detailsResponse && detailsResponse.data) {
              detailsData = Array.isArray(detailsResponse.data) ? 
                detailsResponse.data : 
                detailsResponse.data.data || detailsResponse.data.details || [];
            }
          } catch (detailsError) {
            console.error('Error fetching details separately:', detailsError);
            // Không throw lỗi, vẫn tiếp tục với phiếu nhập không có chi tiết
          }
        }
        
        // Thêm trước khi xử lý detailsData
        console.log('Raw response:', response);
        console.log('Type of response:', typeof response);
        console.log('detailsData before processing:', detailsData);
        console.log('Type of detailsData:', typeof detailsData);
        if (detailsData) {
          console.log('Is detailsData an array?', Array.isArray(detailsData));
        }
        
        // Chuẩn hóa dữ liệu phiếu nhập
        this.selectedImport = {
          id: importData.id || id,
          warehouseId: importData.warehouseId || null,
          dateInput: importData.dateInput || importData.date || new Date().toISOString(),
          cost: importData.cost || importData.amount || importData.totalCost || 0,
          status: importData.status || 'Pending'
        };
        
        // Đảm bảo detailsData là một mảng
        if (!Array.isArray(detailsData)) {
          console.error('detailsData is not an array:', detailsData);
          // Kiểm tra xem detailsData có phải là object không
          if (detailsData && typeof detailsData === 'object') {
            // Nếu là object, kiểm tra xem có thuộc tính dạng mảng không
            const possibleArrays = Object.values(detailsData).filter(val => Array.isArray(val));
            if (possibleArrays.length > 0) {
              detailsData = possibleArrays[0];
            } else {
              // Nếu không tìm thấy mảng, kiểm tra xem có phải là object đơn lẻ không
              if (!Array.isArray(detailsData) && detailsData !== null && typeof detailsData === 'object') {
                // Nếu là object đơn, chuyển thành mảng 1 phần tử
                detailsData = [detailsData];
              } else {
                // Trường hợp không thể xử lý, sử dụng mảng rỗng
                detailsData = [];
              }
            }
          } else {
            // Không phải object, sử dụng mảng rỗng
            detailsData = [];
          }
        }
        
        // Chuẩn hóa dữ liệu chi tiết - giờ đã chắc chắn detailsData là mảng
        this.importDetails = detailsData.map(detail => ({
          idProduct: detail.idProduct || detail.productId || detail.ProductId || null,
          idSupplier: detail.idSupplier || detail.supplierId || detail.SupplierId || null,
          quantity: detail.quantity || 0,
          cost: detail.cost || detail.price || 0,
          note: detail.note || ''
        }));
        
        console.log('Processed import:', this.selectedImport);
        console.log('Processed details:', this.importDetails);
      } catch (error) {
        console.error('Error in viewDetails:', error);
        this.importDetails = [];
        alert(`Không thể tải chi tiết phiếu nhập. Lỗi: ${error.message}`);
      } finally {
        this.isLoading = false;
      }
    },
    
    // Create/Edit methods
    showCreateModal() {
      console.log('Showing create modal');
      this.isEditing = false;
      
      // Đảm bảo đã tải dữ liệu cần thiết
      if (this.warehouses.length === 0) {
        this.loadWarehouses();
      }
      if (this.products.length === 0) {
        this.loadProducts();
      }
      if (this.suppliers.length === 0) {
        this.loadSuppliers();
      }
      
      // Khởi tạo phiếu nhập mới với giá trị mặc định
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
        
        if (this.isEditing) {
          importData.id = this.currentImport.id;
          await importApi.updateImport(this.currentImport.id, importData);
          alert('Cập nhật phiếu nhập thành công');
        } else {
          await importApi.createImport(importData);
          alert('Tạo phiếu nhập thành công');
        }
        
        this.showImportModal = false;
        this.loadImports(); // Tải lại danh sách phiếu nhập
      } catch (error) {
        console.error('Error saving import:', error);
        this.errorMessage = error.response?.data?.message || 
                            'Không thể lưu phiếu nhập. Vui lòng kiểm tra kết nối và thử lại.';
      } finally {
        this.isLoading = false;
      }
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