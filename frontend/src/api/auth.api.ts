import baseApi from "./base.api";
export default {
  login: async (model: LoginModel): Promise<any | undefined> => {
    const loginData = { ...model, rememberMe: true };
    const response = await baseApi.postAuthenticate("Auth/login", loginData);
    if (response.status === 200) {
      return response.data;
    } else {
      throw new Error("Login failed");
    }
  },
};
export interface LoginModel {
  userName: string;
  password: string;
  rememberMe?: boolean;
}
