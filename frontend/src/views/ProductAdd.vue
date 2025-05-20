<template>
  <div class="add-product-modal">
    <div class="add-product-header">
      <span class="title">Thêm sản phẩm</span>
      <button class="close-btn" @click="$router.push('/products')">×</button>
    </div>
    <form class="add-product-form" @submit.prevent="submitForm">
      <div class="form-grid">
        <!-- Hàng 1 -->
        <div class="form-group">
          <label>Mã vạch</label>
          <input type="text" v-model="product.barcode" required />
        </div>
        <div class="form-group">
          <label>Giá xuất</label>
          <input type="number" v-model.number="product.cost" min="0" required />
        </div>
        <!-- Hàng 2 -->
        <div class="form-group">
          <label>Tên sản phẩm</label>
          <input type="text" v-model="product.name" required />
        </div>
        <div class="form-group">
          <label>Giá nhập</label>
          <input type="number" v-model.number="product.price" min="0" required />
        </div>
        <!-- Hàng 3 -->
        <div class="form-group">
          <label>Loại hàng</label>
          <select v-model.number="product.categoryID" required>
            <option value="">---Chọn loại---</option>
            <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>Trạng thái</label>
          <select v-model.number="product.status" required>
            <option value="">---Chọn trạng thái---</option>
            <option :value="1">Đang bán</option>
            <option :value="0">Ngừng bán</option>
          </select>
        </div>
        <!-- Hàng 4 -->
        <div class="form-group">
          <label>Thương hiệu</label>
          <input type="text" v-model="product.brand" />
        </div>
        <div class="form-group">
          <label>Kho</label>
          <select v-model.number="product.warehouseId" required>
            <option value="">---Chọn kho---</option>
            <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
          </select>
        </div>
        <!-- Hàng 5: Vị trí (trái) - Ảnh sản phẩm (phải, cùng hàng: chọn + xem ảnh) -->
        <div class="form-group">
          <label>Vị trí</label>
          <input type="text" v-model="product.location" />
        </div>
        <div class="form-group image-upload-group">
          <label>Ảnh</label>
          <div class="image-upload-inline">
            <input type="file" @change="onImageChange" accept="image/*" />
            <div class="image-preview-list">
              <img v-if="imagePreview" :src="imagePreview" class="image-preview" />
            </div>
          </div>
        </div>
        <!-- Hàng cuối: Mô tả chiếm toàn bộ -->
        <div class="form-group full-width">
          <label>Mô tả</label>
          <textarea v-model="product.description" placeholder="Nhập mô tả sản phẩm..."></textarea>
        </div>
      </div>
      <div class="form-actions">
        <button type="submit" class="btn green"><i class="icon-save"></i> Lưu</button>
        <button type="button" class="btn gray" @click="$router.push('/products')">
          <i class="icon-cancel"></i> Bỏ qua
        </button>

      </div>
    </form>
  </div>
</template>

