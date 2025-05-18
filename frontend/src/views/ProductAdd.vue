<template>
  <div class="add-product-wrapper">
    <div class="add-product-header">
      <span class="title">Thêm sản phẩm</span>
      <button class="close-btn" @click="$router.back()">×</button>
    </div>
    <form class="add-product-form" @submit.prevent="submitForm">
      <div class="form-main">
        <div class="form-col">
          <div class="form-group">
            <label>Mã vạch</label>
            <input type="text" v-model="product.barcode" required />
          </div>
          <div class="form-group">
            <label>Tên sản phẩm</label>
            <input type="text" v-model="product.name" required />
          </div>
          <div class="form-group">
            <label>Giá bán</label>
            <input type="number" v-model.number="product.price" min="0" required />
          </div>
          <div class="form-group">
            <label>Giá vốn</label>
            <input type="number" v-model.number="product.cost" min="0" required />
          </div>
          <div class="form-group">
            <label>Loại hàng</label>
            <select v-model.number="product.categoryID" required>
              <option value="">---Chọn loại---</option>
              <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
            </select>
          </div>
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
          <div class="form-group">
            <label>Trạng thái</label>
            <select v-model.number="product.status" required>
              <option value="">---Chọn trạng thái---</option>
              <option :value="1">Đang bán</option>
              <option :value="0">Ngừng bán</option>
            </select>
          </div>
          <div class="form-group">
            <label>Vị trí</label>
            <input type="text" v-model="product.location" />
          </div>
          <div class="form-group">
            <label>Ảnh sản phẩm</label>
            <input type="file" @change="onImageChange" accept="image/*" />
            <img v-if="imagePreview" :src="imagePreview" style="width:60px;height:60px;margin-left:10px;" />
          </div>
          <div class="form-group">
            <label>Mô tả</label>
            <textarea v-model="product.description"></textarea>
          </div>
        </div>
      </div>
      <div class="form-actions">
        <button type="submit" class="btn green"><i class="icon-save"></i> Lưu</button>
        <button type="button" class="btn gray" @click="$router.back()"><i class="icon-cancel"></i> Bỏ qua</button>
      </div>
    </form>
  </div>
</template>

<script>
import axios from "axios";
export default {
  name: "ProductAdd",
  data() {
    return {
      product: {
        barcode: "",
        name: "",
        price: 0,
        cost: 0,
        brand: "",
        categoryID: "",
        warehouseId: "",
        status: "",
        description: "",
        image: null,
        location: ""
      },
      categories: [],
      warehouses: [],
      imagePreview: null
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
        const res = await axios.get("http://localhost:7189/api/warehouses");
        this.warehouses = res.data;
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
    async submitForm() {
      try {
        const formData = new FormData();
        formData.append("Barcode", this.product.barcode);
        formData.append("Name", this.product.name);
        formData.append("Price", String(this.product.price));
        formData.append("Cost", String(this.product.cost));
        formData.append("Brand", this.product.brand);
        formData.append("CategoryID", String(this.product.categoryID));
        formData.append("WarehouseId", String(this.product.warehouseId));
        formData.append("Status", String(this.product.status));
        formData.append("Description", this.product.description);
        formData.append("location", this.product.location);
        if (this.product.image) {
          formData.append("Image", this.product.image);
        }

        await axios.post("https://localhost:7189/api/Products/create", formData, {
          headers: { "Content-Type": "multipart/form-data" }
        });

        alert("Đã thêm sản phẩm!");
        this.$router.back();
      } catch (err) {
        alert("Lỗi khi thêm sản phẩm!");
        if (err.response?.data?.errors) {
          console.error("Validation errors:", err.response.data.errors);
        } else {
          console.error(err);
        }
      }
    }
  }
};
</script>

<style scoped>
.add-product-wrapper {
  background: #f4f6fa;
  min-height: 100vh;
  padding: 0;
}
.add-product-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 28px 32px 0 32px;
}
.add-product-header .title {
  font-size: 1.6rem;
  font-weight: 700;
  color: #222;
}
.close-btn {
  background: none;
  border: none;
  font-size: 2rem;
  color: #888;
  cursor: pointer;
}
.add-product-form {
  background: #fff;
  margin: 0 32px 32px 32px;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(10,116,230,0.07);
  padding: 32px;
}
.form-main {
  display: flex;
  gap: 32px;
}
.form-col {
  flex: 1;
  min-width: 320px;
}
.form-group {
  display: flex;
  align-items: center;
  margin-bottom: 18px;
  gap: 10px;
}
.form-group label {
  min-width: 110px;
  font-weight: 600;
  color: #222;
}
.form-group input,
.form-group select,
.form-group textarea {
  flex: 1;
  padding: 7px 12px;
  border-radius: 7px;
  border: 1px solid #ddd;
  font-size: 16px;
  background: #f8fafc;
  color: #222;
}
.form-actions {
  display: flex;
  gap: 18px;
  justify-content: flex-end;
  margin-top: 32px;
}
.btn {
  background: #19c37d;
  border: none;
  color: #fff;
  padding: 0.7rem 1.7rem;
  border-radius: 9px;
  cursor: pointer;
  font-weight: 700;
  font-size: 17px;
  transition: background 0.2s;
  box-shadow: 0 2px 8px rgba(25,195,125,0.08);
  display: flex;
  align-items: center;
  gap: 7px;
}
.btn.gray {
  background: #888;
}
.icon-save:before {
  content: "💾";
}
.icon-cancel:before {
  content: "❌";
}
</style>
