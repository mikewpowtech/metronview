import { Card } from "antd";
import { LoginForm } from "./LoginForm"; // Assuming you have a LoginForm component used inside LoginModal
import React from "react";

interface LoginCardProps {
    onLoginSuccess?: () => void;
    loading?: boolean;
}

export const LoginCard: React.FC<LoginCardProps> = ({ onLoginSuccess, loading }) => {
    return (
        <div style={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "60vh" }}>
            <Card
                title="Login"
                style={{ width: 350, boxShadow: "0 2px 8px rgba(0,0,0,0.1)" }}
                bordered
            >
                <LoginForm onLoginSuccess={onLoginSuccess} loading={loading} />
            </Card>
        </div>
    );
};