<template>
  <div class="home-container">
    <div class="hero-section">
      <h1>Hệ Thống Quản Lý Kho Hàng</h1>
      <p class="hero-subtitle">
        Giải pháp quản lý kho hàng hiệu quả cho doanh nghiệp của bạn
      </p>

      <div class="action-buttons">
        <!-- Hiển thị nút dựa trên trạng thái đăng nhập -->
        <template v-if="!isLoggedIn">
          <router-link to="/login" class="login-btn">Đăng nhập</router-link>
          <router-link to="/register" class="register-btn">Đăng ký</router-link>
        </template>
        <template v-else>
          <router-link to="/dashboard" class="dashboard-btn">
            <i class="fas fa-tachometer-alt"></i> Bảng điều khiển
          </router-link>
          <router-link to="/add-product" class="primary-btn">
            <i class="fas fa-plus-circle"></i> Thêm sản phẩm
          </router-link>
          <router-link to="/products" class="secondary-btn">
            <i class="fas fa-list"></i> Danh sách sản phẩm
          </router-link>
          <button @click="logout" class="logout-btn">
            <i class="fas fa-sign-out-alt"></i> Đăng xuất
          </button>

        </template>


        
      </div>
    </div>

    <!-- Phần giới thiệu tính năng -->
    <div class="features-section" v-if="!isLoggedIn">
      <h2>Tính năng nổi bật</h2>
      <div class="features-grid">
        <div class="feature-card">
          <div class="feature-icon">📊</div>
          <h3>Thống kê trực quan</h3>
          <p>
            Xem báo cáo, số liệu thống kê về kho hàng một cách trực quan và dễ
            dàng
          </p>
        </div>
        <div class="feature-card">
          <div class="feature-icon">🔍</div>
          <h3>Tìm kiếm nhanh chóng</h3>
          <p>
            Dễ dàng tìm kiếm sản phẩm với bộ lọc thông minh và kết quả tìm kiếm
            nhanh chóng
          </p>
        </div>
        <div class="feature-card">
          <div class="feature-icon">📱</div>
          <h3>Giao diện thân thiện</h3>
          <p>
            Trải nghiệm người dùng tuyệt vời trên mọi thiết bị với giao diện
            tiện lợi
          </p>
        </div>
        <div class="feature-card">
          <div class="feature-icon">🔔</div>
          <h3>Cảnh báo kịp thời</h3>
          <p>Nhận thông báo khi hàng tồn kho sắp hết hoặc đã quá hạn</p>
        </div>
      </div>
    </div>

    <!-- Hiển thị thống kê nhanh nếu đã đăng nhập -->
    <div class="dashboard-preview" v-if="isLoggedIn">
      <h2>Tổng quan</h2>
      <div class="stats-container">
        <div class="stat-card">
          <h3>Tổng sản phẩm</h3>
          <div class="stat-value">{{ stats.totalProducts || 0 }}</div>
        </div>
        <div class="stat-card">
          <h3>Sắp hết hàng</h3>
          <div class="stat-value warning">{{ stats.lowStock || 0 }}</div>
        </div>
        <div class="stat-card">
          <h3>Mới nhập kho</h3>
          <div class="stat-value success">{{ stats.newArrivals || 0 }}</div>
        </div>
        <div class="stat-card">
          <h3>Hoạt động gần đây</h3>
          <div class="stat-value">{{ stats.recentActivities || 0 }}</div>
        </div>
      </div>
    </div>

    <footer class="footer">
      <p>&copy; 2025 Hệ thống Quản lý Kho hàng</p>
    </footer>
  </div>
</template>

<script>
import authApi from "@/api/auth.api";