<script>
import axios from "axios";
export default {
  name: "ProductAdd",
  props: {
    initialWarehouseId: {
      type: [String, Number],
      default: null
    },
    isModal: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      product: {
        barcode: "",
        name: "",
        price: 0,
        cost: 0,
        brand: "",
        categoryID: "",
        warehouseId: this.initialWarehouseId || "",  // Sử dụng giá trị từ props
        status: "1", // Mặc định là 1 (Đang bán)
        description: "",
        image: null,
        location: ""
      },
      categories: [],
      warehouses: [],
      imagePreview: null,
      isSubmitting: false
    };
  },
  mounted() {
    this.fetchCategories();
    this.fetchWarehouses();
  },
  methods: {
    async fetchCategories() {
      try {
        const res = await axios.get("http://localhost:7189/api/categories");
        this.categories = res.data;
      } catch {
        this.categories = [
          { id: 1, name: "Hàng hóa" },
          { id: 2, name: "Dịch vụ" },
          { id: 3, name: "Combo" }
        ];
      }
    },
    async fetchWarehouses() {
      try {
        const res = await axios.get("https://localhost:7189/api/warehouse");
        // Lấy đúng mảng kho hàng từ response mới
        this.warehouses = res.data?.result?.data || [];
      } catch {
        this.warehouses = [
          { id: 1, name: "Kho A" },
          { id: 2, name: "Kho B" }
        ];
      }
    },
    onImageChange(e) {
      const file = e.target.files[0];
      if (file) {
        this.product.image = file;
        this.imagePreview = URL.createObjectURL(file);
      }
    },
    async submitForm(fromParent = false) {
      try {
        this.isSubmitting = true;
        
        console.log("Bắt đầu gửi form");
        
        const formData = new FormData();
        formData.append("Barcode", this.product.barcode);
        formData.append("Name", this.product.name);
        // Convert to string explicitly to avoid type issues
        formData.append("Price", String(this.product.price));
        formData.append("Cost", String(this.product.cost));
        formData.append("Brand", this.product.brand || "Không có thương hiệu");
        formData.append("CategoryID", String(this.product.categoryID || 1));
        formData.append("WarehouseId", String(this.product.warehouseId));
        formData.append("Status", String(this.product.status || 1));
        formData.append("Description", this.product.description || "Sản phẩm mới");
        formData.append("location", this.product.location || "Kho chính");
        formData.append("Num", "0");
        if (this.product.image) {
          formData.append("Image", this.product.image);
        }
        
        // Hiển thị một thông báo đang xử lý
        const loadingToast = document.createElement('div');
        loadingToast.innerHTML = 'Đang xử lý...';
        loadingToast.style = 'position: fixed; top: 20px; right: 20px; background: #ffc107; color: black; padding: 10px 20px; border-radius: 5px; z-index: 9999; box-shadow: 0 4px 8px rgba(0,0,0,0.2);';
        document.body.appendChild(loadingToast);
        
        console.log("Đang gửi request API");
        const response = await axios.post("https://localhost:7189/api/Products/create", formData, {
          headers: { "Content-Type": "multipart/form-data" }
        });
        
        // Xóa thông báo đang xử lý
        if (document.body.contains(loadingToast)) {
          document.body.removeChild(loadingToast);
        }
        
        console.log("Kết quả trả về:", response.data);
        
        // Nếu được gọi từ component cha khi dùng làm modal
        if (fromParent) {
          const newProduct = {
            id: response.data?.id || response.data?.Id,
            barcode: this.product.barcode,
            name: this.product.name,
            price: this.product.price,
            cost: this.product.cost,
            // Các thuộc tính khác...
          };
          
          this.$emit('product-created', newProduct);
          return { success: true, product: newProduct };
        } else {
          // Tạo thông báo thành công tùy chỉnh thay vì dùng confirm
          const successToast = document.createElement('div');
          successToast.innerHTML = `
            <div style="display: flex; flex-direction: column; gap: 10px;">
              <div style="font-size: 16px; font-weight: bold;">✅ THÊM SẢN PHẨM THÀNH CÔNG!</div>
              <div style="display: flex; gap: 10px; margin-top: 5px;">
                <button id="successYes" style="background: #28a745; color: white; border: none; padding: 8px 12px; border-radius: 4px; cursor: pointer; font-weight: bold;">Quay lại danh sách</button>
                <button id="successNo" style="background: #6c757d; color: white; border: none; padding: 8px 12px; border-radius: 4px; cursor: pointer; font-weight: bold;">Tiếp tục thêm</button>
              </div>
            </div>
          `;
          
          // Cải thiện style của toast
          successToast.style = `
            position: fixed; 
            top: 20px; 
            right: 20px; 
            background: #4CAF50; 
            color: white; 
            padding: 20px; 
            border-radius: 8px; 
            z-index: 99999; 
            box-shadow: 0 6px 16px rgba(0,0,0,0.3);
            min-width: 300px;
          `;
          
          document.body.appendChild(successToast);
          
          // Thêm âm thanh thông báo (tùy chọn)
          const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2869/2869-preview.mp3');
          audio.volume = 0.5;
          audio.play().catch(e => console.log('Không phát được âm thanh:', e));
          
          // Xử lý sự kiện nút
          document.getElementById('successYes').addEventListener('click', () => {
            if (document.body.contains(successToast)) {
              document.body.removeChild(successToast);
            }
            if (!this.isModal) {
              this.$router.push('/products');
            }
          });
          
          document.getElementById('successNo').addEventListener('click', () => {
            if (document.body.contains(successToast)) {
              document.body.removeChild(successToast);
            }
            // Reset form để thêm sản phẩm mới
            this.resetForm();
          });
          
          // Tự động ẩn sau 8 giây (thời gian dài hơn)
          setTimeout(() => {
            if (document.body.contains(successToast)) {
              document.body.removeChild(successToast);
            }
          }, 8000);
        }
        
        this.isSubmitting = false;
      } catch (err) {
        this.isSubmitting = false;
        console.error("Chi tiết lỗi:", err);
        
        // Xóa thông báo loading nếu còn tồn tại
        const existingLoadingToast = document.querySelector('div[style*="position: fixed"]');
        if (existingLoadingToast && document.body.contains(existingLoadingToast)) {
          document.body.removeChild(existingLoadingToast);
        }
        
        // Cải thiện thông báo lỗi
        const errorToast = document.createElement('div');
        errorToast.innerHTML = `
          <div style="font-weight: bold; margin-bottom: 5px;">⚠️ LỖI KHI THÊM SẢN PHẨM</div>
          <div>${err.response?.data?.title || err.message || "Lỗi không xác định"}</div>
        `;
        errorToast.style = `
          position: fixed; 
          top: 20px; 
          right: 20px; 
          background: #dc3545; 
          color: white; 
          padding: 20px; 
          border-radius: 8px; 
          z-index: 99999; 
          box-shadow: 0 6px 16px rgba(0,0,0,0.3);
          min-width: 300px;
        `;
        document.body.appendChild(errorToast);
        
        // Tự động ẩn sau 8 giây
        setTimeout(() => {
          if (document.body.contains(errorToast)) {
            document.body.removeChild(errorToast);
          }
        }, 8000);
        
        return { success: false };
      }
    },
    // Thêm phương thức resetForm để dùng khi tiếp tục thêm sản phẩm
    resetForm() {
      this.product = {
        barcode: "",
        name: "",
        price: 0,
        cost: 0,
        brand: "",
        categoryID: "",
        warehouseId: this.initialWarehouseId || "",
        status: "1",
        description: "",
        image: null,
        location: ""
      };
      this.imagePreview = null;
    }
  }
};
</script>

