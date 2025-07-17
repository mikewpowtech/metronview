import React, { useEffect, useState } from "react";
import { Form, Input, Button, Spin, message } from "antd";
import { useAppSelector, useAppDispatch } from "../app/hooks";
import { selectAuth } from "../app/store";
import { updateProfileAsync } from "../features/user/authSlice";

export const EditProfile: React.FC = () => {
  const dispatch = useAppDispatch();
  const auth = useAppSelector(selectAuth);
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (auth.user) {
      form.setFieldsValue({
        userName: auth.user.UserName,
        // email: auth.user.email,
        // phoneNumber: auth.user.phoneNumber,
      });
    }
  }, [auth.user, form]);

  const onFinish = async (values: any) => {
    setLoading(true);
    try {
      // Dispatch your update profile action here
      await dispatch(updateProfileAsync(values)).unwrap();
      message.success("Profile updated!");
    } catch (e) {
      message.error("Failed to update profile.");
    }
    setLoading(false);
  };

  if (!auth.user) {
    return <Spin />;
  }

  return (
    <div style={{ maxWidth: 400, margin: "40px auto" }}>
      <h2>Edit Profile</h2>
      <Form form={form} layout="vertical" onFinish={onFinish}>
        <Form.Item label="Username" name="userName" rules={[{ required: true }]}>
          <Input />
        </Form.Item>
        <Form.Item label="Email" name="email" rules={[{ required: true, type: "email" }]}>
          <Input />
        </Form.Item>
        <Form.Item label="Phone Number" name="phoneNumber">
          <Input />
        </Form.Item>
        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading} style={{ float: "right" }}>
            Save
          </Button>
        </Form.Item>
      </Form>
    </div>
  );
};

export default EditProfile;