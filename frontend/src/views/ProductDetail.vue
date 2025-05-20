<template>
    <div class="add-product-modal" v-if="product">
        <form class="add-product-form" @submit.prevent="editMode ? updateProduct() : null">
            <div class="form-grid">
                <!-- Ảnh sản phẩm bên trái -->
                <div class="form-group image-upload-group" style="align-items:flex-start;">
                    <div class="product-detail-image-block">
                        <img
                            :src="product.image || defaultImg"
                            alt="Ảnh sản phẩm"
                            class="product-detail-image"
                        />
                    </div>
                </div>
                <!-- Thông tin sản phẩm bên phải -->
                <div class="form-group product-info-cols" style="grid-column:2/4;">
                    <div class="info-col">
                        <div class="info-row"><span>Mã hàng:</span> <b>{{ product.id }}</b></div>
                        <div class="info-row"><span>Mã vạch:</span> <b>{{ product.barcode }}</b></div>
                        <div class="info-row"><span>Tên hàng:</span> <b>{{ product.name }}</b></div>
                        <div class="info-row"><span>Nhóm hàng:</span> <b>{{ product.categoryID }}</b></div>
                        <div class="info-row"><span>Thương hiệu:</span> <b>{{ product.brand }}</b></div>
                        <div class="info-row"><span>Vị trí:</span> <b>{{ product.location }}</b></div>
                        <div class="info-row"><span>Số lượng:</span> <b>{{ product.num }}</b></div>
                    </div>
                    <div class="info-col">
                        <div class="info-row"><span>Giá bán:</span> <b>{{ formatCurrency(product.price) }}</b></div>
                        <div class="info-row"><span>Giá vốn:</span> <b>{{ formatCurrency(product.cost) }}</b></div>
                        <div class="info-row"><span>Kho:</span> <b>{{ product.warehouseId }}</b></div>
                        <div class="info-row"><span>Trạng thái:</span>
                            <b>
                                <span v-if="product.status == 1" style="color:#19c37d;">Bán trực tiếp</span>
                                <span v-else style="color:#e74c3c;">Ngừng bán</span>
                            </b>
                        </div>
                        <div class="info-row"><span>Ngày tạo:</span> <b>{{ product.createdDate ?
                            formatDate(product.createdDate) : "—" }}</b></div>
                        <div class="info-row"><span>Ngày cập nhật:</span> <b>{{ product.finaldDate ?
                            formatDate(product.finaldDate) : "—" }}</b></div>
                    </div>
                </div>
                <!-- Mô tả và ghi chú chiếm 2 cột -->
                <div class="form-group full-width">
                    <label>Mô tả</label>
                    <div class="desc-box">{{ product.description || "—" }}</div>
                </div>
            </div>
            <div class="form-actions">
                <button v-if="!editMode" type="button" class="btn green" @click="editMode = true"><i
                        class="icon-save"></i> Cập nhật</button>
                <button v-if="editMode" type="submit" class="btn green"><i class="icon-save"></i> Lưu</button>
                <button v-if="editMode" type="button" class="btn gray" @click="cancelEdit">Bỏ qua</button>
                <button v-if="product.status == 1" type="button" class="btn red" @click="confirmChangeStatus">
                    <i class="icon-ban"></i> Ngừng bán
                </button>
                <button v-else type="button" class="btn green" @click="confirmReactivate">
                    <i class="icon-check"></i> Kích hoạt lại
                </button>
                <button type="button" class="btn" @click="$router.back()">Quay lại</button>
                <button type="button" class="btn blue" @click="findInactiveProducts">
                    <i class="icon-search"></i> Tìm SP ngừng bán
                </button>
            </div>
        </form>

        <!-- Form cập nhật (nếu editMode) -->
        <div v-if="editMode" class="edit-modal">
            <div class="add-product-modal">
                <div class="add-product-header">
                    <span class="title">Cập nhật sản phẩm</span>
                    <button class="close-btn" @click="cancelEdit">×</button>
                </div>
                <form class="add-product-form" @submit.prevent="updateProduct">
                    <div class="form-grid-2col">
                        <!-- Cột trái -->
                        <div>
                            <div class="form-group">
                                <label>Mã vạch</label>
                                <input type="text" v-model="editProduct.barcode" required />
                            </div>
                            <div class="form-group">
                                <label>Tên sản phẩm</label>
                                <input type="text" v-model="editProduct.name" required />
                            </div>
                            <div class="form-group">
                                <label>Loại hàng</label>
                                <select v-model="editProduct.categoryID" required>
                                    <option value="">---Chọn loại---</option>
                                    <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}
                                    </option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label>Thương hiệu</label>
                                <input type="text" v-model="editProduct.brand" />
                            </div>
                            <div class="form-group">
                                <label>Vị trí</label>
                                <input type="text" v-model="editProduct.location" />
                            </div>
                            <div class="form-group full-width">
                                <label>Mô tả</label>
                                <textarea v-model="editProduct.description"
                                    placeholder="Nhập mô tả sản phẩm..."></textarea>
                            </div>
                        </div>
                        <!-- Cột phải -->
                        <div>
                            <div class="form-group">
                                <label>Giá vốn</label>
                                <input type="number" v-model.number="editProduct.cost" min="0" required />
                            </div>
                            <div class="form-group">
                                <label>Giá bán</label>
                                <input type="number" v-model.number="editProduct.price" min="0" required />
                            </div>
                            <div class="form-group">
                                <label>Trạng thái</label>
                                <select v-model="editProduct.status" required>
                                    <option value="">---Chọn trạng thái---</option>
                                    <option :value="1">Đang bán</option>
                                    <option :value="0">Ngừng bán</option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label>Kho</label>
                                <select v-model="editProduct.warehouseId" required>
                                    <option value="">---Chọn kho---</option>
                                    <option v-for="wh in warehouses" :key="wh.id" :value="wh.id">{{ wh.name }}</option>
                                </select>
                            </div>
                            <div class="form-group">
                                <label>Ảnh</label>
                                <input type="file" @change="onImageChange" accept="image/*" />
                                <div class="image-preview-list">
                                    <img v-if="imagePreview" :src="imagePreview" class="image-preview" />
                                    <img v-else-if="editProduct.image && typeof editProduct.image === 'string'"
                                        :src="editProduct.image" class="image-preview" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label>Số lượng</label>
                                <input type="number" v-model.number="editProduct.num" min="0" required />
                            </div>
                        </div>
                    </div>
                    <div class="form-actions">
                        <button type="submit" class="btn green"><i class="icon-save"></i> Lưu</button>
                        <button type="button" class="btn gray" @click="cancelEdit"><i class="icon-cancel"></i> Bỏ
                            qua</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
    <div v-else class="loading">Đang tải dữ liệu...</div>
