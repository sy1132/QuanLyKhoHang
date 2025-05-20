<template>
  <div class="container mt-4">
    <h1 class="title-black mb-3">Báo cáo nhập hàng</h1>

    <!-- Filter section -->
    <div class="card mb-4">
      <div class="card-header py-2 px-3">
        <strong>Bộ lọc báo cáo</strong>
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
            <input type="date" v-model="filter.fromDate" class="form-control">
          </div>
          <div class="col-md-3">
            <label>Đến ngày:</label>
            <input type="date" v-model="filter.toDate" class="form-control">
          </div>
          <div class="col-md-3">
            <label>Kho:</label>
            <select v-model="filter.warehouseId" class="form-select">
              <option value="">Tất cả</option>
              <option v-for="warehouse in warehouses" :key="warehouse.id" :value="warehouse.id">
                {{ warehouse.name || 'Kho #' + warehouse.id }}
              </option>
            </select>
          </div>
          <div class="col-md-12 mt-3 d-flex gap-2">
            <button @click="applyFilters" class="btn btn-primary">
              <i class="fas fa-search"></i> Tìm kiếm
            </button>
            <button @click="resetFilters" class="btn btn-secondary">
              <i class="fas fa-redo"></i> Đặt lại
            </button>
            <button @click="exportToExcel" class="btn btn-success">
              <i class="fas fa-file-excel"></i> Xuất Excel
            </button>
            <button @click="exportToPdf" class="btn btn-danger">
              <i class="fas fa-file-pdf"></i> Xuất PDF
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Report Summary -->
    <div class="row mb-4">
      <div class="col-md-3">
        <div class="card bg-primary text-white">
          <div class="card-body">
            <h5 class="card-title">Tổng số phiếu nhập</h5>
            <p class="card-text display-6">{{ reportSummary.totalImports }}</p>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-success text-white">
          <div class="card-body">
            <h5 class="card-title">Tổng giá trị nhập</h5>
            <p class="card-text display-6">{{ formatCurrency(reportSummary.totalValue) }}</p>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-info text-white">
          <div class="card-body">
            <h5 class="card-title">Số lượng sản phẩm</h5>
            <p class="card-text display-6">{{ reportSummary.totalQuantity }}</p>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-warning text-white">
          <div class="card-body">
            <h5 class="card-title">Số nhà cung cấp</h5>
            <p class="card-text display-6">{{ reportSummary.supplierCount }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading indicator -->
    <div v-if="isLoading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
      <p class="mt-2">Đang tải dữ liệu báo cáo...</p>
    </div>

    <!-- Error message -->
    <div v-if="errorMessage && !isLoading" class="alert alert-danger">
      <strong>Lỗi:</strong> {{ errorMessage }}
      <div class="mt-2">
        <button @click="loadReportData" class="btn btn-sm btn-outline-primary">Thử lại</button>
        <button @click="loadLocalData" class="btn btn-sm btn-outline-secondary ms-2">Dùng dữ liệu mẫu</button>
      </div>
    </div>

    <!-- Empty message -->
    <div v-else-if="imports.length === 0 && !isLoading" class="alert alert-info">
      <p>Không có dữ liệu phù hợp với bộ lọc đã chọn.</p>
    </div>

    <!-- Report Data Table -->
    <div v-else>
      <h3 class="mb-3">Chi tiết nhập hàng</h3>
      
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
            <tr v-for="importItem in imports" :key="importItem.id">
              <td>{{ importItem.id }}</td>
              <td>{{ getWarehouseName(importItem.warehouseId) }}</td>
              <td>{{ formatDate(importItem.dateInput) }}</td>
              <td>{{ formatCurrency(importItem.cost) }}</td>
              <td>
                <span :class="getStatusBadgeClass(importItem.status)">
                  {{ getStatusText(importItem.status) }}
                </span>
              </td>
              <td>
                <button @click="viewDetails(importItem.id)" class="btn btn-info btn-sm me-1" title="Xem chi tiết">
                  <i class="fas fa-eye"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal Chi tiết phiếu nhập -->
    <div v-if="showDetailModal" class="modal fade show" tabindex="-1" style="display: block; background-color: rgba(0,0,0,0.5);">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title text-dark">Chi tiết phiếu nhập #{{ selectedImport.id }}</h5>
            <button type="button" class="btn-close" @click="closeDetailModal"></button>
          </div>
          <div class="modal-body">
            <!-- Hiển thị loading khi đang tải dữ liệu -->
            <div v-if="isLoadingDetail" class="text-center my-3">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Đang tải...</span>
              </div>
              <p class="mt-2">Đang tải chi tiết phiếu nhập...</p>
            </div>
            
            <div v-else-if="detailError" class="alert alert-danger">
              {{ detailError }}
            </div>
            
            <div v-else-if="importDetails.length === 0" class="alert alert-info">
              <p>Phiếu nhập này không có chi tiết hoặc không thể tải được dữ liệu chi tiết.</p>
            </div>
            
            <div v-else>
              <!-- Thông tin phiếu nhập -->
              <div class="row mb-3">
                <div class="col-md-6">
                  <p><strong>Mã phiếu:</strong> {{ selectedImport.code || selectedImport.id }}</p>
                  <p><strong>Kho nhập:</strong> {{ getWarehouseName(selectedImport.warehouseId) }}</p>
                </div>
                <div class="col-md-6">
                  <p><strong>Ngày nhập:</strong> {{ formatDate(selectedImport.dateInput) }}</p>
                  <p>
                    <strong>Trạng thái:</strong>
                    <span :class="getStatusBadgeClass(selectedImport.status)">
                      {{ getStatusText(selectedImport.status) }}
                    </span>
                  </p>
                </div>
              </div>
              
              <!-- Danh sách sản phẩm -->
              <h5 class="mt-4">Danh sách sản phẩm</h5>
              <div class="table-responsive">
                <table class="table table-bordered table-striped">
                  <thead class="table-light">
                    <tr>
                      <th>STT</th>
                      <th>Sản phẩm</th>
                      <th>Số lượng</th>
                      <th>Đơn giá</th>
                      <th>Thành tiền</th>
                      <th>Nhà cung cấp</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(detail, index) in importDetails" :key="index">
                      <td class="text-center">{{ index + 1 }}</td>
                      <td>
                        <div class="d-flex flex-column">
                          <strong>{{ detail.productName || 'Sản phẩm #' + detail.idProduct }}</strong>
                          <small v-if="detail.barcode" class="text-muted">{{ detail.barcode }}</small>
                        </div>
                      </td>
                      <td class="text-end">{{ detail.quantity }}</td>
                      <td class="text-end">{{ formatCurrency(detail.cost) }}</td>
                      <td class="text-end">{{ formatCurrency(detail.quantity * detail.cost) }}</td>
                      <td>{{ detail.supplierName || 'Không xác định' }}</td>
                    </tr>
                  </tbody>
                  <tfoot class="table-light">
                    <tr>
                      <td colspan="2" class="text-end"><strong>Tổng cộng:</strong></td>
                      <td class="text-end"><strong>{{ calculateTotalQuantity() }}</strong></td>
                      <td></td>
                      <td class="text-end text-primary"><strong>{{ formatCurrency(calculateTotalAmount()) }}</strong></td>
                      <td></td>
                    </tr>
                  </tfoot>
                </table>
              </div>
              
              <!-- Ghi chú phiếu nhập -->
              <div v-if="selectedImport.note" class="mt-4">
                <h5>Ghi chú</h5>
                <div class="p-3 bg-light rounded">
                  {{ selectedImport.note }}
                </div>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-primary" @click="printDetail">
              <i class="fas fa-print"></i> In phiếu
            </button>
            <button type="button" class="btn btn-secondary" @click="closeDetailModal">
              Đóng
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
// Import các thư viện cần thiết
import axios from 'axios';
import { importApi } from '../api/import.api';
// Các thư viện xuất báo cáo sẽ được import động khi cần

const BASE_URL = 'https://localhost:7189/api';

export default {
  name: 'ImportReportView',
  data() {
    return {
      // Filters
      filter: {
        status: '',
        fromDate: '',
        toDate: '',
        warehouseId: ''
      },
      
      // Data
      imports: [],
      warehouses: [],
      products: [],
      
      // Report stats
      reportSummary: {
        totalImports: 0,
        totalValue: 0,
        totalQuantity: 0,
        supplierCount: 0
      },
      
      // UI states
      isLoading: false,
      errorMessage: '',
      
      // Fallback data for development
      sampleData: {
        imports: [
          { id: 1, warehouseId: 1, dateInput: '2025-05-18', cost: 375, status: 'Pending' },
          { id: 2, warehouseId: 1, dateInput: '2025-05-19', cost: 220000, status: 'Pending' }
        ],
        summary: {
          totalImports: 2,
          totalValue: 220375,
          totalQuantity: 26,
          supplierCount: 1
        }
      },

      // Modal states
      showDetailModal: false,
      selectedImport: null,
      importDetails: [],
      isLoadingDetail: false,
      detailError: ''
    };
  },
  
  mounted() {
    // Thay đổi ngày mặc định để bao gồm cả dữ liệu hiện tại (tháng 5, 2025)
    const now = new Date();
    const firstDay = new Date(2025, 4, 1); // Tháng 5/2025
    const lastDay = new Date(2025, 4, 31); // Tháng 5/2025
    
    this.filter.fromDate = firstDay.toISOString().split('T')[0];
    this.filter.toDate = lastDay.toISOString().split('T')[0];
    
    this.loadWarehouses();
    this.loadReportData();
  },
  
  methods: {
    initDefaultDates() {
      // Mặc định là tháng hiện tại
      const now = new Date();
      const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
      const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      
      this.filter.fromDate = firstDay.toISOString().split('T')[0];
      this.filter.toDate = lastDay.toISOString().split('T')[0];
    },
    
    async loadReportData() {
      this.isLoading = true;
      this.errorMessage = '';
      
      try {
        console.log('Loading report with filter:', this.filter);
        
        // Nếu không có dữ liệu trong khoảng thời gian, thử mở rộng phạm vi
        const reportData = await importApi.getImportReport(this.filter);
        
        if (reportData && reportData.imports && reportData.imports.length > 0) {
          this.imports = reportData.imports;
          this.reportSummary = reportData.summary;
        } else {
          console.log('No data from API, trying with all data');
          // Thử lại với bộ lọc trống
          const allData = await importApi.getImportReport({});
          
          if (allData && allData.imports && allData.imports.length > 0) {
            this.imports = allData.imports;
            this.reportSummary = allData.summary;
            this.errorMessage = 'Đã hiển thị tất cả dữ liệu do không có dữ liệu trong khoảng lọc đã chọn.';
          } else {
            // Sử dụng dữ liệu mẫu nếu cả hai cách trên đều không thành công
            this.loadLocalData();
            this.errorMessage = 'Không thể tải dữ liệu từ API. Hiển thị dữ liệu mẫu.';
          }
        }
      } catch (error) {
        console.error('Error loading report data:', error);
        this.loadLocalData();
        this.errorMessage = 'Lỗi khi tải dữ liệu: ' + (error.message || 'Không xác định');
      } finally {
        this.isLoading = false;
      }
    },
    
    loadLocalData() {
      console.log('Loading sample data based on database');
      this.imports = this.sampleData.imports;
      this.reportSummary = this.sampleData.summary;
      this.errorMessage = '';
    },
    
    async loadWarehouses() {
      try {
        const response = await axios.get(`${BASE_URL}/warehouse`);
        
        if (response.data) {
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
          
          this.warehouses = warehouseData.map(warehouse => {
            if (!warehouse.name && (warehouse.warehouseName || warehouse.title)) {
              warehouse.name = warehouse.warehouseName || warehouse.title;
            }
            return warehouse;
          });
        }
      } catch (error) {
        console.error('Error loading warehouses:', error);
        this.warehouses = [
          { id: 1, name: 'Kho mặc định' },
          { id: 2, name: 'Kho phụ' }
        ];
      }
    },
    
    applyFilters() {
      this.loadReportData();
    },
    
    resetFilters() {
      this.initDefaultDates();
      this.filter.status = '';
      this.filter.warehouseId = '';
      this.loadReportData();
    },
    
    async viewDetails(id) {
      this.isLoadingDetail = true;
      this.detailError = '';
      this.selectedImport = this.imports.find(imp => imp.id === id) || { id };
      this.showDetailModal = true;
      
      try {
        // Sử dụng axios thay vì importApi
        const response = await axios.get(`${BASE_URL}/import/detail/${id}`);
        
        console.log('Import details response:', response);
        
        // Không có phản hồi
        if (!response) {
          throw new Error('Không nhận được phản hồi từ server');
        }
        
        // Xử lý dữ liệu trả về từ API - tương tự ImportManagementView
        let importData = null;
        let detailsData = [];
        
        // Kiểm tra xem response có phải là lỗi không
        if (response.data.error || (response.data.success === false)) {
          throw new Error(response.data.message || 'API trả về lỗi');
        }
        
        // Truy cập dữ liệu phụ thuộc vào cấu trúc API
        if (response.data.import && response.data.details) {
          importData = response.data.import;
          detailsData = response.data.details;
        } else if (response.data) {
          if (response.data.import && response.data.details) {
            importData = response.data.import;
            detailsData = response.data.details;
          } else if (Array.isArray(response.data)) {
            detailsData = response.data;
          } else if (response.data.data) {
            // Thử thêm một trường hợp nữa
            if (Array.isArray(response.data.data)) {
              detailsData = response.data.data;
            } else if (response.data.data.details) {
              detailsData = response.data.data.details;
            }
          }
        } else if (Array.isArray(response)) {
          detailsData = response;
        }
        
        // Cập nhật thông tin phiếu nhập nếu có dữ liệu
        if (importData) {
          this.selectedImport = {
            ...this.selectedImport,
            ...importData
          };
        }
        
        // Xử lý dữ liệu chi tiết phiếu nhập
        if (Array.isArray(detailsData) && detailsData.length > 0) {
          this.importDetails = detailsData.map(detail => ({
            idProduct: detail.idProduct || detail.productId,
            productName: detail.productName,
            quantity: detail.quantity,
            cost: detail.cost || detail.price,
            supplierName: detail.supplierName,
            barcode: detail.barcode
          }));
        } else {
          this.importDetails = []; // Không hiển thị lỗi, chỉ hiển thị thông báo trống
          this.detailError = 'Phiếu nhập này không có chi tiết sản phẩm';
        }
      } catch (error) {
        console.error('Error loading import details:', error);
        
        // Tạo dữ liệu mẫu để hiển thị thay vì hiện lỗi
        this.importDetails = [
          { 
            idProduct: 1, 
            productName: 'Sản phẩm mẫu', 
            quantity: 5, 
            cost: 50000,
            supplierName: 'Nhà cung cấp mẫu'
          }
        ];

        // Thông báo thân thiện hơn
        this.detailError = 'Đang hiển thị dữ liệu mẫu do không thể kết nối đến server. Vui lòng thử lại sau.';
      } finally {
        this.isLoadingDetail = false;
      }
    },
    
    closeDetailModal() {
      this.showDetailModal = false;
      this.selectedImport = {};
      this.importDetails = [];
    },
    
    async exportToExcel() {
      try {
        this.isLoading = true;
        
        // Thử sử dụng API trước
        try {
          const blob = await importApi.exportToExcel({
            status: this.filter.status,
            fromDate: this.filter.fromDate,
            toDate: this.filter.toDate,
            warehouseId: this.filter.warehouseId
          });
          
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.setAttribute('download', `bao-cao-nhap-hang-${new Date().toISOString().split('T')[0]}.xlsx`);
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
          return;
        } catch (apiError) {
          console.error('API export to Excel failed, using client-side fallback:', apiError);
          // Tiếp tục với phương pháp client-side
        }
        
        // Client-side export khi API thất bại
        // Import thư viện XLSX
        const XLSX = await import('xlsx');
        const FileSaver = await import('file-saver');
        
        // Chuẩn bị dữ liệu cho Excel
        const reportTitle = "BÁO CÁO NHẬP HÀNG";
        const filterInfo = [
          ["Thời gian:", `${this.formatDate(this.filter.fromDate)} - ${this.formatDate(this.filter.toDate)}`],
          ["Kho:", this.filter.warehouseId ? this.getWarehouseName(this.filter.warehouseId) : "Tất cả"],
          ["Trạng thái:", this.filter.status ? this.getStatusText(this.filter.status) : "Tất cả"],
        ];
        
        const summaryData = [
          ["THỐNG KÊ", ""],
          ["Tổng số phiếu nhập:", this.reportSummary.totalImports],
          ["Tổng giá trị nhập:", this.formatCurrency(this.reportSummary.totalValue)],
          ["Số lượng sản phẩm:", this.reportSummary.totalQuantity],
          ["Số nhà cung cấp:", this.reportSummary.supplierCount],
          ["", ""]
        ];
        
        // Chuẩn bị dữ liệu chi tiết
        const headers = ["ID", "Kho", "Ngày nhập", "Giá trị", "Trạng thái"];
        const importData = this.imports.map(item => [
          item.id,
          this.getWarehouseName(item.warehouseId),
          this.formatDate(item.dateInput),
          this.formatCurrency(item.cost),
          this.getStatusText(item.status)
        ]);
        
        // Tạo workbook và worksheet
        const wb = XLSX.utils.book_new();
        
        // Tiêu đề báo cáo
        const titleData = [[reportTitle]];
        const wsTitle = XLSX.utils.aoa_to_sheet(titleData);
        
        // Thông tin lọc
        const wsFilter = XLSX.utils.aoa_to_sheet(filterInfo);
        
        // Dữ liệu thống kê
        const wsSummary = XLSX.utils.aoa_to_sheet(summaryData);
        
        // Dữ liệu chi tiết với header
        const wsData = XLSX.utils.aoa_to_sheet([headers, ...importData]);
        
        // Tạo worksheet cuối cùng bằng cách kết hợp tất cả
        const finalWorksheet = Object.assign({},
          wsTitle, 
          { '!ref': 'A1:E1' }
        );
        
        // Vị trí bắt đầu của từng phần
        const filterStart = 3; // Bắt đầu từ dòng 3
        const summaryStart = filterStart + filterInfo.length + 1;
        const detailStart = summaryStart + summaryData.length + 1;
        
        // Chèn dữ liệu vào worksheet
        for (let i = 0; i < filterInfo.length; i++) {
          const cell1 = XLSX.utils.encode_cell({r: filterStart + i, c: 0});
          const cell2 = XLSX.utils.encode_cell({r: filterStart + i, c: 1});
          finalWorksheet[cell1] = { v: filterInfo[i][0], t: 's' };
          finalWorksheet[cell2] = { v: filterInfo[i][1], t: 's' };
        }
        
        for (let i = 0; i < summaryData.length; i++) {
          const cell1 = XLSX.utils.encode_cell({r: summaryStart + i, c: 0});
          const cell2 = XLSX.utils.encode_cell({r: summaryStart + i, c: 1});
          finalWorksheet[cell1] = { v: summaryData[i][0], t: 's' };
          finalWorksheet[cell2] = { v: summaryData[i][1], t: 's' };
        }
        
        // Thêm header
        for (let i = 0; i < headers.length; i++) {
          const cell = XLSX.utils.encode_cell({r: detailStart, c: i});
          finalWorksheet[cell] = { v: headers[i], t: 's' };
        }
        
        // Thêm dữ liệu chi tiết
        for (let i = 0; i < importData.length; i++) {
          for (let j = 0; j < importData[i].length; j++) {
            const cell = XLSX.utils.encode_cell({r: detailStart + i + 1, c: j});
            finalWorksheet[cell] = { v: importData[i][j], t: 's' };
          }
        }
        
        // Cập nhật vùng dữ liệu cho worksheet
        finalWorksheet['!ref'] = XLSX.utils.encode_range({
          s: {r: 0, c: 0},
          e: {r: detailStart + importData.length, c: 4}
        });
        
        // Thêm worksheet vào workbook
        XLSX.utils.book_append_sheet(wb, finalWorksheet, "Báo cáo nhập hàng");
        
        // Xuất workbook thành file Excel
        const excelBuffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
        const blob = new Blob([excelBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        FileSaver.saveAs(blob, `bao-cao-nhap-hang-${new Date().toISOString().split('T')[0]}.xlsx`);
        
      } catch (error) {
        console.error('Error exporting to Excel:', error);
        alert('Không thể xuất file Excel. Đã xảy ra lỗi trong quá trình tạo file.');
      } finally {
        this.isLoading = false;
      }
    },
    
    // Phiên bản sử dụng font tùy chỉnh (thêm vào đầu phương thức exportToPdf)
    async exportToPdf() {
      try {
        this.isLoading = true;
        
        // Thử sử dụng API trước với tùy chọn buộc dùng client-side
        try {
          // Bỏ qua API và sử dụng client-side để đảm bảo hiển thị tiếng Việt đúng
          throw new Error('Use client-side PDF generation');
        } catch (apiError) {
          console.log('Sử dụng client-side để xuất PDF với font tiếng Việt');
        }
        
        // Import pdfmake và thư viện liên quan
        const pdfMake = await import('pdfmake/build/pdfmake');
        const pdfFonts = await import('pdfmake/build/vfs_fonts');
        
        // Cấu hình fonts cho pdfmake
        pdfMake.default.vfs = pdfFonts.default.pdfMake.vfs;
        
        // Định nghĩa fonts bao gồm font có hỗ trợ tiếng Việt
        const fontDefinition = {
          Roboto: {
            normal: 'https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.2.7/fonts/Roboto/Roboto-Regular.ttf',
            bold: 'https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.2.7/fonts/Roboto/Roboto-Medium.ttf',
            italics: 'https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.2.7/fonts/Roboto/Roboto-Italic.ttf',
            bolditalics: 'https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.2.7/fonts/Roboto/Roboto-MediumItalic.ttf'
          }
        };
        
        // Áp dụng font definition
        pdfMake.default.vfs = pdfFonts.default.pdfMake.vfs;
        pdfMake.default.fonts = fontDefinition;
        
        // Chuẩn bị dữ liệu báo cáo
        const reportTitle = "BÁO CÁO NHẬP HÀNG";
        const timeRange = `Thời gian: ${this.formatDate(this.filter.fromDate)} - ${this.formatDate(this.filter.toDate)}`;
        const warehouseInfo = `Kho: ${this.filter.warehouseId ? this.getWarehouseName(this.filter.warehouseId) : "Tất cả"}`;
        const statusInfo = `Trạng thái: ${this.filter.status ? this.getStatusText(this.filter.status) : "Tất cả"}`;
        
        // Dữ liệu thống kê
        const summaryData = [
          [`Tổng số phiếu nhập: ${this.reportSummary.totalImports}`],
          [`Tổng giá trị nhập: ${this.formatCurrency(this.reportSummary.totalValue)}`],
          [`Số lượng sản phẩm: ${this.reportSummary.totalQuantity}`],
          [`Số nhà cung cấp: ${this.reportSummary.supplierCount}`]
        ];
        
        // Dữ liệu bảng chi tiết
        const tableHeaders = ['ID', 'Kho', 'Ngày nhập', 'Giá trị', 'Trạng thái'];
        const tableData = this.imports.map(item => [
          item.id.toString(),
          this.getWarehouseName(item.warehouseId),
          this.formatDate(item.dateInput),
          this.formatCurrency(item.cost),
          this.getStatusText(item.status)
        ]);
        
        // Tạo định nghĩa document cho PDF
        const documentDefinition = {
          pageSize: 'A4',
          pageOrientation: 'portrait',
          content: [
            // Tiêu đề báo cáo
            {
              text: reportTitle,
              fontSize: 18,
              bold: true,
              alignment: 'center',
              margin: [0, 0, 0, 20]
            },
            
            // Thông tin lọc
            { text: timeRange, fontSize: 11, margin: [0, 0, 0, 5] },
            { text: warehouseInfo, fontSize: 11, margin: [0, 0, 0, 5] },
            { text: statusInfo, fontSize: 11, margin: [0, 0, 0, 20] },
            
            // Tiêu đề thống kê
            {
              text: 'THỐNG KÊ',
              fontSize: 14,
              bold: true,
              margin: [0, 0, 0, 10]
            },
            
            // Dữ liệu thống kê
            {
              layout: 'noBorders',
              table: {
                widths: ['100%'],
                body: summaryData
              },
              margin: [0, 0, 0, 20]
            },
            
            // Tiêu đề bảng chi tiết
            {
              text: 'CHI TIẾT NHẬP HÀNG',
              fontSize: 14,
              bold: true,
              margin: [0, 0, 0, 10]
            },
            
            // Bảng chi tiết
            {
              table: {
                headerRows: 1,
                widths: ['auto', '*', 'auto', 'auto', '*'],
                body: [tableHeaders, ...tableData]
              },
              layout: {
                fillColor: function (rowIndex) {
                  return rowIndex === 0 ? '#0088cc' : (rowIndex % 2 === 0 ? '#f2f2f2' : null);
                },
                fillOpacity: function (rowIndex) {
                  return rowIndex === 0 ? 1 : 0.5;
                },
                hLineWidth: function () { return 1; },
                vLineWidth: function () { return 1; },
                hLineColor: function () { return '#aaa'; },
                vLineColor: function () { return '#aaa'; },
              }
            }
          ],
          
          // Định nghĩa styles
          styles: {
            header: {
              fontSize: 18,
              bold: true,
              alignment: 'center',
              margin: [0, 0, 0, 10]
            }
          },
          
          // Định nghĩa defaults
          defaultStyle: {
            font: 'Roboto'
          },
          
          // Footer cho mỗi trang
          footer: function(currentPage, pageCount) {
            return {
              text: `Trang ${currentPage} / ${pageCount} - Ngày xuất: ${new Date().toLocaleDateString('vi-VN')}`,
              alignment: 'center',
              fontSize: 8,
              margin: [0, 10, 0, 0]
            };
          }
        };
        
        // Đối tượng styles mở rộng cho định dạng text
        const styles = {
          header: {
            fontSize: 18,
            bold: true,
            color: '#000000',
            alignment: 'center',
            margin: [0, 0, 0, 10]
          },
          subheader: {
            fontSize: 14,
            bold: true,
            color: '#0d6efd',
            margin: [0, 0, 0, 10]
          },
          tableHeader: {
            fontSize: 12,
            bold: true,
            color: '#ffffff',
            fillColor: '#0d6efd'
          },
          tableRow: {
            fontSize: 10,
            color: '#000000'
          },
          tableTotal: {
            fontSize: 11,
            bold: true,
            color: '#000000'
          }
        };
        
        // Tạo PDF và lưu file
        const pdfDocGenerator = pdfMake.default.createPdf(documentDefinition);
        pdfDocGenerator.download(`bao-cao-nhap-hang-${new Date().toLocaleDateString('vi-VN').replace(/\//g, '-')}.pdf`);
        
      } catch (error) {
        console.error('Error exporting to PDF:', error);
        alert('Không thể xuất file PDF. Đã xảy ra lỗi trong quá trình tạo file.');
      } finally {
        this.isLoading = false;
      }
    },
    
    getWarehouseName(id) {
      const warehouse = this.warehouses.find(w => w.id === id);
      return warehouse ? warehouse.name : 'Không xác định';
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
        case 'Pending': return 'badge bg-warning';
        case 'Approved': return 'badge bg-success';
        case 'Rejected': return 'badge bg-danger';
        default: return 'badge bg-secondary';
      }
    },
    
    formatDate(dateString) {
      if (!dateString) return '';
      try {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
      } catch (e) {
        return dateString;
      }
    },
    
    // Thay thế phương thức formatCurrency hiện tại
    formatCurrency(value) {
      if (value === null || value === undefined) return '0 ₫';
      
      // Định dạng tiền tệ
      const formatter = new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
      });
      
      return formatter.format(value);
    },

    // Thêm phương thức mới cho Excel
    formatCurrencyRaw(value) {
      if (value === null || value === undefined) return '0';
      return value.toString();
    },

    handleApiError(error, defaultMessage) {
      console.error(defaultMessage, error);
      let errorMessage = defaultMessage;
      
      if (error.response) {
        // Lỗi từ phản hồi của máy chủ
        errorMessage += ` (${error.response.status}: ${error.response.statusText || 'Unknown'})`;
      } else if (error.request) {
        // Không nhận được phản hồi từ máy chủ
        errorMessage += ' (Không thể kết nối đến máy chủ)';
      }
      
      return errorMessage;
    },

    // Hàm chuyển đổi chuỗi tiếng Việt sang không dấu
    removeAccents(str) {
      if (!str) return str;
      return str.normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/đ/g, 'd')
        .replace(/Đ/g, 'D');
    },
    
    // Hàm định dạng tiền tệ không dấu cho PDF
    formatCurrencyNoDiacritics(value) {
      if (value === null || value === undefined) return '0 d';
      return value.toLocaleString('en-US') + ' d';
    },
    
    getStatusTextNoDiacritics(status) {
      const statusMap = {
        'Pending': 'Dang xu ly',
        'Approved': 'Da duyet',
        'Rejected': 'Da tu choi',
        'Completed': 'Hoan thanh'
      };
      
      return statusMap[status] || status || 'Khong xac dinh';
    },

    calculateTotalQuantity() {
      return this.importDetails.reduce((total, detail) => total + detail.quantity, 0);
    },

    calculateTotalAmount() {
      return this.importDetails.reduce((total, detail) => total + (detail.quantity * detail.cost), 0);
    },

    printDetail() {
      const printWindow = window.open('', '_blank');
  
      const printContent = `
        <!DOCTYPE html>
        <html>
        <head>
          <title>Phiếu nhập kho #${this.selectedImport.id}</title>
          <meta charset="utf-8">
          <style>
            body { font-family: Arial, sans-serif; margin: 0; padding: 20px; }
            h1, h2 { text-align: center; color: #000; }
            .info { margin-bottom: 20px; }
            .info p { margin: 5px 0; }
            table { width: 100%; border-collapse: collapse; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f2f2f2; }
            .text-end { text-align: right; }
            .footer { margin-top: 30px; text-align: center; font-size: 12px; }
          </style>
        </head>
        <body>
          <h1>PHIẾU NHẬP KHO</h1>
          <h2>Mã phiếu: ${this.selectedImport.code || this.selectedImport.id}</h2>
          
          <div class="info">
            <p><strong>Kho nhập:</strong> ${this.getWarehouseName(this.selectedImport.warehouseId)}</p>
            <p><strong>Ngày nhập:</strong> ${this.formatDate(this.selectedImport.dateInput)}</p>
            <p><strong>Trạng thái:</strong> ${this.getStatusText(this.selectedImport.status)}</p>
          </div>
          
          <table>
            <thead>
              <tr>
                <th>STT</th>
                <th>Sản phẩm</th>
                <th>Số lượng</th>
                <th>Đơn giá</th>
                <th>Thành tiền</th>
                <th>Nhà cung cấp</th>
              </tr>
            </thead>
            <tbody>
              ${this.importDetails.map((detail, index) => `
                <tr>
                  <td>${index + 1}</td>
                  <td>${detail.productName || 'Sản phẩm #' + detail.idProduct}</td>
                  <td class="text-end">${detail.quantity}</td>
                  <td class="text-end">${this.formatCurrency(detail.cost)}</td>
                  <td class="text-end">${this.formatCurrency(detail.quantity * detail.cost)}</td>
                  <td>${detail.supplierName || 'Không xác định'}</td>
                </tr>
              `).join('')}
            </tbody>
            <tfoot>
              <tr>
                <td colspan="2" class="text-end"><strong>Tổng cộng:</strong></td>
                <td class="text-end"><strong>${this.calculateTotalQuantity()}</strong></td>
                <td></td>
                <td class="text-end"><strong>${this.formatCurrency(this.calculateTotalAmount())}</strong></td>
                <td></td>
              </tr>
            </tfoot>
          </table>
          
          <div class="footer">
            <p>Ngày in: ${new Date().toLocaleDateString('vi-VN')}</p>
          </div>
        </body>
        </html>
      `;
      
      printWindow.document.open();
      printWindow.document.write(printContent);
      printWindow.document.close();
      
      // Đợi tài nguyên tải xong rồi mới in
      printWindow.onload = function() {
        printWindow.print();
      };
    }
  }
};
</script>

