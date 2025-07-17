import React from "react";
import { LoginCard } from "../features/user/LoginCard";
import { useNavigate } from "react-router-dom";
import { Button } from "antd";
import { ToolOutlined } from "@ant-design/icons";

export const LoginPage = () => {
  const navigate = useNavigate();
  const onLoginSuccess = () => navigate("/");

  return (
    <div style={{ position: "relative" }}>
      <LoginCard onLoginSuccess={onLoginSuccess} />
      <div style={{ position: "absolute", top: "16px", right: "16px" }}>
        <Button
          type="text"
          icon={<ToolOutlined />}
          onClick={() => navigate("/system-status")}
          style={{ color: "#666" }}
        >
          System Status
        </Button>
      </div>
    </div>
  );
};