export default {
  name: "HomeView",
  data() {
    return {
      isLoggedIn: false,
      stats: {
        totalProducts: 0,
        lowStock: 0,
        newArrivals: 0,
        recentActivities: 0,
      },
    };
  },
  methods: {
    checkLoginStatus() {
      const token = localStorage.getItem("authToken");
      this.isLoggedIn = !!token;

      if (this.isLoggedIn) {
        this.fetchDashboardStats();
      }
    },
    async logout() {
      try {
        localStorage.removeItem("authToken");
        localStorage.removeItem("user");
        this.isLoggedIn = false;
        this.$router.push("/login");
      } catch (error) {
        console.error("Logout error:", error);
      }
    },
    async fetchDashboardStats() {
      try {
        // Giả lập dữ liệu thống kê (trong thực tế sẽ fetch từ API)
        setTimeout(() => {
          this.stats = {
            totalProducts: 128,
            lowStock: 12,
            newArrivals: 8,
            recentActivities: 24,
          };
        }, 500);
      } catch (error) {
        console.error("Error fetching dashboard stats:", error);
      }
    },
  },
  mounted() {
    this.checkLoginStatus();
  },
};
</script>

<style scoped>
.home-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.hero-section {
  text-align: center;
  padding: 3rem 1rem;
  margin-bottom: 2rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 12px;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

h1 {
  font-size: 2.8rem;
  color: #2c3e50;
  margin-bottom: 1rem;
  font-weight: 700;
}

.hero-subtitle {
  font-size: 1.2rem;
  color: #34495e;
  margin-bottom: 2.5rem;
}

.action-buttons {
  display: flex;
  justify-content: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.login-btn,
.register-btn,
.primary-btn,
.secondary-btn,
.logout-btn,
.dashboard-btn {
  padding: 0.8rem 1.5rem;
  border-radius: 6px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.login-btn {
  background-color: #3498db;
  color: white;
}

.login-btn:hover {
  background-color: #2980b9;
}

.register-btn {
  background-color: transparent;
  color: #3498db;
  border: 2px solid #3498db;
}

.register-btn:hover {
  background-color: rgba(52, 152, 219, 0.1);
}

.dashboard-btn {
  background-color: #9b59b6;
  color: white;
}

.dashboard-btn:hover {
  background-color: #8e44ad;
}

.primary-btn {
  background-color: #4caf50;
  color: white;
}

.primary-btn:hover {
  background-color: #45a049;
  transform: translateY(-2px);
}

.secondary-btn {
  background-color: transparent;
  color: #4caf50;
  border: 2px solid #4caf50;
}

.secondary-btn:hover {
  background-color: rgba(76, 175, 80, 0.1);
}

.logout-btn {
  background-color: #f44336;
  color: white;
  border: none;
  cursor: pointer;
}

.logout-btn:hover {
  background-color: #d32f2f;
}

/* Feature section */
.features-section {
  padding: 3rem 0;
  text-align: center;
}

.features-section h2 {
  font-size: 2rem;
  color: #2c3e50;
  margin-bottom: 2.5rem;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 2rem;
}

.feature-card {
  background-color: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease;
}

.feature-card:hover {
  transform: translateY(-5px);
}

.feature-icon {
  font-size: 2.5rem;
  margin-bottom: 1rem;
}

.feature-card h3 {
  color: #2c3e50;
  margin-bottom: 1rem;
}

.feature-card p {
  color: #7f8c8d;
  line-height: 1.6;
}

/* Dashboard preview */
.dashboard-preview {
  margin: 2rem 0;
}

.dashboard-preview h2 {
  font-size: 1.8rem;
  color: #2c3e50;
  margin-bottom: 1.5rem;
}

.stats-container {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1.5rem;
}

.stat-card {
  background-color: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  text-align: center;
}

.stat-card h3 {
  color: #7f8c8d;
  font-size: 1rem;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.stat-value {
  font-size: 2.2rem;
  font-weight: 700;
  color: #2c3e50;
}

.warning {
  color: #e74c3c;
}

.success {
  color: #27ae60;
}

/* Footer */
.footer {
  margin-top: 3rem;
  text-align: center;
  color: #7f8c8d;
  padding: 1.5rem 0;
  border-top: 1px solid #ecf0f1;
}

/* Responsive */
@media (max-width: 768px) {
  h1 {
    font-size: 2rem;
  }

  .hero-subtitle {
    font-size: 1rem;
  }

  .action-buttons {
    flex-direction: column;
    align-items: stretch;
    max-width: 300px;
    margin: 0 auto;
  }
}
</style>
