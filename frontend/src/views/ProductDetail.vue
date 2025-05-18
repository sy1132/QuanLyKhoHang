<template>
    <div class="product-detail-wrapper" v-if="product">
        <div class="detail-header">
            <h2>{{ product.name }}</h2>
            <button class="btn" @click="$router.back()">Quay lại</button>
        </div>
        <div class="detail-tabs">
            <button class="tab active">Thông tin</button>
            <button class="tab">Thẻ kho</button>
            <button class="tab">Tồn kho</button>
        </div>
        <div class="detail-content">
            <div class="detail-image">
                <img :src="product.image || defaultImg" alt="Ảnh sản phẩm" />
            </div>
            <div class="detail-info">
                <div class="info-row"><span>Mã hàng:</span> <b>{{ product.id }}</b></div>
                <div class="info-row"><span>Mã vạch:</span> <b>{{ product.barcode }}</b></div>
                <div class="info-row"><span>Nhóm hàng:</span> <b>{{ product.categoryID }}</b></div>
                <div class="info-row"><span>Loại hàng:</span> <b>Hàng hóa</b></div>
                <div class="info-row"><span>Thương hiệu:</span> <b>{{ product.brand }}</b></div>
                <div class="info-row"><span>Giá bán:</span> <b>{{ formatCurrency(product.price) }}</b></div>
                <div class="info-row"><span>Giá vốn:</span> <b>{{ formatCurrency(product.cost) }}</b></div>
                <div class="info-row"><span>Vị trí:</span> <b>{{ product.location }}</b></div>
            </div>
            <div class="detail-actions">
                <button class="btn green" @click="editMode = true"><i class="icon-save"></i> Cập nhật</button>
                <button class="btn red" @click="confirmDelete"><i class="icon-delete"></i> Xóa</button>
            </div>
        </div>
        <div class="detail-desc">
            <div>
                <div><b>Mô tả</b></div>
                <div class="desc-box">{{ product.description || "—" }}</div>
            </div>
        </div>

        <!-- Form cập nhật -->
        <div v-if="editMode" class="edit-modal">
            <div class="edit-form update-grid">
                <h3 style="grid-column:1/3;">Sửa hàng</h3>
                <!-- Cột trái -->
                <div class="update-col">
                    <label>Mã hàng
                        <input v-model="editProduct.id" disabled />
                    </label>
                    <label>Mã vạch
                        <input v-model="editProduct.barcode" />
                    </label>
                    <label>Tên hàng
                        <input v-model="editProduct.name" />
                    </label>
                    <label>Nhóm hàng
                        <input v-model="editProduct.categoryID" />
                    </label>
                    <label>Thương hiệu
                        <input v-model="editProduct.brand" />
                    </label>
                    <label>Vị trí
                        <input v-model="editProduct.location" />
                    </label>
                    <label>Kho
                        <input v-model="editProduct.warehouseId" />
                    </label>
                    <label>Trạng thái
                        <select v-model="editProduct.status">
                            <option value="1">Bán trực tiếp</option>
                            <option value="0">Không bán</option>
                        </select>
                    </label>
                    <label>Mô tả
                        <textarea v-model="editProduct.description"></textarea>
                    </label>
                </div>
                <!-- Cột phải -->
                <div class="update-col">
                    <label>Giá vốn
                        <input v-model.number="editProduct.cost" type="number" />
                    </label>
                    <label>Giá bán
                        <input v-model.number="editProduct.price" type="number" />
                    </label>
                    <label>Ảnh sản phẩm
                        <input type="file" @change="onImageChange" />
                        <img v-if="imagePreview" :src="imagePreview" alt="Preview"
                            style="max-width:80px;max-height:80px;margin-top:6px;" />
                        <img v-else-if="editProduct.image && typeof editProduct.image === 'string'"
                            :src="editProduct.image" alt="Ảnh cũ"
                            style="max-width:80px;max-height:80px;margin-top:6px;" />
                    </label>
                </div>
                <div class="edit-actions" style="grid-column:1/3;">
                    <button class="btn green" @click="updateProduct">Lưu</button>
                    <button class="btn gray" @click="editMode = false">Bỏ qua</button>
                </div>
            </div>
        </div>
    </div>
    <div v-else class="loading">Đang tải dữ liệu...</div>
</template>

