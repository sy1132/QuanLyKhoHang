import baseApi from "./base.api";
import { type Product, type CreateProductModel } from "../models/product-model";

const productApi = {
  getAllProducts: async (): Promise<Product[]> => {
    const response = await baseApi.get("/Products");
    return response.data;
  },
  getProductById: async (id: number): Promise<Product> => {
    const response = await baseApi.get(`/Products/${id}`);
    return response.data;
  },
  createProduct: async (data: CreateProductModel): Promise<Product> => {
    const response = await baseApi.post("/Products", data);
    return response.data;
  },
  updateProduct: async (
    id: number,
    data: Partial<Product>
  ): Promise<Product> => {
    const response = await baseApi.put(`/Products/${id}`, data);
    return response.data;
  },
  deleteProduct: async (id: number): Promise<void> => {
    await baseApi.delete(`/Products/${id}`);
  },
};

export default productApi;
