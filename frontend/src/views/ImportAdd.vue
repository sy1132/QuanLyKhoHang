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
                <label>Mã phiếu nhập:</label>
                <input type="text" v-model="currentImport.code" class="form-control" readonly>
                <small class="text-muted">Mã phiếu nhập được tạo tự động</small>
              </div>
              <div class="form-group mb-3">
                <label>Kho nhập:</label>
                <div v-if="isLoadingWarehouses" class="d-flex align-items-center mb-2">
                  <div class="spinner-border spinner-border-sm me-2" role="status"></div>
                  <span>Đang tải danh sách kho...</span>
                </div>
                
                <!-- Thêm input tìm kiếm nhanh kho -->
                <div v-if="warehouses.length > 5" class="input-group mb-2">
                  <input 
                    type="text" 
                    v-model="warehouseSearchTerm" 
                    class="form-control"
                    placeholder="Tìm kho theo tên..."
                  >
                  <button class="btn btn-outline-secondary" type="button" @click="warehouseSearchTerm = ''">
                    <i class="fas fa-times"></i>
                  </button>
                </div>
                
                <select v-model="currentImport.warehouseId" class="form-select" :disabled="isLoadingWarehouses">
                  <option v-if="filteredWarehouses.length === 0" value="" disabled>
                    {{ isLoadingWarehouses ? 'Đang tải...' : 'Không có kho nào' }}
                  </option>
                  <option v-for="warehouse in filteredWarehouses" :key="warehouse.id" :value="warehouse.id">
                    {{ warehouse.name || warehouse.warehouseName || 'Kho #' + warehouse.id }}
                  </option>
                </select>
                
                <div v-if="warehouses.length === 0 && !isLoadingWarehouses" class="text-danger mt-1">
                  <small>Không thể tải danh sách kho. Vui lòng thử lại sau.</small>
                </div>
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
                        <!-- Thay thế phần chọn sản phẩm hiện tại bằng phiên bản có tìm kiếm -->
                        <div class="d-flex flex-column">
                          <!-- Dropdown kết hợp tìm kiếm -->
                          <div class="dropdown product-search-dropdown w-100">
                            <div class="input-group">
                              <input 
                                type="text" 
                                v-model="productSearchTerms[index]" 
                                class="form-control form-control-sm" 
                                :placeholder="isLoadingProducts ? 'Đang tải...' : 'Tìm sản phẩm...'"
                                @focus="openProductDropdown(index)"
                                :disabled="isLoadingProducts"
                              >
                              <button class="btn btn-outline-secondary btn-sm dropdown-toggle" type="button" 
                                :id="`productDropdown${index}`"
                                @click="toggleProductDropdown(index)"
                                :disabled="isLoadingProducts">
                              </button>
                            </div>

                            <!-- Dropdown menu -->
                            <div class="dropdown-menu w-100" :class="{ 'show': activeProductDropdown === index }">
                              <div class="dropdown-header p-2">
                                <span v-if="filteredProducts(index).length > 0">
                                  {{ filteredProducts(index).length }} sản phẩm
                                </span>
                                <span v-else-if="isLoadingProducts">
                                  <span class="spinner-border spinner-border-sm" role="status"></span>
                                  Đang tải sản phẩm...
                                </span>
                                <span v-else>Không tìm thấy sản phẩm</span>
                              </div>
                              <div class="dropdown-divider"></div>
                              <div class="product-dropdown-items">
                                <button v-for="product in filteredProducts(index)" :key="product.id"
                                  class="dropdown-item"
                                  @click="selectProduct(index, product)">
                                  <div class="d-flex justify-content-between align-items-center">
                                    <div>
                                      <strong>{{ product.name || 'Sản phẩm #' + product.id }}</strong>
                                      <small v-if="product.barcode" class="text-muted d-block">{{ product.barcode }}</small>
                                    </div>
                                    <span class="badge bg-primary">{{ formatCurrency(product.price) }}</span>
                                  </div>
                                </button>
                              </div>
                            </div>
                          </div>

                          <!-- Hiển thị sản phẩm đã chọn -->
                          <div v-if="detail.idProduct && getProductInfo(detail.idProduct)" class="mt-1 small text-muted">
                            <strong>{{ getProductInfo(detail.idProduct).name }}</strong><br>
                            Barcode: {{ getProductInfo(detail.idProduct).barcode || 'N/A' }} | 
                            Đơn vị: {{ getProductInfo(detail.idProduct).unit || 'Cái' }}
                          </div>
                          
                          <!-- Nút làm mới -->
                          <div class="mt-1 d-flex">
                            <button type="button" class="btn btn-outline-info btn-sm" @click="refreshProducts" title="Làm mới danh sách">
                              <i class="fas fa-sync-alt"></i> Làm mới
                            </button>
                          </div>
                        </div>
                      </td>
                      <td>
                        <select v-model="detail.idSupplier" class="form-select form-select-sm">
                          <option v-if="suppliers.length === 0" value="" disabled>Đang tải nhà cung cấp...</option>
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
      isLoadingWarehouses: false,
      errorMessage: '',
      
      // Dữ liệu danh mục
      warehouses: [],
      products: [],
      suppliers: [],
      
      // Dữ liệu phiếu nhập mới
      currentImport: {
        warehouseId: null,
        code: '', // Thêm trường code
        details: []
      },
      
      warehouseSearchTerm: '', // Biến đã được thêm tại đây
      importCodes: [], // Lưu danh sách mã phiếu nhập để tạo mã mới
      isLoadingProducts: false,
      productSearchTerms: [], // Thêm biến này để lưu trữ các từ khóa tìm kiếm sản phẩm
      activeProductDropdown: null // Thêm biến này để theo dõi dropdown sản phẩm đang mở
    };
  },
  
  computed: {
    filteredWarehouses() {
      if (!this.warehouseSearchTerm) {
        return this.warehouses;
      }
      
      const searchTerm = this.warehouseSearchTerm.toLowerCase();
      return this.warehouses.filter(warehouse => {
        const name = (warehouse.name || warehouse.warehouseName || '').toLowerCase();
        return name.includes(searchTerm);
      });
    },
    // Lọc sản phẩm theo từ khóa tìm kiếm
    filteredProducts() {
      return (index) => {
        const searchTerm = (this.productSearchTerms[index] || '').toLowerCase();
        
        if (!searchTerm) {
          return this.products.slice(0, 100); // Giới hạn 100 sản phẩm nếu không tìm kiếm
        }
        
        return this.products.filter(product => {
          const name = (product.name || '').toLowerCase();
          const barcode = (product.barcode || '').toLowerCase();
          const id = String(product.id).toLowerCase();
          
          return name.includes(searchTerm) || 
                 barcode.includes(searchTerm) || 
                 id.includes(searchTerm);
        });
      };
    }
  },
  
  methods: {
    // Sửa phương thức showModal để đảm bảo tải dữ liệu sản phẩm
    showModal() {
      this.showImportModal = true;
      
      // Khởi tạo phiếu nhập mới với giá trị mặc định
      this.currentImport = {
        warehouseId: this.warehouses.length > 0 ? this.warehouses[0].id : null,
        code: '', // Mã sẽ được tạo tự động trong loadData()
        details: [this.createEmptyDetail()]
      };
      
      // Khởi tạo mảng từ khóa tìm kiếm
      this.productSearchTerms = [''];
      this.activeProductDropdown = -1;
      
      // Tải dữ liệu và đảm bảo có danh sách sản phẩm
      this.loadData().then(() => {
        // Nếu không có sản phẩm từ API, tải dữ liệu thủ công
        if (this.products.length === 0) {
          this.loadProductsDirectly();
        }
      });
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
        
        // Lấy mã phiếu nhập cao nhất để tạo mã mới
        await this.generateNextImportCode();
        
      } catch (error) {
        console.error('Error loading data:', error);
        this.errorMessage = 'Không thể tải dữ liệu. Vui lòng thử lại sau.';
      } finally {
        this.isLoading = false;
      }
    },
    
    async loadWarehouses() {
      this.isLoadingWarehouses = true;
      try {
        console.log('Loading warehouses...');
        
        const response = await axios.get(`${BASE_URL}/warehouse`);
        console.log('Warehouse API response:', response);
        
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
            const possibleArrays = Object.values(response.data).filter(val => Array.isArray(val));
            warehouseData = possibleArrays.length > 0 ? possibleArrays[0] : [];
          }
          
          // Đảm bảo mỗi warehouse có thuộc tính name
          this.warehouses = warehouseData.map(warehouse => {
            // Nếu không có thuộc tính name, sử dụng thuộc tính thay thế hoặc ID
            if (!warehouse.name && (warehouse.warehouseName || warehouse.title)) {
              warehouse.name = warehouse.warehouseName || warehouse.title;
            }
            return warehouse;
          });
          
          console.log('Warehouses loaded:', this.warehouses);
          
          if (this.warehouses.length > 0 && !this.currentImport.warehouseId) {
            this.currentImport.warehouseId = this.warehouses[0].id;
            
            if (this.showProductModal) {
              this.newProduct.warehouseId = this.warehouses[0].id;
            }
          }
        } else {
          console.error('Invalid warehouse data format:', response);
          this.warehouses = [];
        }
      } catch (error) {
        console.error('Error loading warehouses:', error);
        this.warehouses = [];
        
        if (!this.currentImport.warehouseId) {
          this.currentImport.warehouseId = this.warehouses[0].id;
        }
      } finally {
        this.isLoadingWarehouses = false;
      }
    },
    
    async loadProducts() {
      this.isLoadingProducts = true;
      try {
        console.log('Loading products...');
        
        // Thay đổi endpoint để khớp với ProductIndex.vue
        const response = await axios.get(`${BASE_URL}/Products/list`);
        console.log('Product API response:', response);
        
        // Áp dụng cách xử lý từ ProductIndex.vue
        if (Array.isArray(response.data)) {
          this.products = response.data.map(item => ({
            id: item.id || item.Id || item.productId || item.ProductId,
            name: item.name || item.Name || item.productName || item.ProductName || 'Không có tên',
            price: item.price || (item.cost ? item.cost * 1.2 : 0),
            barcode: item.barcode || item.Barcode || '',
            unit: item.unit || item.Unit || 'Cái',
            status: item.status || item.Status || '',
            warehouseId: item.warehouseId || item.WarehouseId,
            cost: item.cost || item.Cost || 0,
            brand: item.brand || item.Brand || '',
            categoryId: item.categoryId || item.CategoryId || item.categoryID || item.CategoryID
          }));
          console.log('Products loaded from direct array:', this.products);
        }
        // Nếu trả về dạng { result: { data: [...] } }
        else if (response.data && response.data.result && Array.isArray(response.data.result.data)) {
          this.products = response.data.result.data.map(item => ({
            id: item.id || item.Id || item.productId || item.ProductId,
            name: item.name || item.Name || item.productName || item.ProductName || 'Không có tên',
            price: item.price || (item.cost ? item.cost * 1.2 : 0),
            barcode: item.barcode || item.Barcode || '',
            unit: item.unit || item.Unit || 'Cái',
            status: item.status || item.Status || '',
            warehouseId: item.warehouseId || item.WarehouseId,
            cost: item.cost || item.Cost || 0,
            brand: item.brand || item.Brand || '',
            categoryId: item.categoryId || item.CategoryId || item.categoryID || item.CategoryID
          }));
          console.log('Products loaded from result.data:', this.products);
        }
        // Giữ lại xử lý kiểu dữ liệu cũ để đảm bảo tương thích ngược
        else if (response.data && (response.data.data || response.data.result?.data)) {
          const productData = response.data.data || response.data.result.data;
          
          this.products = productData.map(item => ({
            id: item.id || item.Id || item.productId || item.ProductId,
            name: item.name || item.Name || item.productName || item.ProductName || 'Không có tên',
            price: item.price || (item.cost ? item.cost * 1.2 : 0),
            barcode: item.barcode || item.Barcode || '',
            unit: item.unit || item.Unit || 'Cái',
            status: item.status || item.Status || '',
            warehouseId: item.warehouseId || item.WarehouseId,
            cost: item.cost || item.Cost || 0,
            brand: item.brand || item.Brand || '',
            categoryId: item.categoryId || item.CategoryId || item.categoryID || item.CategoryID
          }));
          
          console.log('Products loaded from data structure:', this.products);
        } else {
          console.error('Invalid product data format:', response.data);
          this.products = [];
          
          // Thử phương thức loadProductsDirectly nếu không thấy dữ liệu
          await this.loadProductsDirectly();
        }
      } catch (error) {
        console.error('Error loading products:', error);
        this.products = [];
        
        // Nếu có lỗi, thử tải sản phẩm bằng phương thức trực tiếp
        try {
          await this.loadProductsDirectly();
        } catch (directError) {
          console.error('Also failed with direct loading:', directError);
        }
      } finally {
        this.isLoadingProducts = false;
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
      this.productSearchTerms.push(''); // Thêm từ khóa tìm kiếm trống cho chi tiết mới
    },
    
    removeDetail(index) {
      if (this.currentImport.details.length > 1) {
        this.currentImport.details.splice(index, 1);
        this.productSearchTerms.splice(index, 1); // Xóa từ khóa tìm kiếm tương ứng
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
          code: this.currentImport.code, // Thêm mã phiếu nhập
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
    },

    async generateNextImportCode() {
      try {
        // Lấy danh sách phiếu nhập để xác định mã cao nhất
        const response = await axios.get(`${BASE_URL}/import/codes`);
        let codes = [];
        
        // Xử lý phản hồi dựa trên cấu trúc API
        if (response.data) {
          if (Array.isArray(response.data)) {
            codes = response.data;
          } else if (response.data.data && Array.isArray(response.data.data)) {
            codes = response.data.data;
          } else if (response.data.result && response.data.result.data) {
            codes = response.data.result.data;
          }
        }
        
        // Lưu danh sách mã
        this.importCodes = codes;
        
        // Tạo mã mới
        let maxCode = 0;
        if (codes.length > 0) {
          // Lọc các mã số
          const numericCodes = codes
            .filter(code => code && /^PN\d+$/.test(code))
            .map(code => parseInt(code.replace('PN', ''), 10))
            .filter(num => !isNaN(num));
          
          if (numericCodes.length > 0) {
            maxCode = Math.max(...numericCodes);
          }
        }
        
        // Tạo mã mới với định dạng PN + số (ví dụ: PN0001)
        const nextCode = maxCode + 1;
        this.currentImport.code = `PN${nextCode.toString().padStart(4, '0')}`;
        console.log('Generated new import code:', this.currentImport.code);
        
      } catch (error) {
        console.error('Error generating import code:', error);
        // Nếu không thể lấy mã từ API, tạo mã với timestamp
        const timestamp = new Date().getTime();
        this.currentImport.code = `PN${timestamp.toString().substring(timestamp.toString().length - 8)}`;
      }
    },

    // Thêm phương thức này vào methods
    async refreshProducts() {
      try {
        this.errorMessage = '';
        this.isLoadingProducts = true;
        
        // Thử tải từ API trước
        await this.loadProducts();
        
        // Nếu không có sản phẩm nào được tải, sử dụng dữ liệu thủ công
        if (this.products.length === 0) {
          await this.loadProductsDirectly();
        }
        
        return true;
      } catch (error) {
        console.error('Error refreshing products:', error);
        
        // Nếu cả hai cách đều thất bại, hiển thị thông báo
        this.errorMessage = 'Không thể tải danh sách sản phẩm';
        
        // Thử tải dữ liệu thủ công
        await this.loadProductsDirectly();
        
        return false;
      } finally {
        this.isLoadingProducts = false;
      }
    },

    getProductInfo(productId) {
      if (!productId) return null;
      const product = this.products.find(p => p.id === productId);
      return product || null;
    },

    // Thêm hàm mới vào methods để tải thủ công dữ liệu sản phẩm từ database
    async loadProductsDirectly() {
      try {
        console.log('Loading products directly from database...');
        
        // Sử dụng đúng endpoint như trong ProductIndex.vue
        const response = await axios.get(`${BASE_URL}/Products/list`, {
          params: {
            includeInactive: true
          }
        });
        
        // In ra cấu trúc JSON để debug
        console.log('API response structure:', JSON.stringify(response.data).substring(0, 100) + '...');
        
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
          
          // Map dữ liệu với logic tính giá từ ProductIndex.vue
          this.products = products.map(item => ({
            id: item.id ?? item.Id ?? item.productId ?? item.ProductId ?? "",
            name: item.name ?? item.Name ?? item.productName ?? item.ProductName ?? 'Không có tên',
            price: item.price ?? (item.cost ? item.cost * 1.2 : 0),
            barcode: item.barcode ?? item.Barcode ?? '',
            unit: item.unit ?? item.Unit ?? 'Cái',
            status: item.status ?? item.Status ?? '',
            warehouseId: item.warehouseId ?? item.WarehouseId ?? "",
            cost: item.cost ?? item.Cost ?? 0,
            brand: item.brand ?? item.Brand ?? '',
            categoryId: item.categoryId ?? item.CategoryId ?? item.categoryID ?? item.CategoryID
          }));
          
          console.log('Processed products from database:', this.products);
        }
        return true;
      } catch (error) {
        console.error('Error loading products directly from database:', error);
        return false;
      }
    },

    // Thêm phương thức mới
    async forceRefreshProducts() {
      this.products = []; // Xóa danh sách sản phẩm hiện tại
      this.isLoadingProducts = true;
      try {
        await this.loadProductsDirectly(); // Gọi trực tiếp phương thức tải sản phẩm
        alert('Đã làm mới danh sách sản phẩm');
      } catch (error) {
        console.error('Error refreshing products:', error);
        alert('Không thể làm mới danh sách sản phẩm');
      } finally {
        this.isLoadingProducts = false;
      }
    },

    // Thêm phương thức này vào methods
    async debugApiConnection() {
      try {
        // Thử gọi API với cả hai kiểu endpoint
        const responses = [];
        
        try {
          const resp1 = await axios.get(`${BASE_URL}/products`);
          responses.push({ 
            url: `${BASE_URL}/products`, 
            success: true, 
            status: resp1.status,
            data: JSON.stringify(resp1.data).substring(0, 100) + '...'
          });
        } catch (err) {
          responses.push({ 
            url: `${BASE_URL}/products`, 
            success: false, 
            status: err.response?.status,
            error: err.message
          });
        }
        
        try {
          const resp2 = await axios.get(`${BASE_URL}/Products`);
          responses.push({ 
            url: `${BASE_URL}/Products`, 
            success: true, 
            status: resp2.status,
            data: JSON.stringify(resp2.data).substring(0, 100) + '...'
          });
        } catch (err) {
          responses.push({ 
            url: `${BASE_URL}/Products`, 
            success: false, 
            status: err.response?.status,
            error: err.message
          });
        }
        
        console.log('API Debug Results:', responses);
        alert('Kết quả kiểm tra API đã được in trong console. Mở F12 để xem chi tiết.');
      } catch (error) {
        console.error('Debug error:', error);
        alert(`Lỗi kiểm tra: ${error.message}`);
      }
    },

    // Thêm các phương thức mới để xử lý dropdown sản phẩm
    openProductDropdown(index) {
      this.activeProductDropdown = index;
    },
    toggleProductDropdown(index) {
      this.activeProductDropdown = this.activeProductDropdown === index ? null : index;
    },
    filteredProducts(index) {
      const searchTerm = this.productSearchTerms[index]?.toLowerCase() || '';
      return this.products.filter(product => {
        const name = product.name?.toLowerCase() || '';
        const barcode = product.barcode?.toLowerCase() || '';
        return name.includes(searchTerm) || barcode.includes(searchTerm);
      });
    },
    selectProduct(index, product) {
      this.currentImport.details[index].idProduct = product.id;
      this.activeProductDropdown = null;
      
      // Gán từ khóa tìm kiếm là tên sản phẩm đã chọn để hiển thị
      this.productSearchTerms[index] = product.name;
    },

    // Đóng dropdown khi click ra ngoài
    closeDropdownOnClickOutside(event) {
      if (this.activeProductDropdown >= 0) {
        const dropdownElement = document.querySelector(`.product-search-dropdown:nth-child(${this.activeProductDropdown + 1})`);
        if (dropdownElement && !dropdownElement.contains(event.target)) {
          this.activeProductDropdown = -1;
        }
      }
    }
  },

  mounted() {
    // Thêm event listener để đóng dropdown khi click ra ngoài
    document.addEventListener('click', this.closeDropdownOnClickOutside);
  },

  beforeUnmount() {
    // Loại bỏ event listener khi component bị hủy
    document.removeEventListener('click', this.closeDropdownOnClickOutside);
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
  color: #000;
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

/* Thêm CSS cho dropdown tìm kiếm sản phẩm */
.product-search-dropdown .dropdown-menu {
  max-height: 200px;
  overflow-y: auto;
}

.product-search-dropdown .product-dropdown-items {
  max-height: 150px;
  overflow-y: auto;
}

/* Thêm CSS vào phần <style> */
.product-search-dropdown {
  position: relative;
}

.product-dropdown-items {
  max-height: 250px;
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
  width: 100%;
  padding: 0;
}

.dropdown-header {
  font-weight: 600;
  color: #1976d2;
  background-color: #f8f9fa;
}

/* Thêm vào phần <style> để cập nhật cỡ chữ cho ô tìm kiếm */
/* Điều chỉnh font cho ô tìm kiếm sản phẩm */
.product-search-dropdown input.form-control {
  font-size: 14px !important; /* Tăng cỡ chữ */
  font-weight: 500; /* Đậm hơn một chút */
  color: #333; /* Màu tối hơn để dễ đọc */
  padding: 8px 12px; /* Tăng padding để ô input rộng hơn */
  height: auto; /* Đảm bảo chiều cao phù hợp với padding */
}

/* Điều chỉnh font cho ô tìm kiếm kho */
.input-group input[v-model="warehouseSearchTerm"] {
  font-size: 14px !important; 
  font-weight: 500;
  color: #333;
}

/* Điều chỉnh font cho placeholder */
.form-control::placeholder {
  font-size: 14px;
  color: #6c757d;
  opacity: 0.8;
}

/* Đảm bảo văn bản nhập vào hiển thị đầy đủ */
.dropdown-toggle {
  min-width: 30px;
}

/* Điều chỉnh text trong dropdown item */
.dropdown-item strong {
  font-size: 14px !important;
  color: #333;
}

.dropdown-item small {
  font-size: 13px !important;
  color: #666;
}

/* Cải thiện hiển thị sản phẩm đã chọn */
.small.text-muted strong {
  font-size: 13px;
  color: #0d6efd;
}

/* Cải thiện thẻ badge giá */
.badge.bg-primary {
  font-size: 12px;
  padding: 4px 8px;
}

/* Đảm bảo dropdown menu hiển thị đầy đủ */
.dropdown-menu.show {
  min-width: 100%;
  max-width: 350px; /* Giới hạn chiều rộng tối đa */
}

/* Điều chỉnh màu chữ toàn bộ form */
.modal-content {
  color: #000;
}

/* Điều chỉnh màu cho label và text */
label {
  color: #000; 
  font-weight: 500;
}

/* Điều chỉnh font cho các input */
.form-control, .form-select, input, textarea, select {
  color: #000;
  font-weight: 500;
}

/* Điều chỉnh font cho ô tìm kiếm sản phẩm */
.product-search-dropdown input.form-control {
  font-size: 14px !important;
  font-weight: 500; 
  color: #000 !important;
  padding: 8px 12px;
  height: auto;
}

/* Điều chỉnh font cho ô tìm kiếm kho */
.input-group input[v-model="warehouseSearchTerm"] {
  font-size: 14px !important; 
  font-weight: 500;
  color: #000;
}

/* Điều chỉnh font cho placeholder */
.form-control::placeholder {
  font-size: 14px;
  color: #555;
  opacity: 0.8;
}

/* Điều chỉnh text trong dropdown item */
.dropdown-item strong {
  font-size: 14px !important;
  color: #000;
}

.dropdown-item small {
  font-size: 13px !important;
  color: #333;
}

/* Cải thiện hiển thị sản phẩm đã chọn */
.small.text-muted {
  color: #333 !important;
}

.small.text-muted strong {
  font-size: 13px;
  color: #000;
}

/* Điều chỉnh màu cho bảng */
.table th, .table td {
  color: #000;
}

/* Điều chỉnh màu cho modal header và title */
.modal-title {
  color: #000;
  font-weight: 600;
}

/* Đảm bảo text trong button đủ đậm và đen */
.btn {
  color: #fff;
  font-weight: 500;
}

.btn-outline-secondary, .btn-outline-info {
  color: #000;
}

/* Đảm bảo dropdown và các phần khác hiển thị màu đen */
.dropdown-header {
  color: #000;
  font-weight: 600;
}

/* Các text mô tả và giải thích */
.text-muted {
  color: #333 !important;
}

small {
  color: #333;
}

/* Modal styling */
.modal-content {
  color: var(--text-dark, #000);
}

/* Form elements */
input, select, textarea {
  border: 1.5px solid var(--border-medium, #dfe6e9);
  background: var(--bg-light, #f8fafc);
  color: var(--text-dark, #000);
}

input:focus, select:focus, textarea:focus {
  border-color: var(--primary-color, #3498db);
  box-shadow: 0 0 0 0.2rem rgba(52, 152, 219, 0.25);
  outline: none;
}

/* Labels */
label {
  color: var(--text-dark, #000);
  font-weight: 500;
}

/* Table headers */
.table-light th {
  background-color: var(--bg-light, #f8f9fa);
}

/* Dropdown items */
.dropdown-item strong {
  color: var(--text-dark, #000);
}

.dropdown-item small {
  color: var(--text-medium, #333);
}

.dropdown-item:hover {
  background-color: var(--primary-light, #e3f2fd);
}
</style>