</template>

<script>
import axios from "axios";
const defaultImg = "https://via.placeholder.com/180x180?text=IMG";
export default {
    name: "ProductDetail",
    data() {
        return {
            product: null,
            defaultImg,
            editMode: false,
            editProduct: {},
            imagePreview: null,
            categories: [],
            warehouses: [],
        };
    },
    methods: {
        onImageChange(e) {
            const file = e.target.files[0];
            if (file) {
                this.editProduct.image = file;
                this.imagePreview = URL.createObjectURL(file);
            }
        },
        async fetchProduct() {
            try {
                const id = this.$route.params.id;
                const res = await axios.get(`https://localhost:7189/api/Products/detail/${id}`);
                // Hỗ trợ cả 2 kiểu trả về: object trực tiếp hoặc { result: { data: {...} } }
                let raw = res.data;
                if (res.data?.result?.data) {
                    raw = res.data.result.data;
                }
                const data = {
                    id: raw.id ?? "",
                    name: raw.name ?? "",
                    price: raw.price ?? 0,
                    barcode: raw.barcode ?? "",
                    status: raw.status ?? "",
                    warehouseId: raw.warehouseId ?? "",
                    cost: raw.cost ?? 0,
                    brand: raw.brand ?? "",
                    categoryID: raw.categoryID ?? "",
                    description: raw.description ?? "",
                    image: raw.image ?? "",
                    location: raw.location ?? "",
                    finaldDate: raw.finaldDate ?? "",
                    createdDate: raw.createdDate ?? "",
                    num: raw.num ?? "",

                };
                this.product = data;
                this.editProduct = { ...data };
                this.imagePreview = typeof data.image === "string" && data.image ? data.image : null;
            } catch (err) {
                alert("Không tìm thấy sản phẩm!");
                this.$router.back();
            }
        },
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
            // Đổi API cho đúng backend của bạn
            const res = await axios.get("https://localhost:7189/api/warehouse");
            // Lấy đúng mảng kho hàng từ response mới
            this.warehouses = res.data?.result?.data || [];
        },
        formatCurrency(val) {
            if (!val) return "0";
            return Number(val).toLocaleString("vi-VN");
        },
        formatDate(val) {
            if (!val) return "—";
            const d = new Date(val);
            return d.toLocaleDateString("vi-VN") + " " + d.toLocaleTimeString("vi-VN");
        },
        async updateProduct() {
            try {
                const formData = new FormData();
                formData.append("Id", String(this.editProduct.id));
                formData.append("Barcode", this.editProduct.barcode);
                formData.append("Name", this.editProduct.name);
                formData.append("Price", String(this.editProduct.price));
                formData.append("Cost", String(this.editProduct.cost));
                formData.append("Brand", this.editProduct.brand);
                formData.append("CategoryID", String(this.editProduct.categoryID));
                formData.append("WarehouseId", String(this.editProduct.warehouseId));
                formData.append("Status", String(this.editProduct.status));
                formData.append("Description", this.editProduct.description);
                formData.append("location", this.editProduct.location);
                formData.append("num", String(this.editProduct.num));


                // Chỉ gửi Image nếu là File
                if (this.editProduct.image instanceof File) {
                    formData.append("Image", this.editProduct.image);
                }

                await axios.put(
                    `https://localhost:7189/api/Products/update/${this.editProduct.id}`,
                    formData,
                    { headers: { "Content-Type": "multipart/form-data" } }
                );
                this.product = { ...this.editProduct };
                this.editMode = false;
                alert("Cập nhật thành công!");
            } catch (err) {
                alert("Lỗi khi cập nhật!");
                if (err.response?.data?.errors) {
                    console.error("Validation errors:", err.response.data.errors);
                } else {
                    console.error(err);
                }
            }
        },
        async confirmChangeStatus() {
            if (confirm("Bạn có chắc chắn muốn ngừng bán sản phẩm này?\n\nSản phẩm sẽ được lưu trong hệ thống và có thể kích hoạt lại sau.")) {
                try {
                    // Thay vì gọi DELETE, tạo formData để cập nhật trạng thái
                    const formData = new FormData();
                    formData.append("Id", String(this.product.id));
                    formData.append("Barcode", this.product.barcode);
                    formData.append("Name", this.product.name);
                    formData.append("Price", String(this.product.price));
                    formData.append("Cost", String(this.product.cost));
                    formData.append("Brand", this.product.brand || "");
                    formData.append("CategoryID", String(this.product.categoryID));
                    formData.append("WarehouseId", String(this.product.warehouseId));
                    formData.append("Status", "0"); // Đặt trạng thái ngừng bán
                    formData.append("Description", this.product.description || "");
                    formData.append("location", this.product.location || "");
                    formData.append("num", String(this.product.num));
                    
                    // Nếu có ảnh, thêm vào request
                    if (this.product.image && typeof this.product.image === 'string') {
                        formData.append("ImagePath", this.product.image);
                    }
                    
                    // Gọi API UPDATE thay vì DELETE
                    await axios.put(
                        `https://localhost:7189/api/Products/update/${this.product.id}`,
                        formData,
                        { headers: { "Content-Type": "multipart/form-data" } }
                    );
                    
                    // Cập nhật trạng thái sản phẩm trong UI
                    this.product.status = 0;
                    this.editProduct.status = 0;
                    
                    alert("Đã chuyển sản phẩm sang trạng thái ngừng bán. Sản phẩm vẫn được lưu trong hệ thống.");
                } catch (err) {
                    console.error(err);
                    alert("Lỗi khi cập nhật trạng thái sản phẩm!");
                }
            }
        },

        async confirmReactivate() {
            if (confirm("Bạn có chắc chắn muốn kích hoạt lại sản phẩm này?")) {
                try {
                    // Tạo formData để gửi request
                    const formData = new FormData();
                    formData.append("Id", String(this.product.id));
                    formData.append("Barcode", this.product.barcode);
                    formData.append("Name", this.product.name);
                    formData.append("Price", String(this.product.price));
                    formData.append("Cost", String(this.product.cost));
                    formData.append("Brand", this.product.brand || "");
                    formData.append("CategoryID", String(this.product.categoryID));
                    formData.append("WarehouseId", String(this.product.warehouseId));
                    formData.append("Status", "1"); // Đặt trạng thái về 1 (đang bán)
                    formData.append("Description", this.product.description || "");
                    formData.append("location", this.product.location || "");
                    formData.append("num", String(this.product.num));
                    
                    // Nếu có ảnh, thêm vào request
                    if (this.product.image && typeof this.product.image === 'string') {
                        formData.append("ImagePath", this.product.image);
                    }
                    
                    // Gọi API update
                    await axios.put(
                        `https://localhost:7189/api/Products/update/${this.product.id}`,
                        formData,
                        { headers: { "Content-Type": "multipart/form-data" } }
                    );
                    
                    // Cập nhật trạng thái trong UI
                    this.product.status = 1;
                    this.editProduct.status = 1;
                    
                    this.showToastMessage("Đã kích hoạt lại sản phẩm thành công!");
                } catch (err) {
                    console.error(err);
                    this.showToastMessage("Lỗi khi kích hoạt lại sản phẩm!", true);
                }
            }
        },

        showToastMessage(message, isError = false) {
            // Nếu bạn đã có một hệ thống toast, sử dụng nó thay vì alert
            alert(message);
        },
        cancelEdit() {
            this.editMode = false;
            this.editProduct = { ...this.product };
            this.imagePreview = this.product.image || null;
        },
        async findInactiveProducts() {
            try {
                // Gọi API để tìm tất cả sản phẩm bao gồm sản phẩm ngừng bán
                const response = await axios.get('https://localhost:7189/api/Products/list', {
                    params: { 
                        includeInactive: true // Thêm tham số để lấy cả sản phẩm ngừng bán
                    }
                });
                
                // Phân tích response để lấy danh sách sản phẩm
                let products = [];
                if (Array.isArray(response.data)) {
                    products = response.data;
                } else if (response.data.data && Array.isArray(response.data.data)) {
                    products = response.data.data;
                } else if (response.data.result && Array.isArray(response.data.result)) {
                    products = response.data.result;
                }
                
                // Lọc để tìm sản phẩm có status = 0
                const inactiveProducts = products.filter(p => p.status == 0);
                
                if (inactiveProducts.length > 0) {
                    alert(`Đã tìm thấy ${inactiveProducts.length} sản phẩm đã ngừng bán trong hệ thống.`);
                    console.log('Danh sách sản phẩm ngừng bán:', inactiveProducts);
                } else {
                    alert('Không tìm thấy sản phẩm nào đã ngừng bán.');
                }
            } catch (error) {
                console.error('Lỗi khi tìm kiếm sản phẩm ngừng bán:', error);
                alert('Không thể tìm kiếm sản phẩm ngừng bán. Vui lòng thử lại sau.');
            }
        }
    },
    mounted() {
        this.fetchProduct();
        this.fetchCategories();
        this.fetchWarehouses();
    },
};
</script>

