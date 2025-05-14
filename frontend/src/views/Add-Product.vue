<template>
  <div class="add-product-container">
    <h1>Thêm Sản Phẩm Mới</h1>

    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <div v-if="successMessage" class="success-message">
      {{ successMessage }}
    </div>

    <form @submit.prevent="submitForm" class="product-form">
      <div class="form-group">
        <label for="name">Tên sản phẩm</label>
        <input
          type="text"
          id="name"
          v-model="productForm.name"
          placeholder="Nhập tên sản phẩm"
          required
        />
      </div>

      <div class="form-group">
        <label for="description">Mô tả</label>
        <textarea
          id="description"
          v-model="productForm.description"
          placeholder="Nhập mô tả sản phẩm"
          rows="4"
        ></textarea>
      </div>

      <div class="form-group">
        <label for="quantity">Số lượng</label>
        <input
          type="number"
          id="quantity"
          v-model.number="productForm.quantity"
          min="0"
          placeholder="Nhập số lượng"
          required
        />
      </div>

      <div class="form-group">
        <label for="category">Danh mục</label>
        <input
          type="text"
          id="category"
          v-model="productForm.category"
          placeholder="Nhập danh mục"
          required
        />
      </div>

      <div class="form-actions">
        <button type="submit" class="submit-button" :disabled="isSubmitting">
          <span v-if="isSubmitting">Đang xử lý...</span>
          <span v-else>Thêm sản phẩm</span>
        </button>
        <button type="button" class="cancel-button" @click="goBack">
          Hủy bỏ
        </button>
      </div>
    </form>
  </div>
</template>

<script>
import productApi from "@/api/product.api";

export default {
  name: "AddProduct",
  data() {
    return {
      productForm: {
        name: "",
        description: "",
        quantity: 0,
        category: "",
      },
      isSubmitting: false,
      errorMessage: "",
      successMessage: "",
    };
  },
  methods: {
    async submitForm() {
      try {
        this.isSubmitting = true;
        this.errorMessage = "";
        this.successMessage = "";

        // Validate form
        if (!this.productForm.name || !this.productForm.category) {
          this.errorMessage = "Vui lòng nhập đầy đủ thông tin sản phẩm";
          return;
        }

        // Call API to create product
        await productApi.createProduct(this.productForm);

        // Show success message
        this.successMessage = "Thêm sản phẩm thành công!";

        // Reset form after successful submission
        this.resetForm();

        // Redirect to product list after short delay
        setTimeout(() => {
          this.$router.push("/");
        }, 2000);
      } catch (error) {
        console.error("Error creating product:", error);
        this.errorMessage =
          error.message || "Đã xảy ra lỗi khi thêm sản phẩm. Vui lòng thử lại.";
      } finally {
        this.isSubmitting = false;
      }
    },
    resetForm() {
      this.productForm = {
        name: "",
        description: "",
        quantity: 0,
        category: "",
      };
    },
    goBack() {
      this.$router.go(-1);
    },
  },
};
</script>

<style scoped>
.add-product-container {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem;
}

h1 {
  text-align: center;
  margin-bottom: 2rem;
  color: #333;
}

.error-message {
  background-color: #ffebee;
  color: #c62828;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.success-message {
  background-color: #e8f5e9;
  color: #2e7d32;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.product-form {
  background-color: #fff;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #555;
  font-weight: 500;
}

.form-group input,
.form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

.form-group input:focus,
.form-group textarea:focus {
  border-color: #4caf50;
  outline: none;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
}

.submit-button,
.cancel-button {
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.3s;
}

.submit-button {
  background-color: #4caf50;
  color: white;
  border: none;
  flex: 1;
}

.submit-button:hover {
  background-color: #45a049;
}

.submit-button:disabled {
  background-color: #a5d6a7;
  cursor: not-allowed;
}

.cancel-button {
  background-color: #f5f5f5;
  color: #333;
  border: 1px solid #ddd;
}

.cancel-button:hover {
  background-color: #e0e0e0;
}
</style>
