import { App, Dropdown, Layout } from "antd";
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

    // Define dropdown menu items for each main menu
    const menuDropdowns = [
        {
            key: "/dashboard",
            label: "Home",
            items: [{
                key: "/dashboard",
                label: <span onClick={() => handleClick("/dashboard")}>Dashboard</span>,
            },{
                key: "/visualization", // Changed from "/visualisation"
                label: <span onClick={() => handleClick("/visualization")}>Metronview Designer{'\u2122'}</span>,
            }],
        },
        {
            key: "/units",
            label: "Units",
            items: [
                {
                    key: "/units",
                    label: <span onClick={() => handleClick("/units")}>Units</span>,
                },
                {
                    key: "/readings",
                    label: <span onClick={() => handleClick("/readings")}>Readings</span>,
                },
                {
                    key: "/sensors",
                    label: <span onClick={() => handleClick("/sensors")}>Sensors</span>,
                },

            ],
        },
        {
            key: "/alarms",
            label: "Alarms",
            items: [
                {
                    key: "/alarms",
                    label: <span onClick={() => handleClick("/alarms")}>Alarms</span>,
                },
                {
                    key: "/triggers",
                    label: <span onClick={() => handleClick("/triggers")}>Triggers</span>,
                },
                {
                    key: "/senderoptions",
                    label: <span onClick={() => handleClick("/senderoptions")}>Sender Options</span>,
                },
                {
                    key: "/alarminstructions",
                    label: <span onClick={() => handleClick("/alarminstructions")}>Instructions</span>,
                },
            ],
        },
        {
            key: "/unitmodels",
            label: "Config",
            items: [
                {
                    key: "/unitmodels",
                    label: <span onClick={() => handleClick("/unitmodels")}>Sensor Types</span>,
                },
                {
                    key: "/unitmodelconfigs",
                    label: <span onClick={() => handleClick("/unitmodels")}>Sensor Templates</span>,
                },
                {
                    key: "/unitmodels",
                    label: <span onClick={() => handleClick("/unitmodels")}>Unit Types</span>,
                },
                {
                    key: "/unitmodelconfigs",
                    label: <span onClick={() => handleClick("/unitmodels")}>Unit Templates</span>,
                },
            ],
        },
        {
            key: "/companies",
            label: "Companies",
            items: [
                {
                    key: "/companies",
                    label: <span onClick={() => handleClick("/companies")}>Companies</span>,
                },
                {
                    key: "/companies",
                    label: <span onClick={() => handleClick("/companies")}>Contacts</span>,
                },
                {
                    key: "/recipientsets",
                    label: <span onClick={() => handleClick("/recipientsets")}>Contact Groups</span>,
                },
            ],
        },
        {
            key: "/users",
            label: "Admin",
            items: [
                {
                    key: "/users",
                    label: <span onClick={() => handleClick("/users")}>Users</span>,
                },
                {
                    key: "/users",
                    label: <span onClick={() => handleClick("/users")}>System</span>,
                },
            ],
        },
    ];

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
                    <div className="flex-between" style={{ width: "100%" }}>
                        <div className="app-menu" style={{ display: "flex" }}>
                            {menuDropdowns.map(menu => (
                                <Dropdown
                                    key={menu.key}
                                    menu={{ items: menu.items }}
                                    trigger={["hover"]}
                                    placement="bottomLeft"
                                >
                                    <span
                                        className={`menu-item ${current === menu.key ? 'active' : ''}`}
                                        style={{
                                            color: current === menu.key ? "#1890ff" : "#fff",
                                            fontWeight: 500,
                                            cursor: "pointer",
                                            marginRight: 24,
                                            userSelect: "none",
                                            padding: "0 12px",
                                            lineHeight: "60px",
                                            fontSize: 15,
                                            borderBottom: current === menu.key ? "2px solid #1890ff" : "none",
                                            transition: "all 0.3s",
                                        }}
                                        onClick={() => handleClick(menu.key)}
                                    >
                                        {menu.label}
                                    </span>
                                </Dropdown>
                            ))}
                        </div>
                        <div style={{ display: "flex", alignItems: "center" }}>
                            <Dropdown menu={userMenu} trigger={["click"]} placement="bottomRight">
                                <span
                                    style={{
                                        color: "#fff",
                                        fontWeight: 500,
                                        cursor: "pointer",
                                        userSelect: "none",
                                        fontSize: 15,
                                        padding: "0 12px",
                                        lineHeight: "60px",
                                        transition: "color 0.3s",
                                    }}
                                >
                                    {UserName}
                                </span>
                            </Dropdown>
                        </div>
                    </div>
                </Header>
                <Content style={{ 
                    padding: "24px", 
                    flex: 1, 
                    minHeight: 0,
                    background: "#f0f2f5"
                }}>
                    <div style={{ 
                        background: "#fff",
                        padding: "24px",
                        borderRadius: "8px",
                        boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
                        minHeight: "calc(100vh - 200px)"
                    }}>
                        <Outlet />
                    </div>
                </Content>
                <AppFooter />
            </Layout>
        </App>
    );
};