<style scoped>
.add-product-modal {
  background: #fff;
  border-radius: 18px;
  box-shadow: 0 8px 32px rgba(25, 118, 210, 0.13);
  max-width: 1280px;
  margin: 36px auto;
  padding: 36px 40px 32px 40px;
  position: relative;
  animation: fadeIn 0.3s;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(30px);}
  to { opacity: 1; transform: none;}
}

/* Bố cục chi tiết sản phẩm */
.form-grid {
  display: grid;
  grid-template-columns: 440px 1.2fr 1fr;
  gap: 36px 40px;
  align-items: flex-start;
  margin-bottom: 32px;
}

.image-upload-group {
  grid-row: 1 / span 3;
  background: none;
  border: none;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  min-width: 100%;
  min-height: 100%;
  max-width: 100%;
  max-height: 100%;
  margin-bottom: 0;
  padding: 0;
}

.product-detail-image-block {
  background: #f8fafc;
  border-radius: 16px;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 410px;
  min-width: 410px;
  max-width: 410px;
  max-height: 410px;
  box-shadow: 0 4px 32px rgba(25, 118, 210, 0.10);
}

.product-detail-image {
  width: 100%;
  max-width: 390px;
  aspect-ratio: 1/1;
  object-fit: cover;
  border-radius: 14px;
  background: #fff;
  border: 2.5px solid #e3e8ee;
  box-shadow: 0 2px 12px rgba(25, 118, 210, 0.08);
  display: block;
  margin: 0 auto;
}

