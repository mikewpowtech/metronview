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
  debug: true,
};

const rootReducer = combineReducers({
  auth: persistReducer(authPersistConfig, authReducer),
});

export const selectAuth = (state: RootState) => {
    console.log("Redux state:", state);
    return state.auth;
};

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