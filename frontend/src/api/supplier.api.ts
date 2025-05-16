import baseApi from "./base.api";

const SupplierApi = {
    // Lấy tất cả nhà cung cấp
    GetAll: async (): Promise<any> => {
        const response = await baseApi.get("supplier");
        return response.data.data;
    },
    // Tìm kiếm nhà cung cấp
    Search: async (searchString: string): Promise<any> => {
        const response = await baseApi.get("supplier/search", {
            params: { searchString }
        });
        return response.data.data;
    },
    // Tạo mới nhà cung cấp
    Create: async (data: any): Promise<any> => {
        const response = await baseApi.post("supplier/create", data);
        return response.data;
    },
    // Import nhà cung cấp từ file Excel
    Import: async (file: File): Promise<any> => {
        const formData = new FormData();
        formData.append("file", file);
        const response = await baseApi.postForm("supplier/import", formData);
        return response.data;
    },
    Export: async (): Promise<Blob> => {
        const response = await baseApi.get("supplier/export", {
            responseType: "blob"
        });
        return response.data;
    },
    // Tìm kiếm theo khoảng thời gian tạo
    SearchByDate: async (fromDate: string, toDate: string): Promise<any> => {
        const response = await baseApi.get("supplier/search-by-date", {
            params: { fromDate, toDate }
        });
        return response.data.data;
    },
    // Tìm kiếm theo kho
    SearchByWarehouse: async (warehouseId: number): Promise<any> => {
        const response = await baseApi.get("supplier/search-by-warehouse", {
            params: { warehouseId }
        });
        return response.data.data;
    }
};

export default SupplierApi;