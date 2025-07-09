import { App, Layout } from "antd";
import { Content, Header } from "antd/es/layout/layout";
import { AppLogo } from "../features/AppLogo";
import { AppFooter } from "../features/Footer";
import { LoginCard } from "../features/user/LoginCard";
import { useNavigate } from "react-router-dom";

export const DefaultLayout = () => {
    const navigate = useNavigate();
    const onLoginSuccess = () => navigate("/");

    return (
        <App>
            <Layout className="layout" style={{ minHeight: "100vh" }}>
                {/* AppLogo on its own line */}
                <div style={{ background: "#5a6a71", padding: "2px 0 1px 0", display: "flex", justifyContent: "flex-start" }}>
                    <AppLogo />
                </div>
                <Header
                    style={{
                        display: "flex",
                        alignItems: "center",
                        background: "linear-gradient(to bottom, #5a6a71 0%, #3a4a99 100%)",
                        minHeight: 40,
                        height: 40,
                        padding: "0 40px",
                    }}
                ></Header>
                <Content style={{ padding: "0 30px", flex: 1, minHeight: 0 }}>
                    <LoginCard onLoginSuccess={onLoginSuccess} />
                </Content>
                <AppFooter />
            </Layout>
        </App>
    );
};