<style scoped>
.add-product-modal {
  background: #fff;
  border-radius: 18px;
  box-shadow: 0 8px 32px rgba(25, 118, 210, 0.13);
  max-width: 900px;
  margin: 48px auto;
  padding-bottom: 20px;
  position: relative;
  animation: fadeIn 0.3s;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(30px);
  }

  to {
    opacity: 1;
    transform: none;
  }
}

.add-product-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 28px 36px 0 36px;
  border-bottom: 1.5px solid #e3e8ee;
}

.add-product-header .title {
  font-size: 1.6rem;
  font-weight: 700;
  color: var(--primary-color, #1976d2);
  letter-spacing: -1px;
}

.close-btn {
  background: none;
  border: none;
  font-size: 2.2rem;
  color: #b0b0b0;
  cursor: pointer;
  transition: color 0.2s;
}

.close-btn:hover {
  color: var(--primary-color, #1976d2);
}

.add-product-form {
  padding: 28px 36px 0 36px;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px 32px;
}

.form-group {
  display: flex;
  align-items: center;
  min-height: 48px;
}

.form-group label {
  width: 120px;
  min-width: 120px;
  font-size: 15px;
  font-weight: 600;
  color: var(--primary-color, #1976d2);
  margin-bottom: 0;
  letter-spacing: -0.5px;
  text-align: right;
  padding-right: 12px;
}

.form-group input,
.form-group select,
.form-group textarea {
  flex: 1;
  padding: 9px 14px;
  border-radius: 8px;
  border: 1.5px solid var(--border-light, #e3e8ee);
  font-size: 16px;
  background: var(--bg-light, #f8fafc);
  color: var(--text-dark, #222);
  transition: border 0.2s, box-shadow 0.2s;
  outline: none;
  min-height: 38px;
}

.form-group input[type="file"] {
  padding: 0;
  background: none;
  border: none;
  flex: unset;
  margin-right: 10px;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  border: 1.5px solid var(--primary-color, #1976d2);
  background: var(--bg-white, #fff);
}

.full-width {
  grid-column: 1 / span 2;
  align-items: flex-start;
  flex-direction: column;
}

.full-width label {
  text-align: left;
  padding-right: 0;
  margin-bottom: 6px;
}

.description-row {
  display: flex;
  flex-direction: column;
  width: 100%;
  margin-bottom: 0;
  gap: 8px;
}

.desc-label {
  width: 120px;
  min-width: 120px;
  font-size: 15px;
  font-weight: 600;
  color: #1976d2;
  margin-bottom: 4px;
  text-align: left;
  padding-left: 2px;
}

.desc-image-flex {
  display: flex;
  flex-direction: row;
  gap: 18px;
  align-items: flex-start;
  width: 100%;
}

.desc-image-flex textarea {
  flex: 1 1 0;
  min-height: 90px;
  resize: vertical;
  padding: 10px 14px;
  border-radius: 8px;
  border: 1.5px solid #e3e8ee;
  font-size: 16px;
  background: #f8fafc;
  color: #222;
  transition: border 0.2s, box-shadow 0.2s;
  outline: none;
}

.desc-image-flex textarea:focus {
  border: 1.5px solid #1976d2;
  background: #fff;
}

.image-upload-area {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 80px;
  margin-top: 0;
  background: #f6fafd;
  border-radius: 8px;
  padding: 8px 6px 6px 6px;
  box-shadow: 0 1px 4px rgba(25, 118, 210, 0.04);
  border: 1px solid #e3e8ee;
}

.image-upload-group {
  display: flex;
  align-items: center;
  flex-direction: row;
}

.image-upload-group label {
  width: 120px;
  min-width: 120px;
  font-size: 15px;
  font-weight: 600;
  color: #1976d2;
  margin-bottom: 0;
  letter-spacing: -0.5px;
  text-align: right;
  padding-right: 12px;
  line-height: 38px;
}

.image-upload-inline {
  display: flex;
  align-items: center;
  gap: 10px;
}

.image-upload-inline input[type="file"] {
  width: 100px;
  font-size: 13px;
}

.image-preview-list {
  display: flex;
  gap: 6px;
  align-items: center;
}

.image-preview {
  width: 48px;
  height: 48px;
  object-fit: cover;
  border-radius: 7px;
  border: 1.5px solid #e3e8ee;
  background: #fafbfc;
  box-shadow: 0 1px 4px rgba(25, 118, 210, 0.07);
}

.full-width {
  grid-column: 1 / span 2;
  align-items: flex-start;
  flex-direction: column;
}

.full-width textarea {
  min-height: 90px;
  resize: vertical;
  padding: 10px 14px;
  border-radius: 8px;
  border: 1.5px solid #e3e8ee;
  font-size: 16px;
  background: #f8fafc;
  color: #222;
  transition: border 0.2s, box-shadow 0.2s;
  outline: none;
  width: 100%;
}

.form-actions {
  display: flex;
  gap: 22px;
  justify-content: flex-end;
  margin-top: 38px;
  padding-right: 2px;
}

.btn {
  background: #19c37d;
  border: none;
  color: #fff;
  padding: 0.8rem 2.1rem;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
  font-size: 18px;
  transition: background 0.18s, box-shadow 0.18s;
  box-shadow: 0 2px 8px rgba(25, 195, 125, 0.10);
  display: flex;
  align-items: center;
  gap: 9px;
}

.btn.green {
  background: var(--success-color, #19c37d);
}

.btn.green:hover {
  background: var(--success-dark, #13a76a);
}

.btn.gray {
  background: var(--text-light, #b0b0b0);
}

.btn.gray:hover {
  background: #888;
}

.icon-save:before {
  content: "💾";
}

.icon-cancel:before {
  content: "❌";
}

@media (max-width: 900px) {

  .add-product-modal,
  .add-product-form {
    padding: 12px !important;
  }

  .form-grid {
    grid-template-columns: 1fr;
    gap: 0;
  }

  .full-width {
    grid-column: 1;
  }

  .form-group label {
    width: 100px;
    min-width: 100px;
    text-align: left;
    padding-right: 0;
  }
}
</style>
