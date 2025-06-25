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

    // Define dropdown menu items for each main menu
    const menuDropdowns = [
        {
            key: "/dashboard",
            label: "Home",
            items: [
            ],
        },
        {
            key: "/units",
            label: "Units",
            items: [
                {
                    key: "/readings",
                    label: <span onClick={() => handleClick("/readings")}>Readings</span>,
                },
                {
                    key: "/sensors",
                    label: <span onClick={() => handleClick("/sensors")}>Sensors</span>,
                },
                {
                    key: "/unitmodels",
                    label: <span onClick={() => handleClick("/unitmodels")}>Models</span>,
                },
                {
                    key: "/unitmodelconfigs",
                    label: <span onClick={() => handleClick("/unitmodels")}>Model Configurations</span>,
                },
                // Add more submenu items here if needed
            ],
        },
        {
            key: "/alarms",
            label: "Alarms",
            items: [
                {
                    key: "/alarmhistory",
                    label: <span onClick={() => handleClick("/alarmhistory")}>History</span>,
                },
                {
                    key: "/recipients",
                    label: <span onClick={() => handleClick("/recipients")}>Recipients</span>,
                },
                {
                    key: "/recipientgroups",
                    label: <span onClick={() => handleClick("/recipientgroups")}>Recipient Groups</span>,
                },
                {
                    key: "/senderoptions",
                    label: <span onClick={() => handleClick("/senderoptions")}>SenderOptions</span>,
                },
                {
                    key: "/alarminstructions",
                    label: <span onClick={() => handleClick("/alarminstructions")}>Instructions</span>,
                },
                // Add more submenu items here if needed
            ],
        },
        {
            key: "/companies",
            label: "Companies",
            items: [
            ],
        },
        {
            key: "/users",
            label: "Logins",
            items: [
            ],
        },
    ];

    return (
        <App>
            <Layout className="layout">
                <Header style={{ display: "flex", alignItems: "center" }}>
                    <AppLogo />
                    <div style={{ minWidth: "500px", display: "flex" }}>
                        {menuDropdowns.map(menu => (
                            <Dropdown
                                key={menu.key}
                                menu={{ items: menu.items }}
                                trigger={["hover"]}
                            >
                                <span
                                    style={{
                                        color: current === menu.key ? "#1890ff" : "#fff",
                                        fontWeight: 500,
                                        cursor: "pointer",
                                        marginRight: 24,
                                        userSelect: "none",
                                        padding: "0 12px",
                                        lineHeight: "48px",
                                        borderBottom: current === menu.key ? "2px solid #1890ff" : "none",
                                    }}
                                    onClick={() => handleClick(menu.key)}
                                >
                                    {menu.label}
                                </span>
                            </Dropdown>
                        ))}
                    </div>
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
