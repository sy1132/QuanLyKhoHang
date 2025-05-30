import axios from "axios";
import Cookies from "js-cookie";
import type { AxiosResponse } from "axios";

const apiClient = axios.create({
  baseURL: "https://localhost:7189/api/",
  headers: {
    "Content-Type": "application/json",
  },
});

function getToken() {
  return Cookies.get("authToken");
}

export default {
  get: async (
    endpoint: string,
    params?: Record<string, any>
  ): Promise<AxiosResponse> => {
    const token = getToken();
    return await apiClient.get(endpoint, {
      headers: { Authorization: `Bearer ${token}` },
      params,
    });
  },

  post: async (endpoint: string, body: any): Promise<AxiosResponse> => {
    const token = getToken();
    console.log("Token being used:", token);
    return await apiClient.post(`${endpoint}`, body, {
      headers: { Authorization: `Bearer ${token}` },
    });
  },

  postForm: async (
    endpoint: string,
    formData: FormData
  ): Promise<AxiosResponse> => {
    const token = getToken();
    return await apiClient.post(endpoint, formData, {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "multipart/form-data",
      },
    });
  },

  postAuthenticate: async (
    endpoint: string,
    body: any
  ): Promise<AxiosResponse> => {
    return await apiClient.post(`${endpoint}`, body);
  },

  put: async (endpoint: string, body: any): Promise<AxiosResponse> => {
    const token = getToken();
    return await apiClient.put(`${endpoint}`, body, {
      headers: { Authorization: `Bearer ${token}` },
    });
  },

  delete: async (endpoint: string): Promise<AxiosResponse> => {
    const token = getToken();
    return await apiClient.delete(`${endpoint}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
  },
  register: async (
    userName: string,
    password: string,
    email: string
  ): Promise<AxiosResponse> => {
    return await apiClient.post("Authenticate/register", {
      userName,
      password,
      email,
    });
  },
  updateUser: async (
    token: string,
    userName: string,
    email: string
  ): Promise<AxiosResponse> => {
    return await apiClient.put(
      "Authenticate/update-user",
      { userName, email },
      { headers: { Authorization: `Bearer ${token}` } }
    );
  },
};