<script>
import axios from "axios";
const defaultImg = "https://via.placeholder.com/120x120?text=IMG";
export default {
    name: "ProductDetail",
    data() {
        return {
            product: null,
            defaultImg,
            editMode: false,
            editProduct: {},
            imagePreview: null,
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
                this.product = res.data;
                this.editProduct = { ...res.data }; // clone for editing
                this.imagePreview = typeof res.data.image === 'string' ? res.data.image : null;
            } catch (err) {
                alert("Không tìm thấy sản phẩm!");
                this.$router.back();
            }
        },
        formatCurrency(val) {
            if (!val) return "0";
            return Number(val).toLocaleString("vi-VN");
        },
        async updateProduct() {
            try {
                const formData = new FormData();
                formData.append("Id", this.editProduct.id);
                formData.append("Barcode", this.editProduct.barcode ?? "");
                formData.append("Name", this.editProduct.name ?? "");
                formData.append("CategoryID", this.editProduct.categoryID ?? "");
                formData.append("Brand", this.editProduct.brand ?? "");
                formData.append("Cost", this.editProduct.cost ?? "");
                formData.append("Price", this.editProduct.price ?? "");
                formData.append("Description", this.editProduct.description ?? "");
                formData.append("Status", String(this.editProduct.status ?? ""));
                formData.append("WarehouseId", this.editProduct.warehouseId ?? "");
                formData.append("location", this.editProduct.location ?? "");

                // Chỉ gửi ảnh nếu là file mới
                if (this.editProduct.image instanceof File) {
                    formData.append("Image", this.editProduct.image);
                }

                // Debug thông tin gửi đi (có thể bỏ sau khi test xong)
                // console.log([...formData.entries()]);

                await axios.put(
                    `https://localhost:7189/api/Products/update/${this.editProduct.id}`,
                    formData,
                    { headers: { "Content-Type": "multipart/form-data" } }
                );

                this.product = { ...this.editProduct };
                this.editMode = false;
                alert("Cập nhật thành công!");
            } catch (err) {
                console.error(err);
                alert("Lỗi khi cập nhật!");
            }
        },

        async confirmDelete() {
            if (confirm("Bạn có chắc chắn muốn xóa sản phẩm này?")) {
                try {
                    await axios.delete(`https://localhost:7189/api/Products/delete/${this.product.id}`);
                    alert("Đã xóa sản phẩm!");
                    this.$router.back();
                } catch {
                    alert("Lỗi khi xóa sản phẩm!");
                }
            }
        },
    },
    mounted() {
        this.fetchProduct();
    },
};
</script>

<style scoped>
.product-detail-wrapper {
    max-width: 1100px;
    margin: 30px auto;
    background: #fff;
    border-radius: 10px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.07);
    padding: 24px 32px 32px 32px;
}

.detail-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 8px;
}

.detail-tabs {
    display: flex;
    gap: 18px;
    margin-bottom: 18px;
    border-bottom: 1.5px solid #e0e7ef;
}

.tab {
    background: none;
    border: none;
    font-size: 1.1rem;
    font-weight: 600;
    color: #222;
    padding: 12px 0 10px 0;
    border-bottom: 3px solid transparent;
    cursor: pointer;
}

.tab.active {
    color: #19c37d;
    border-bottom: 3px solid #19c37d;
}

.detail-content {
    display: flex;
    gap: 32px;
    margin-bottom: 18px;
}

.detail-image img {
    width: 180px;
    height: 180px;
    object-fit: cover;
    border-radius: 8px;
    border: 1px solid #eee;
    background: #fafbfc;
}

.detail-info {
    flex: 1;
    min-width: 320px;
    display: flex;
    flex-direction: column;
    gap: 7px;
}

.info-row {
    display: flex;
    justify-content: space-between;
    border-bottom: 1px dashed #eee;
    padding: 5px 0;
    font-size: 16px;
}

.detail-actions {
    display: flex;
    gap: 12px;
    align-items: center;
    margin-left: 32px;
    flex-direction: column;
    justify-content: flex-start;
}

.detail-actions .btn {
    min-width: 160px;
    margin-bottom: 6px;
}

.detail-desc {
    display: flex;
    gap: 32px;
    margin-top: 18px;
}

.desc-box {
    min-height: 40px;
    background: #fafbfc;
    border: 1px solid #eee;
    border-radius: 6px;
    padding: 8px 12px;
    margin-top: 4px;
    font-size: 15px;
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
    transition: background 0.2s, color 0.2s;
    outline: none;
    display: flex;
    align-items: center;
    gap: 7px;
}

.btn.green {
    background: #19c37d;
}

.btn.gray {
    background: #888;
}

.btn.green-outline {
    background: #e6f9f1;
    color: #19c37d;
    border: 1.5px solid #19c37d;
}

.btn.red-outline {
    background: #fff0f0;
    color: #e74c3c;
    border: 1.5px solid #e74c3c;
}

.btn.red {
    background: #e74c3c;
}

.btn:not(:last-child) {
    margin-right: 0;
}

.icon-save:before {
    content: "💾";
}

.icon-barcode:before {
    content: "� barcode";
}

.icon-copy:before {
    content: "📋";
}

.icon-lock:before {
    content: "🔒";
}

.icon-delete:before {
    content: "🗑️";
}

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

.edit-form {
    background: #fff;
    border-radius: 10px;
    padding: 32px 28px;
    min-width: 340px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.13);
    display: flex;
    flex-direction: column;
    gap: 14px;
}

.update-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 32px 24px;
    align-items: flex-start;
}

.update-col {
    display: flex;
    flex-direction: column;
    gap: 14px;
}

.edit-form label {
    font-weight: 600;
    margin-bottom: 2px;
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.edit-actions {
    display: flex;
    gap: 18px;
    margin-top: 12px;
    justify-content: flex-end;
}

.loading {
    text-align: center;
    margin-top: 80px;
    font-size: 20px;
    color: #888;
}
</style>