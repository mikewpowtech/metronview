import { App, Dropdown, Layout, Menu } from "antd";
import { Content, Header } from "antd/es/layout/layout";
import { useEffect, useState } from "react";
import { Outlet, useLocation, useNavigate } from "react-router";
import { type iUser, logoutAsync } from "../features/user/authSlice";
import { useAppDispatch } from "../app/hooks";
import { AppLogo } from "../features/AppLogo";
import { AppFooter } from "../features/Footer";

export const UserLayout = ({ UserName }: iUser) => {
    const navigate = useNavigate();
    const location = useLocation();
    const dispatch = useAppDispatch();
    const [current, setCurrent] = useState(
        location.pathname === "/" || location.pathname === ""
            ? "/"
            : location.pathname
    );

    useEffect(() => {
        if (location) {
            if (current !== location.pathname) {
                setCurrent(location.pathname);
            }
        }
    }, [location]);

    const handleClick = (key: string) => {
        navigate(key);
    };

    const handleLogout = () => {
        dispatch(logoutAsync());
        handleClick("/");
    };

    const userMenu = {
        items: [
            {
                key: "edit-profile",
                label: <span onClick={() => handleClick("/edit-profile")}>Edit Profile</span>,
            },
            {
                key: "logout",
                label: <span onClick={handleLogout}>Logout</span>,
            },
        ],
    };

    return (
        <App>
            <Layout className="layout">
                <Header style={{ display: "flex", alignItems: "center" }}>
                    <AppLogo />
                    <Menu
                        theme="dark"
                        mode="horizontal"
                        defaultSelectedKeys={["/"]}
                        selectedKeys={[current]}
                        style={{ minWidth: "500px" }}
                        items={[
                            {
                                key: "/",
                                label: "Home",
                                onClick: (e) => {
                                    handleClick(e.key);
                                },
                            },
                            {
                                key: "/users",
                                label: "Users",
                                onClick: (e) => {
                                    handleClick(e.key);
                                },
                            },
                            {
                                key: "/units",
                                label: "Units",
                                onClick: (e) => {
                                    handleClick(e.key);
                                },
                            },
                            {
                                key: "/companies",
                                label: "Companies",
                                onClick: (e) => {
                                    handleClick(e.key);
                                },
                            },
                            {
                                key: "/system-status",
                                label: "System Status",
                                onClick: (e) => {
                                    handleClick(e.key);
                                },
                            },

                        ]}
                    />
                    <div style={{ marginLeft: "auto", display: "flex", alignItems: "center" }}>
                        <Dropdown menu={userMenu} trigger={["click"]}>
                            <span
                                style={{
                                    marginLeft: 14,
                                    color: "#fff",
                                    fontWeight: 500,
                                    cursor: "pointer",
                                    userSelect: "none",
                                }}
                            >
                                {UserName}
                            </span>
                        </Dropdown>
                    </div>
                </Header>
                <Content style={{ padding: "0 50px", minHeight: "400px" }}>
                    <Outlet />
                </Content>
                <AppFooter />
            </Layout>
        </App>
    );
};
