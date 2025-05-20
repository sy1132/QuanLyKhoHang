<template>
  <div class="container mt-4">
    <h1 class="title-black mb-3">Báo cáo nhập hàng</h1>

    <div class="card mb-4">
      <div class="card-header py-2 px-3">
        <strong>Thời gian báo cáo</strong>
      </div>
      <div class="card-body py-2 px-3">
        <div class="row">
          <div class="col-md-4">
            <label>Từ ngày:</label>
            <input type="date" v-model="filter.fromDate" class="form-control" />
          </div>
          <div class="col-md-4">
            <label>Đến ngày:</label>
            <input type="date" v-model="filter.toDate" class="form-control" />
          </div>
          <div class="col-md-4 d-flex align-items-end">
            <button @click="generateReport" class="btn btn-info custom-btn-size me-2">
              <i class="fas fa-chart-bar"></i> Tạo báo cáo
            </button>
            <button @click="exportReport" class="btn btn-success custom-btn-size">
              <i class="fas fa-file-export"></i> Xuất Excel
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Thống kê tổng quan -->
    <div class="row mb-4">
      <div class="col-md-3">
        <div class="card bg-primary text-white">
          <div class="card-body">
            <h5 class="card-title">Tổng phiếu nhập</h5>
            <h2>{{ statistics.totalImports }}</h2>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-success text-white">
          <div class="card-body">
            <h5 class="card-title">Đã duyệt</h5>
            <h2>{{ statistics.approvedImports }}</h2>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-warning text-dark">
          <div class="card-body">
            <h5 class="card-title">Chờ duyệt</h5>
            <h2>{{ statistics.pendingImports }}</h2>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card bg-danger text-white">
          <div class="card-body">
            <h5 class="card-title">Từ chối</h5>
            <h2>{{ statistics.rejectedImports }}</h2>
          </div>
        </div>
      </div>
    </div>

    <!-- Biểu đồ -->
    <div class="row mb-4">
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">Giá trị nhập theo ngày</div>
          <div class="card-body">
            <canvas ref="dailyImportChart"></canvas>
          </div>
        </div>
      </div>
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">Số lượng nhập theo sản phẩm</div>
          <div class="card-body">
            <canvas ref="productImportChart"></canvas>
          </div>
        </div>
      </div>
    </div>

    <!-- Bảng chi tiết -->
    <div class="card">
      <div class="card-header">Chi tiết nhập hàng</div>
      <div class="card-body">
        <div class="table-responsive">
          <table class="table table-bordered table-hover">
            <thead class="table-primary">
              <tr>
                <th>Ngày nhập</th>
                <th>Sản phẩm</th>
                <th>Nhà cung cấp</th>
                <th>Số lượng</th>
                <th>Đơn giá</th>
                <th>Thành tiền</th>
                <th>Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in reportData" :key="index">
                <td>{{ formatDate(item.dateInput) }}</td>
                <td>{{ item.productName }}</td>
                <td>{{ item.supplierName }}</td>
                <td class="text-center">{{ item.quantity }}</td>
                <td>{{ formatCurrency(item.cost) }}</td>
                <td>{{ formatCurrency(item.quantity * item.cost) }}</td>
                <td>
                  <span :class="getStatusBadgeClass(item.status)">
                    {{ getStatusText(item.status) }}
                  </span>
                </td>
              </tr>
              <tr v-if="reportData.length === 0">
                <td colspan="7" class="text-center text-muted">Không có dữ liệu báo cáo</td>
              </tr>
            </tbody>
            <tfoot v-if="reportData.length > 0">
              <tr>
                <td colspan="3" class="text-end"><strong>Tổng cộng:</strong></td>
                <td class="text-center"><strong>{{ calculateTotalQuantity() }}</strong></td>
                <td></td>
                <td colspan="2"><strong>{{ formatCurrency(calculateTotalAmount()) }}</strong></td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import Chart from 'chart.js/auto';

