import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from "./App.tsx";
import { BrowserRouter } from "react-router-dom";
import { Provider } from "react-redux";
import { store, persister } from "./app/store.ts";
import { AxiosApiInterceptor } from "./app/AxiosApiInterceptor.ts";
import { PersistGate } from 'redux-persist/integration/react';
import { Spin } from 'antd';

// Loading component for PersistGate - only shows during actual rehydration
const PersistLoading = () => (
    <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh',
        backgroundColor: '#f0f2f5'
    }}>
        <Spin size="large" tip="Initializing application...">
            <div style={{ padding: '50px' }} />
        </Spin>
    </div>
);

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <Provider store={store}>
            <PersistGate loading={<PersistLoading />} persistor={persister}>
                <BrowserRouter>
                    <AxiosApiInterceptor />
                    <App />
                </BrowserRouter>
            </PersistGate>
        </Provider>
    </StrictMode>
);
