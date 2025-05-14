export interface LoginModel {
  user: string;
  password: string;
}

// models/user-model.ts
export interface UserInfoModel {
  userName: string;
  id: number;
  email: string;
}

export interface LoginResponseModel {
  userId: number;
  username: string;
  email: string;
  token: string;
  expiration: string;
  roles: string[];
}
