import { Form, Input, Button, message } from "antd";
import { useState, useRef } from "react";
import { useAppDispatch } from "../../app/hooks";
import { login } from "./authAPI";
import { updateToken, resetLoading, setLoading } from "./authSlice";
import { UserOutlined, LockOutlined } from "@ant-design/icons";

interface LoginFormProps {
    onLoginSuccess?: () => void;
    loading?: boolean;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onLoginSuccess, loading }) => {
    const dispatch = useAppDispatch();
    const [form] = Form.useForm();
    const formRef = useRef<any>(null);
    const [submitting, setSubmitting] = useState(false);

    const handleFinish = async (values: { username: string; password: string }) => {
        setSubmitting(true);
        dispatch(setLoading());
        try {
            const data = await login(values.username, values.password);
            dispatch(resetLoading());
            if (data?.isSucceed && data?.data) {
                message.success("Login successful!");
                dispatch(updateToken(data.data));
                if (onLoginSuccess) {
                    onLoginSuccess();
                }
            } else if (data != null) {
                if (data?.messages?.email) {
                    (formRef.current ?? form).setFields([
                        { name: "username", errors: data.messages.email },
                    ]);
                }
                if (data?.messages?.password) {
                    (formRef.current ?? form).setFields([
                        { name: "password", errors: data.messages.password },
                    ]);
                }
                if (data?.messages?.login) {
                    message.error("Invalid username or password");
                } else {
                    message.error("Login failed. Please check your credentials.");
                }
            } else {
                message.error("Login failed. Please try again.");
            }
        } catch (err: any) {
            dispatch(resetLoading());
            message.error(err?.message || "Network error. Please try again.");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <Form
            form={form}
            ref={formRef}
            layout="vertical"
            onFinish={handleFinish}
            className="app-form"
            size="large"
        >
            <Form.Item
                label="Username"
                name="username"
                rules={[{ required: true, message: "Please enter your username" }]}
            >
                <Input 
                    prefix={<UserOutlined />}
                    placeholder="Enter your username"
                    autoFocus 
                />
            </Form.Item>
            <Form.Item
                label="Password"
                name="password"
                rules={[{ required: true, message: "Please enter your password" }]}
            >
                <Input.Password 
                    prefix={<LockOutlined />}
                    placeholder="Enter your password"
                />
            </Form.Item>
            <Form.Item style={{ marginBottom: 0 }}>
                <Button
                    type="primary"
                    htmlType="submit"
                    loading={loading || submitting}
                    block
                    size="large"
                    style={{
                        height: "48px",
                        fontSize: "16px",
                        fontWeight: "500",
                        marginTop: "16px"
                    }}
                >
                    Log In
                </Button>
            </Form.Item>
        </Form>
    );
};