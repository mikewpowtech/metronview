import {
  configureStore,
  type ThunkAction,
  type Action,
  combineReducers
} from "@reduxjs/toolkit";
import authReducer from "../features/user/authSlice";
import storage from "redux-persist/lib/storage"; 
import {
    persistReducer, persistStore, FLUSH,
    REHYDRATE,
    PAUSE,
    PERSIST,
    PURGE,
    REGISTER,
} from "redux-persist";

const authPersistConfig = {
  key: "auth",
  storage: storage,  
  debug: process.env.NODE_ENV === "development", // Only debug in development
};

const rootReducer = combineReducers({
  auth: persistReducer(authPersistConfig, authReducer),
});

export const store = configureStore({
    reducer: rootReducer,
    devTools: process.env.NODE_ENV !== "production",
    middleware: getDefaultMiddleware => getDefaultMiddleware({
        serializableCheck: {
            ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
        }
    })
});

export const persister = persistStore(store);

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;
export type AppThunk<ReturnType = void> = ThunkAction<ReturnType,RootState,unknown,Action<string>>;

// Move this selector after the store is defined to avoid circular dependency
export const selectAuth = (state: RootState) => {
    // Only log in development and reduce console noise
    if (process.env.NODE_ENV === "development") {
        console.log("Auth state:", {
            status: state.auth.status,
            hasUser: !!state.auth.user,
            hasTokens: !!(state.auth.accessToken && state.auth.refreshToken)
        });
    }
    return state.auth;
};