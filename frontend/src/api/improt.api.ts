import axios from 'axios';

// Interfaces for Import data models
export interface Import {
  id: number;
  warehouseId: number;
  dateInput: string;
  cost: number;
  status: string;
}

export interface ImportDetail {
  id?: number;
  idInport: number;
  idProduct: number;
  idSupplier: number;
  quantity: number;
  cost: number;
  note?: string;
}

export interface ImportCreateModel {
  warehouseId: number;
  details: ImportDetailRequest[];
}

export interface ImportUpdateModel {
  id: number;
  warehouseId: number;
  details: ImportDetailRequest[];
}

export interface ImportDetailRequest {
  idProduct: number;
  idSupplier: number;
  quantity: number;
  cost: number;
  note?: string;
}

export interface ImportListParams {
  status?: string;
  fromDate?: string;
  toDate?: string;
}

export interface ImportDetailsResponse {
  import: Import;
  details: ImportDetail[];
}

export const importApi = {
  /**
   * Get all imports
   */
  getAllImports: async (): Promise<Import[]> => {
    try {
      console.log('API: Getting all imports');
      const response = await axios.get('/api/import');
      console.log('API response for getAllImports:', response);
      return response.data.data;
    } catch (error) {
      console.error('Error fetching imports:', error);
      throw error;
    }
  },

  /**
   * Get filtered imports
   */
  getFilteredImports: async (params: ImportListParams): Promise<Import[]> => {
    try {
      console.log('API: Getting filtered imports with params:', params);
      const queryParams = new URLSearchParams();
      
      if (params.status) {
        queryParams.append('status', params.status);
      }
      
      if (params.fromDate) {
        queryParams.append('fromDate', params.fromDate);
      }
      
      if (params.toDate) {
        queryParams.append('toDate', params.toDate);
      }
      
      const response = await axios.get(`/api/import/list?${queryParams.toString()}`);
      console.log('API response for getFilteredImports:', response);
      return response.data.data;
    } catch (error) {
      console.error('Error fetching filtered imports:', error);
      throw error;
    }
  },

  /**
   * Get import by ID
   */
  getImportById: async (id: number): Promise<Import> => {
    try {
      console.log(`API: Getting import with ID ${id}`);
      const response = await axios.get(`/api/import/${id}`);
      console.log('API response for getImportById:', response);
      return response.data.data;
    } catch (error) {
      console.error(`Error fetching import with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Get import details by ID
   */
  getImportDetails: async (id: number): Promise<ImportDetailsResponse> => {
    try {
      console.log(`API: Getting details for import with ID ${id}`);
      const response = await axios.get(`/api/import/${id}/details`);
      console.log('API response for getImportDetails:', response);
      return response.data.data;
    } catch (error) {
      console.error(`Error fetching details for import with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Create a new import
   */
  createImport: async (data: ImportCreateModel): Promise<{ importId: number }> => {
    try {
      console.log('API: Creating new import with data:', data);
      const response = await axios.post('https://localhost:7189/api/import/create', data);
      console.log('API response for createImport:', response);
      return response.data.data;
    } catch (error) {
      console.error('Error creating import:', error);
      throw error;
    }
  },

  /**
   * Update an existing import
   */
  updateImport: async (id: number, data: ImportUpdateModel): Promise<void> => {
    try {
      console.log(`API: Updating import with ID ${id}`, data);
      const response = await axios.put(`/api/import/${id}`, data);
      console.log('API response for updateImport:', response);
    } catch (error) {
      console.error(`Error updating import with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Approve an import
   */
  approveImport: async (id: number): Promise<string> => {
    try {
      const response = await axios.patch(`/api/import/${id}/approve`);
      return response.data.message;
    } catch (error) {
      console.error(`Error approving import with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Reject an import
   */
  rejectImport: async (id: number): Promise<string> => {
    try {
      const response = await axios.patch(`/api/import/${id}/reject`);
      return response.data.message;
    } catch (error) {
      console.error(`Error rejecting import with ID ${id}:`, error);
      throw error;
    }
  },

  /**
   * Delete an import
   */
  deleteImport: async (id: number): Promise<string> => {
    try {
      const response = await axios.delete(`/api/import/${id}`);
      return response.data.message;
    } catch (error) {
      console.error(`Error deleting import with ID ${id}:`, error);
      throw error;
    }
  }
};

