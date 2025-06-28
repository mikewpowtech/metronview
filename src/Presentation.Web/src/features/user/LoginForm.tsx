import { Form, Input, Button, message } from "antd";
import { useState, useRef } from "react";
import { useAppDispatch } from "../../app/hooks";
import { login } from "./authAPI";
import { updateToken, resetLoading, setLoading } from "./authSlice";

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
            // Use login API (same as LoginModal)
            const data = await login(values.username, values.password);
            dispatch(resetLoading());
            if (data?.isSucceed && data?.data) {
                message.success("Login is successful.");
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
                    message.error("Login failed.");
                }
            } else {
                message.error("Login failed.");
            }
        } catch (err: any) {
            message.error(err?.message || "Login failed");
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
            style={{ maxWidth: 320 }}
        >
            <Form.Item
                label="Username"
                name="username"
                rules={[{ required: true, message: "Please enter your username" }]}
            >
                <Input autoFocus />
            </Form.Item>
            <Form.Item
                label="Password"
                name="password"
                rules={[{ required: true, message: "Please enter your password" }]}
            >
                <Input.Password />
            </Form.Item>
            <Form.Item>
                <Button
                    type="primary"
                    htmlType="submit"
                    loading={loading || submitting}
                    block
                >
                    Log In
                </Button>
            </Form.Item>
        </Form>
    );
};