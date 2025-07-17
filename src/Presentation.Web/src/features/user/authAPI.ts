import axios from "axios";
import type { iAppResponse } from "../../app/appResponse";
import type { ApplicationUser } from "../../features/user/userColumns";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

export const login = async (email: string, password: string) => {
  const response = await axios.post<iAppResponse<{ accessToken: string; refreshToken: string }>>(`${BASE_URL}/user/login`, {
    email: email,
    password: password,
  }).catch((ex)=>{
    console.log(ex);
  });
  return response ? response.data : undefined;
};

export const refreshToken = async (data: {
  accessToken: string;
  refreshToken: string;
}) => {
  const response = await axios.post<iAppResponse<{ accessToken: string; refreshToken: string }>>(`${BASE_URL}/user/refreshToken`, data).catch((ex)=>{
    console.log(ex);
  });;
  return response ? response.data : undefined;
};

export const register = async (email: string, password: string) => {
  const response = await axios.post<iAppResponse<{}>>(
    `${BASE_URL}/user/register`,
    {
      email: email,
      password: password,
    }
  ).catch((ex)=>{
    console.log(ex);
  });
  return response ? response.data : undefined;
};

export const logout = async () => {
  const response = await axios.post<iAppResponse<boolean>>(
    `${BASE_URL}/user/logout`
  ).catch((ex)=>{
    console.log(ex);
  });;
  return response ? response.data : undefined;
};
export const profileApi = async () => {
  const response = await axios.post(`${BASE_URL}/user/profile`).catch((ex)=>{
    console.log(ex);
  });
  return response ? response.data : undefined;
};
export const usersApi = async () => {
    const response = await axios.get<iAppResponse<ApplicationUser[]>>(`${BASE_URL}/user/getall`).catch((ex)=>{
    console.log(ex);
  });
  return response ? response.data : undefined;
};