.product-info-cols {
  display: flex;
  gap: 36px;
  width: 100%;
}
.info-col {
  flex: 1 1 0;
  display: flex;
  flex-direction: column;
  gap: 0;
}
.info-row {
  display: flex;
  align-items: center;
  min-height: 38px;
  font-size: 16px;
  color: #222;
  gap: 10px;
  border-bottom: 1px solid #f0f0f0;
  padding: 7px 0;
}
.info-row span {
  width: 120px;
  min-width: 120px;
  color: var(--primary-color, #1976d2);
  font-weight: 600;
  font-size: 15px;
  text-align: left;
  padding-right: 12px;
  display: inline-block;
}
.info-row b {
  font-weight: 500;
  color: #222;
  font-size: 16px;
  display: inline-block;
  text-align: left;
}

.full-width {
  grid-column: 2 / span 2;
  margin-top: 0;
  margin-bottom: 0;
}
.full-width label {
  font-size: 16px;
  font-weight: 700;
  color: #1976d2;
  margin-bottom: 8px;
  display: block;
  text-align: left;
  padding-left: 2px;
  letter-spacing: 0.2px;
}
.desc-box {
  min-height: 90px;
  background: #f8fafc;
  border-radius: 10px;
  border: 1.5px solid #e3e8ee;
  padding: 14px 18px;
  font-size: 16px;
  color: #222;
  margin-top: 4px;
  white-space: pre-line;
  box-shadow: 0 1px 6px rgba(25, 118, 210, 0.04);
}

/* Nút hành động */
.form-actions {
  display: flex;
  gap: 22px;
  align-items: center;
  margin-top: 36px;
  justify-content: flex-start;
}
.btn {
  background: #19c37d;
  border: none;
  color: #fff;
  padding: 0.85rem 2.2rem;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
  font-size: 18px;
  transition: background 0.18s, box-shadow 0.18s;
  box-shadow: 0 2px 8px rgba(25,195,125,0.10);
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
  background: #b0b0b0;
}
.btn.gray:hover {
  background: #888;
}
.btn.red {
  background: var(--danger-color, #e74c3c);
}
.btn.red:hover {
  background: var(--danger-dark, #c0392b);
}
.btn.blue {
  background: var(--primary-color, #1976d2);
}
.btn.blue:hover {
  background: var(--primary-dark, #1565c0);
}
.icon-save:before {
  content: "💾";
}
.icon-cancel:before {
  content: "❌";
}
.icon-delete:before {
  content: "🗑️";
}
.icon-search:before {
  content: "🔍";
}
.loading {
  text-align: center;
  margin-top: 80px;
  font-size: 20px;
  color: #888;
}

/* Edit modal (update) dùng lại style của ProductAdd.vue */
.edit-modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.18);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.edit-modal .add-product-modal {
  background: #fff;
  border-radius: 18px;
  box-shadow: 0 8px 32px rgba(25, 118, 210, 0.13);
  max-width: 1000px;
  width: 95vw;
  margin: 0;
  padding-bottom: 20px;
  position: relative;
  animation: fadeIn 0.3s;
  overflow: auto;
}
.edit-modal .add-product-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 28px 36px 0 36px;
  border-bottom: 1.5px solid #e3e8ee;
}
.edit-modal .add-product-header .title {
  font-size: 1.6rem;
  font-weight: 700;
  color: var(--primary-color, #1976d2);
  letter-spacing: -1px;
}
.edit-modal .close-btn {
  background: none;
  border: none;
  font-size: 2.2rem;
  color: #b0b0b0;
  cursor: pointer;
  transition: color 0.2s;
}
.edit-modal .close-btn:hover {
  color: #1976d2;
}
.edit-modal .add-product-form {
  padding: 24px 18px 0 18px;
}
.edit-modal .form-grid-2col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px 24px;
}
.edit-modal .form-group {
  display: flex;
  align-items: center;
  min-height: 54px;
  margin-bottom: 0;
}
.edit-modal .form-group label {
  width: 120px;
  min-width: 120px;
  font-size: 15px;
  font-weight: 600;
  color: #1976d2;
  margin-bottom: 0;
  letter-spacing: -0.5px;
  text-align: left;
  padding-right: 12px;
}
.edit-modal .form-group input,
.edit-modal .form-group select,
.edit-modal .form-group textarea {
  flex: 1 1 0%;
  min-width: 0;
  max-width: 100%;
  width: 100%;
  box-sizing: border-box;
  padding: 10px 14px;
  border-radius: 8px;
  border: 1.5px solid #e3e8ee;
  font-size: 16px;
  background: #f8fafc;
  color: #222;
  transition: border 0.2s, box-shadow 0.2s;
  outline: none;
  margin-left: 0;
}
.edit-modal .form-group textarea {
  resize: vertical;
}
.edit-modal .form-group.full-width {
  grid-column: 1 / span 2;
  align-items: flex-start;
  flex-direction: column;
}
.edit-modal .form-group.full-width label {
  padding-left: 0;
  padding-right: 0;
  margin-bottom: 6px;
  width: auto;
  min-width: 0;
  text-align: left;
}
@media (max-width: 900px) {
  .edit-modal .form-grid-2col {
    grid-template-columns: 1fr;
    gap: 12px 0;
  }
  .edit-modal .form-group label {
    width: 100px;
    min-width: 100px;
    padding-right: 8px;
  }
}
.edit-modal .image-upload-group {
  display: flex;
  align-items: center;
  flex-direction: row;
}
.edit-modal .image-upload-group label {
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
.edit-modal .image-upload-inline {
  display: flex;
  align-items: center;
  gap: 10px;
}
.edit-modal .image-preview-list {
  display: flex;
  gap: 6px;
  align-items: center;
  margin-top: 8px;
}
.edit-modal .image-preview {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 8px;
  border: 1.5px solid #e3e8ee;
  background: #fafbfc;
}
.edit-modal .full-width {
  grid-column: 1 / span 2;
  align-items: flex-start;
  flex-direction: column;
}
.edit-modal .full-width label {
  text-align: left;
  padding-right: 0;
  margin-bottom: 6px;
}
.edit-modal .form-actions {
  display: flex;
  gap: 22px;
  justify-content: flex-end;
  margin-top: 38px;
  padding-right: 2px;
}

/* Responsive */
@media (max-width: 1100px) {
  .add-product-modal,
  .add-product-form,
  .edit-modal .add-product-modal,
  .edit-modal .add-product-form {
    padding: 12px !important;
  }
  .form-grid {
    grid-template-columns: 1fr;
    gap: 18px 0;
  }
  .product-detail-image-block {
    min-width: 100%;
    max-width: 100%;
    min-height: 220px;
    max-height: 220px;
  }
  .product-detail-image {
    width: 100%;
    max-width: 98vw;
    height: auto;
    aspect-ratio: 1/1;
  }
  .form-actions {
    flex-wrap: wrap;
    gap: 12px;
  }
}

/* Status colors */
span[style*="color:#19c37d"] {
  color: var(--success-color, #19c37d) !important;
}

span[style*="color:#e74c3c"] {
  color: var(--danger-color, #e74c3c) !important;
}
</style>