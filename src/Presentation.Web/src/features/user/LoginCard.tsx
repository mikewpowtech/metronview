import { Card } from "antd";
import { LoginForm } from "./LoginForm";
import React from "react";

interface LoginCardProps {
    onLoginSuccess?: () => void;
    loading?: boolean;
}

export const LoginCard: React.FC<LoginCardProps> = ({ onLoginSuccess, loading }) => {
    return (
        <div className="flex-center" style={{ minHeight: "60vh" }}>
            <Card
                title="Login to MetronView"
                style={{ 
                    width: 400, 
                    borderRadius: "8px",
                    boxShadow: "0 4px 12px rgba(0,0,0,0.15)"
                }}
                headStyle={{
                    background: "linear-gradient(to bottom, #5a6a71 0%, #3a4a99 100%)",
                    color: "white",
                    textAlign: "center",
                    fontSize: "18px",
                    fontWeight: "600"
                }}
                bodyStyle={{
                    padding: "32px"
                }}
            >
                <LoginForm onLoginSuccess={onLoginSuccess} loading={loading} />
            </Card>
        </div>
    );
};