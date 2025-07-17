/* eslint-disable @typescript-eslint/no-unused-vars */
import { type PayloadAction, createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import { jwtDecode } from "jwt-decode";
import { logout } from "./authAPI";

export interface iUser {
  Id: string;
  RoleClaim: Array<string>;
  UserName: string;
}
export interface iAuthState {
  status: "idle" | "loading" | "failed";
  accessToken?: string;
  refreshToken?: string;
  user?: iUser;
}

const initialState: iAuthState = {
  status: "idle",
};

export const logoutAsync = createAsyncThunk("user/logout", async () => {
  const response = await logout();
  // The value we return becomes the `fulfilled` action payload
  return response?.data;
});

export const authSlice = createSlice({
  name: "auth",
  initialState,
  // The `reducers` field lets us define reducers and generate associated actions
  reducers: {
    updateToken: (
      state,
      action: PayloadAction<{ accessToken: string; refreshToken: string }>
    ) => {
      state.accessToken = action.payload.accessToken;
      state.refreshToken = action.payload.refreshToken;
      state.user = jwtDecode<iUser>(action.payload.accessToken);
      state.status = "idle"; // Ensure status is reset after successful token update
    },
    resetToken: (state) => {
      state.accessToken = undefined;
      state.refreshToken = undefined;
      state.user = undefined;
      state.status = "idle"; // Ensure status is reset after token reset
    },
    setLoading: (state) => {
      state.status = "loading";
    },
    resetLoading: (state) => {
      state.status = "idle";
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(logoutAsync.pending, (state) => {
        state.status = "loading";
      })
      .addCase(logoutAsync.fulfilled, (state) => {
        state.status = "idle";
        state.accessToken = undefined;
        state.refreshToken = undefined;
        state.user = undefined;
      })
      .addCase(logoutAsync.rejected, (state) => {
        state.status = "failed";
      })
      // Handle Redux Persist rehydration
      .addDefaultCase((state, action) => {
        // Handle REHYDRATE action from Redux Persist
        if (action.type === "persist/REHYDRATE") {
          const rehydrateAction = action as any;
          if (rehydrateAction.payload && rehydrateAction.payload.auth) {
            const rehydratedAuth = rehydrateAction.payload.auth as iAuthState;
            // Always reset status to idle after rehydration to prevent startup loading
            state.status = "idle";
            state.accessToken = rehydratedAuth.accessToken;
            state.refreshToken = rehydratedAuth.refreshToken;
            state.user = rehydratedAuth.user;
          }
        }
      });
  },
});

export const { updateToken, resetToken, setLoading, resetLoading } =
  authSlice.actions;
export default authSlice.reducer;

export const updateProfileAsync = createAsyncThunk(
  "auth/updateProfile",
  async (profile: { userName: string; email: string; phoneNumber?: string }, _thunkAPI) => {
    // Simulate API delay
    await new Promise((resolve) => setTimeout(resolve, 800));
    // Return the updated profile as if it was saved on the server
    return profile;
  }
);