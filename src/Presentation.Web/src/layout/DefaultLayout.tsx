import { App, Layout } from "antd";
import { Content, Header } from "antd/es/layout/layout";
import { AppLogo } from "../features/AppLogo";
import { AppFooter } from "../features/Footer";
import { Outlet } from "react-router";

export const DefaultLayout = () => {
    return (
        <App>
            <Layout className="layout" style={{ minHeight: "100vh" }}>
                {/* AppLogo on its own line */}
                <div style={{ 
                    background: "#5a6a71", 
                    padding: "2px 0 1px 0", 
                    display: "flex", 
                    justifyContent: "flex-start" 
                }}>
                    <AppLogo />
                </div>
                <Header className="app-header">
                    {/* Header content can go here */}
                </Header>
                <Content className="flex-center" style={{ flex: 1 }}>
                    <div className="login-card">
                        <Outlet />
                    </div>
                </Content>
                <AppFooter />
            </Layout>
        </App>
    );
};