export interface CreateProductModel {
  name: string;
  description: string;
  quantity: number;
  category: string;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  quantity: number;
  category: string;
}
