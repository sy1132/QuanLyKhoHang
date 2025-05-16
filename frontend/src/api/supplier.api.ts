import baseApi from "./base.api";

const SupplierApi = {
    // Lấy tất cả nhà cung cấp
    GetAll: async (): Promise<any> => {
        const response = await baseApi.get("/supplier");
        return response.data;
    },
    // Tìm kiếm nhà cung cấp
    Search: async (searchString: string): Promise<any> => {
        const response = await baseApi.get(`/supplier/search`, {
            params: { searchString }
        });
        return response.data;
    },
    // Tạo mới nhà cung cấp
    Create: async (data: any): Promise<any> => {
        const response = await baseApi.post("/supplier/create", data);
        return response.data;
    },
    // Import nhà cung cấp từ file Excel
    Import: async (file: File): Promise<any> => {
        const formData = new FormData();
        formData.append("file", file);
        const response = await baseApi.post("/supplier/import", {
            data: formData,
            headers: { "Content-Type": "multipart/form-data" }
        });
        return response.data;
    },
    // Export danh sách nhà cung cấp ra file Excel
    Export: async (): Promise<Blob> => {
        const response = await baseApi.get("/supplier/export", {
            responseType: "blob"
        });
        return response.data;
    }
};

export default SupplierApi;