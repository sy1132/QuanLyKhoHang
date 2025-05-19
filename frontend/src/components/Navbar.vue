<template>
  <nav class="navbar">
    <div class="navbar-container">
      <div class="navbar-left">
        <router-link class="nav-item" to="/"
          ><i class="fas fa-eye"></i> Tổng quan</router-link
        >
        <router-link class="nav-item" to="/products"
          ><i class="fas fa-th-list"></i> Danh mục</router-link
        >
        <router-link class="nav-item" to="/import"
          ><i class="fas fa-download"></i> Nhập hàng</router-link
        >
        <router-link class="nav-item" to="/Supplier"
          ><i class="fas fa-users"></i> Đối tác</router-link
        >
        <router-link class="nav-item" to="/warehouse"
          ><i class="fas fa-warehouse"></i> Nhà Kho</router-link
        >
      </div>
      <div class="navbar-right">
        <template v-if="userName">
          <div
            class="user-info"
            @mouseenter="showDropdown = true"
            @mouseleave="showDropdown = false"
          >
            <img class="avatar" src="https://i.pravatar.cc/32" alt="avatar" />
            <div class="username-dropdown">
              <span class="username">{{ userName }}</span>
              <div v-if="showDropdown" class="dropdown-menu">
                <button class="logout-btn" @click="logout">Đăng xuất</button>
              </div>
            </div>
          </div>
        </template>
        <template v-else>
          <router-link to="/login" class="login-btn">Đăng nhập</router-link>
          <router-link to="/register" class="register-btn">Đăng ký</router-link>
        </template>
      </div>
    </div>
  </nav>
</template>

<script>
export default {
  name: "Navbar",
  data() {
    return {
      userName: "",
      showDropdown: false,
    };
  },
  methods: {
    syncUserName() {
      this.userName = localStorage.getItem("userName") || "";
      window.dispatchEvent(new Event("storage"));
    },
    logout() {
      localStorage.removeItem("authToken");
      localStorage.removeItem("user");
      localStorage.removeItem("userName");
      this.syncUserName();
      this.$router.push("/login");
    },
    loginSuccess(result) {
      localStorage.setItem("userName", result.userName);
      window.dispatchEvent(new Event("storage"));
      window.location.reload(); // Thêm dòng này để load lại trang
    },
  },
  mounted() {
    this.syncUserName();
    window.addEventListener("storage", this.syncUserName);
  },
  beforeUnmount() {
    window.removeEventListener("storage", this.syncUserName);
  },
};
</script>

<style scoped>
.navbar {
  width: 100%;
  background: #0a74e6;
  height: 56px;
  display: flex;
  align-items: center;
}

.navbar-container {
  width: 100%;
  max-width: 1300px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 32px;
  box-sizing: border-box;
}

.navbar-left {
  display: flex;
  gap: 28px;
  /* Tăng khoảng cách giữa các trường */
}

.nav-item {
  color: #fff;
  text-decoration: none;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 18px;
  height: 40px;
  border-radius: 8px;
  transition: background 0.2s, color 0.2s, box-shadow 0.2s, transform 0.15s;
  font-size: 1.08rem;
  position: relative;
}

.nav-item:hover {
  background: #fff;
  color: #0a74e6;
  box-shadow: 0 2px 12px rgba(10, 116, 230, 0.12);
  transform: translateY(-2px) scale(1.06);
  text-shadow: 0 1px 6px #b3d7ff;
}

.nav-item i {
  transition: color 0.2s;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #fff;
  font-weight: 500;
  font-size: 1.08rem;
}

.avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #fff;
  box-shadow: 0 2px 8px rgba(10, 116, 230, 0.15);
  transition: box-shadow 0.2s;
}

.user-info:hover .avatar {
  box-shadow: 0 4px 16px rgba(10, 116, 230, 0.25);
}

.username-dropdown {
  position: relative;
  display: inline-block;
}

.dropdown-menu {
  position: absolute;
  top: 120%;
  left: 0;
  background: #fff;
  min-width: 120px;
  border-radius: 6px;
  box-shadow: 0 4px 16px rgba(10, 116, 230, 0.15);
  z-index: 100;
  padding: 8px 0;
  text-align: left;
  display: block;
}

.dropdown-menu .logout-btn {
  width: 100%;
  background: none;
  color: #0a74e6;
  border: none;
  padding: 8px 16px;
  text-align: left;
  border-radius: 0;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s, color 0.2s;
}

.dropdown-menu .logout-btn:hover {
  background: #f0f6ff;
  color: #d32f2f;
}

.login-btn,
.logout-btn,
.register-btn {
  margin-left: 16px;
  padding: 6px 16px;
  border: none;
  border-radius: 6px;
  background: #fff;
  color: #0a74e6;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s, color 0.2s, box-shadow 0.2s;
  font-size: 1rem;
  box-shadow: 0 2px 8px rgba(10, 116, 230, 0.1);
}

.login-btn:hover,
.logout-btn:hover,
.register-btn:hover {
  background: #0a74e6;
  color: #fff;
  box-shadow: 0 4px 16px rgba(10, 116, 230, 0.18);
}
</style>