<style scoped>
.title-black {
  color: var(--text-dark, #000000);
  font-weight: 700;
  font-size: 1.8rem;
}

.mb-3 {
  color: #000000 !important;
}

.table-primary th {
  background-color: var(--primary-color, #0d6efd) !important;
  color: var(--text-white, #ffffff) !important;
  font-weight: 600;
}

.card {
  border-radius: 0.375rem;
  box-shadow: var(--box-shadow-sm, 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075));
}

.table-hover tbody tr:hover {
  color: var(--text-dark, #000000);
  background-color: var(--primary-light, rgba(13, 110, 253, 0.075));
}

.modal-title {
  color: var(--text-dark, #111) !important;
  font-weight: 600;
}

.table-light th {
  background-color: var(--bg-light, #f8f9fa);
  color: var(--text-dark, #333);
  font-weight: 600;
}

.alert-info {
  color: var(--text-dark, #000) !important;
  background-color: var(--info-light, #e2f0fb);
}

/* Màu chữ cho bảng */
.table {
  color: #000000;
}

/* Màu chữ cho các thẻ badge */
.badge {
  padding: 0.35em 0.65em;
  font-size: 0.75em;
  font-weight: 600;
}

/* Màu chữ cho tiêu đề thẻ */
.card-title {
  margin-bottom: 0.5rem;
  font-size: 1rem;
  color: #f8f9fa;
  font-weight: 600;
}

/* Màu chữ cho số liệu trong thẻ */
.display-6 {
  font-size: 1.75rem;
  font-weight: 700;
  color: #ffffff;
}

/* Màu chữ cho label */
label {
  font-weight: 500;
  color: #000000;
}

/* Màu chữ cho nút */
.btn {
  font-weight: 500;
}

/* Màu chữ cho thông báo lỗi */
.alert-danger {
  color: #721c24;
}

/* Màu chữ cho thông báo thông tin */
.alert-info {
  color: #0c5460;
}

.card {
  border-radius: 0.375rem;
  box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
}

/* Màu chữ khi hover trên các hàng bảng */
.table-hover tbody tr:hover {
  color: #000000;
  background-color: rgba(13, 110, 253, 0.075);
}

/* Màu chữ cho footer trong PDF */
.pdf-footer {
  color: #6c757d;
}

/* Màu chữ cho tiêu đề PDF */
.pdf-title {
  color: #000000;
}

/* Thêm các style mới cho modal chi tiết */
.modal-title {
  color: #111 !important;
  font-weight: 600;
}

.table-light th {
  background-color: #f8f9fa;
  color: #333;
  font-weight: 600;
}

.table-striped tbody tr:nth-of-type(odd) {
  background-color: rgba(0, 0, 0, 0.02);
}

.modal-body h5 {
  color: #000;
  font-weight: 600;
  margin-top: 1.5rem;
  margin-bottom: 1rem;
  font-size: 1.05rem;
}

/* Style cho thông tin phiếu */
.modal-body p {
  margin-bottom: 0.5rem;
  color: #000;
}

.modal-body p strong {
  display: inline-block;
  min-width: 120px;
  font-weight: 600;
}

/* Style cho bảng chi tiết */
.modal-body .table {
  margin-bottom: 0;
  color: #000;
}

.modal-footer .btn-primary {
  background-color: #0d6efd;
}

.modal-footer .btn-secondary {
  background-color: #6c757d;
}

/* Chỉnh màu chữ sang đen cho tất cả văn bản trong modal */
.modal-content {
  color: #000 !important;
}

/* Chỉnh màu cho tiêu đề modal */
.modal-title {
  color: #000 !important;
  font-weight: 600;
}

/* Chỉnh màu cho các nhãn */
.modal-body p strong {
  color: #000 !important;
  font-weight: 600;
}

.modal-body p {
  color: #000 !important;
}

/* Chỉnh màu cho các tiêu đề phần */
.modal-body h5 {
  color: #000 !important;
  font-weight: 600;
}

/* Chỉnh màu cho bảng */
.table th {
  color: #000 !important;
  font-weight: 600;
}

.table td {
  color: #000 !important;
}

/* Chỉnh màu cho dòng tổng cộng */
.table tfoot td {
  color: #000 !important;
  font-weight: 600;
}

/* Chỉnh màu cho thông báo */
.alert {
  color: #000 !important;
}

.alert-info {
  color: #000 !important;
  background-color: #e2f0fb;
}

/* Chỉnh màu cho các badge trạng thái */
.badge.bg-success {
  color: #fff !important;
}

.badge.bg-warning {
  color: #000 !important;
}

.badge.bg-danger {
  color: #fff !important;
}

/* Chỉnh màu cho văn bản trong ghi chú */
.p-3.bg-light.rounded {
  color: #000 !important;
}

/* Đảm bảo text trong button đọc được */
.btn {
  color: #fff !important;
}

.btn-secondary {
  color: #fff !important;
}
</style>