export default {
  name: 'ImportReportView',
  data() {
    return {
      filter: {
        fromDate: this.getDefaultFromDate(),
        toDate: this.getDefaultToDate()
      },
      reportData: [],
      statistics: {
        totalImports: 0,
        approvedImports: 0,
        pendingImports: 0,
        rejectedImports: 0
      },
      dailyChart: null,
      productChart: null
    };
  },
  mounted() {
    this.generateReport();
  },
  methods: {
    getDefaultFromDate() {
      const date = new Date();
      date.setDate(1); // Ngày đầu tiên của tháng hiện tại
      return date.toISOString().split('T')[0];
    },
    
    getDefaultToDate() {
      return new Date().toISOString().split('T')[0];
    },
    
    async generateReport() {
      try {
        const params = new URLSearchParams();
        
        if (this.filter.fromDate) {
          params.append('fromDate', this.filter.fromDate);
        }
        
        if (this.filter.toDate) {
          params.append('toDate', this.filter.toDate);
        }
        
        const response = await axios.get(`/api/report/import?${params.toString()}`);
        
        if (response.data && response.data.data) {
          this.reportData = response.data.data.details || [];
          this.statistics = response.data.data.statistics || {
            totalImports: 0,
            approvedImports: 0,
            pendingImports: 0,
            rejectedImports: 0
          };
          
          this.renderCharts();
        }
      } catch (error) {
        console.error('Error generating report:', error);
        this.$toast?.error('Không thể tạo báo cáo');
      }
    },
    
    exportReport() {
      const params = new URLSearchParams();
      
      if (this.filter.fromDate) {
        params.append('fromDate', this.filter.fromDate);
      }
      
      if (this.filter.toDate) {
        params.append('toDate', this.filter.toDate);
      }
      
      window.location.href = `${axios.defaults.baseURL}/api/report/import/export?${params.toString()}`;
    },
    
    renderCharts() {
      this.renderDailyChart();
      this.renderProductChart();
    },
    
    renderDailyChart() {
      // Tổng hợp dữ liệu theo ngày
      const dailyData = {};
      
      this.reportData.forEach(item => {
        const date = this.formatDate(item.dateInput);
        if (!dailyData[date]) {
          dailyData[date] = 0;
        }
        dailyData[date] += item.quantity * item.cost;
      });
      
      const dates = Object.keys(dailyData).sort();
      const values = dates.map(date => dailyData[date]);
      
      // Nếu đã có chart, cập nhật dữ liệu
      if (this.dailyChart) {
        this.dailyChart.data.labels = dates;
        this.dailyChart.data.datasets[0].data = values;
        this.dailyChart.update();
        return;
      }
      
      // Tạo chart mới
      const ctx = this.$refs.dailyImportChart.getContext('2d');
      this.dailyChart = new Chart(ctx, {
        type: 'line',
        data: {
          labels: dates,
          datasets: [{
            label: 'Giá trị nhập hàng',
            data: values,
            backgroundColor: 'rgba(54, 162, 235, 0.2)',
            borderColor: 'rgba(54, 162, 235, 1)',
            borderWidth: 1,
            fill: true
          }]
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true,
              ticks: {
                callback: function(value) {
                  return new Intl.NumberFormat('vi-VN', {
                    style: 'currency',
                    currency: 'VND',
                    minimumFractionDigits: 0
                  }).format(value);
                }
              }
            }
          }
        }
      });
    },
    
    renderProductChart() {
      // Tổng hợp dữ liệu theo sản phẩm
      const productData = {};
      
      this.reportData.forEach(item => {
        if (!productData[item.productName]) {
          productData[item.productName] = 0;
        }
        productData[item.productName] += item.quantity;
      });
      
      const products = Object.keys(productData);
      const quantities = products.map(product => productData[product]);
      
      // Tạo màu sắc ngẫu nhiên cho các sản phẩm
      const colors = products.map(() => this.getRandomColor());
      
      // Nếu đã có chart, cập nhật dữ liệu
      if (this.productChart) {
        this.productChart.data.labels = products;
        this.productChart.data.datasets[0].data = quantities;
        this.productChart.data.datasets[0].backgroundColor = colors;
        this.productChart.update();
        return;
      }
      
      // Tạo chart mới
      const ctx = this.$refs.productImportChart.getContext('2d');
      this.productChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: products,
          datasets: [{
            label: 'Số lượng nhập',
            data: quantities,
            backgroundColor: colors
          }]
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true
            }
          }
        }
      });
    },
    
    getRandomColor() {
      const r = Math.floor(Math.random() * 255);
      const g = Math.floor(Math.random() * 255);
      const b = Math.floor(Math.random() * 255);
      return `rgba(${r}, ${g}, ${b}, 0.6)`;
    },
    
    calculateTotalQuantity() {
      return this.reportData.reduce((sum, item) => sum + item.quantity, 0);
    },
    
    calculateTotalAmount() {
      return this.reportData.reduce((sum, item) => sum + (item.quantity * item.cost), 0);
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
  color: var(--text-dark, #111) !important;
}

.table-primary th {
  background-color: var(--primary-color, #007bff) !important;
  color: var(--text-white, #fff) !important;
  text-align: center;
  vertical-align: middle;
}

.custom-btn-size {
  min-width: 110px;
  min-height: 44px;
  font-size: 1rem;
  padding: 0.5rem 1.5rem;
  border-radius: 8px;
}

/* Card backgrounds */
.bg-primary {
  background-color: var(--primary-color, #007bff) !important;
}

.bg-success {
  background-color: var(--success-color, #27ae60) !important;
}

.bg-warning {
  background-color: var(--warning-color, #f39c12) !important;
}

.bg-danger {
  background-color: var(--danger-color, #e74c3c) !important;
}
</style>