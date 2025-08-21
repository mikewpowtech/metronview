import React from "react";
import { Modal, Input, Form, InputNumber, Select, Switch, FormInstance } from "antd";
import { PlusOutlined } from "@ant-design/icons";
import { type TriggerType, type CommunicationMode } from "./triggerAPI";
import { type Alarm } from "../alarms/alarmAPI";

const { TextArea } = Input;

// Helper function to convert trigger code to ASCII character
const convertToAsciiChar = (code: string | number): string => {
    if (typeof code === 'string') {
        // If it's already a string, check if it's a single character
        if (code.length === 1) {
            return code; // Already an ASCII character
        }
        // If it's a string representation of a number, convert it
        const numCode = parseInt(code, 10);
        if (!isNaN(numCode)) {
            return String.fromCharCode(numCode);
        }
        return code; // Return as-is if we can't convert
    } else if (typeof code === 'number') {
        // Convert numeric ASCII code to character
        return String.fromCharCode(code);
    }
    return String(code); // Fallback to string conversion
};

export interface TriggerModalProps {
    showModal: boolean;
    isEdit: boolean;
    modalLoading: boolean;
    form: FormInstance;
    alarms: Alarm[];
    triggerTypes: TriggerType[];
    communicationModes: CommunicationMode[];
    alarmId?: number;
    onOk: () => Promise<void>;
    onCancel: () => void;
}

export const TriggerModal: React.FC<TriggerModalProps> = ({
    showModal,
    isEdit,
    modalLoading,
    form,
    alarms,
    triggerTypes,
    communicationModes,
    alarmId,
    onOk,
    onCancel
}) => {
    return (
        <Modal
            title={
                <div style={{
                    fontSize: '16px',
                    fontWeight: 600,
                    color: '#ffffff',
                    padding: '4px 0',
                    marginBottom: '8px',
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px'
                }}>
                    <PlusOutlined style={{ color: '#ffffff' }} />
                    {isEdit ? "Edit Trigger" : "Add Trigger"}
                </div>
            }
            open={showModal}
            onCancel={onCancel}
            onOk={onOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
            width={530}
            styles={{
                header: {
                    background: '#1890ff',
                    borderRadius: '8px 8px 0 0',
                    padding: '16px 24px',
                    border: '2px solid #1890ff',
                    borderBottom: 'none',
                    marginBottom: '0'
                },
                body: {
                    background: '#ffffff',
                    padding: '16px 24px',
                    border: '2px solid #1890ff',
                    borderTop: 'none',
                    borderBottom: 'none',
                    marginTop: '0'
                },
                footer: {
                    background: '#ffffff',
                    padding: '16px 24px',
                    border: '2px solid #1890ff',
                    borderTop: 'none',
                    borderRadius: '0 0 8px 8px',
                    marginTop: '0'
                },
                content: {
                    padding: '0',
                    overflow: 'hidden',
                    borderRadius: '8px',
                    border: 'none'
                }
            }}
            closeIcon={
                <span style={{
                    color: 'white',
                    fontWeight: 'bold',
                    fontSize: '16px'
                }}>x</span>
            }
        >
            <Form
                layout="vertical"
                form={form}
                size="small"
                initialValues={{
                    alarmId: alarmId || undefined,
                    triggerValue: 0,
                    subject: "",
                    body: "",
                    minimumSendIntervalMinutes: 0,
                    isEnabled: false, // Default to disabled for add modal
                }}
                style={{ marginTop: 16 }}
            >
                <Form.Item
                    label="Alarm"
                    name="alarmId"
                    rules={[{ required: true, message: "Please select an alarm" }]}
                    style={{ marginBottom: 12 }}
                >
                    <Select
                        showSearch
                        placeholder="Select an alarm"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {alarms.map(alarm => (
                            <Select.Option key={alarm.id} value={alarm.id}>
                                {alarm.name}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>

                <div style={{ display: 'flex', gap: 8 }}>
                    <Form.Item
                        label="Trigger Type"
                        name="triggerTypeId"
                        rules={[{ required: true, message: "Please select a trigger type" }]}
                        style={{ marginBottom: 12, flex: 1 }}
                    >
                        <Select
                            showSearch
                            placeholder="Select trigger type"
                            optionFilterProp="children"
                            filterOption={(input, option) =>
                                typeof option?.children === "string" &&
                                (option.children as string).toLowerCase().includes(input.toLowerCase())
                            }
                        >
                            {triggerTypes.map(triggerType => (
                                <Select.Option key={triggerType.id} value={triggerType.id}>
                                    {convertToAsciiChar(triggerType.code)} - {triggerType.name}
                                </Select.Option>
                            ))}
                        </Select>
                    </Form.Item>

                    <Form.Item
                        label="Value"
                        name="triggerValue"
                        rules={[{ required: true, message: "Please enter a trigger value" }]}
                        style={{ marginBottom: 12, width: 100 }}
                    >
                        <InputNumber
                            style={{ width: "100%" }}
                            placeholder="Value"
                        />
                    </Form.Item>
                </div>

                <Form.Item
                    label="Communication Mode"
                    name="communicationModeId"
                    rules={[{ required: true, message: "Please select a communication mode" }]}
                    style={{ marginBottom: 12 }}
                >
                    <Select
                        showSearch
                        placeholder="Select communication mode"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {communicationModes.map(mode => (
                            <Select.Option key={mode.id} value={mode.id}>
                                {mode.code} - {mode.name}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>

                <div style={{ display: 'flex', gap: 8 }}>
                    <Form.Item 
                        label="Subject" 
                        name="subject"
                        style={{ marginBottom: 12, flex: 1 }}
                    >
                        <Input placeholder="Email/SMS Subject" />
                    </Form.Item>

                    <Form.Item
                        label="Send Interval (min)"
                        name="minimumSendIntervalMinutes"
                        style={{ marginBottom: 12, width: 150 }}
                    >
                        <InputNumber
                            min={0}
                            style={{ width: "100%" }}
                            placeholder="Minutes"
                        />
                    </Form.Item>
                </div>

                <Form.Item 
                    label="Message Body" 
                    name="body"
                    style={{ marginBottom: 12 }}
                >
                    <TextArea
                        rows={3}
                        placeholder="Message body content"
                    />
                </Form.Item>

                <Form.Item shouldUpdate>
                    {({ getFieldValue }) => {
                        const isEnabled = getFieldValue('isEnabled') ?? false; // Provide default value
                        console.log('Trigger Switch render - isEnabled value:', isEnabled, 'type:', typeof isEnabled);
                        console.log('Trigger Switch render - all form values:', form.getFieldsValue());
                        return (
                            <Form.Item 
                                label="Status"
                                name="isEnabled" 
                                valuePropName="checked"
                                style={{ marginBottom: 0 }}
                            >
                                <Switch 
                                    size="default"
                                    checkedChildren="Enabled" 
                                    unCheckedChildren="Disabled"
                                    style={{
                                        backgroundColor: isEnabled ? '#52c41a' : '#ff4d4f',
                                        transform: 'scale(1.2)', // Make it 20% bigger
                                        transformOrigin: 'left center', // Scale from left edge to maintain alignment
                                        minWidth: '80px' // Ensure minimum width for text
                                    }}
                                />
                            </Form.Item>
                        );
                    }}
                </Form.Item>
            </Form>
        </Modal>
    );
};