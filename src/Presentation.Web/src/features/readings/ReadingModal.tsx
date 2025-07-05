import React from "react";
import { Modal, Form, InputNumber, DatePicker, message, Select } from "antd";
import { addReading, updateReading, type Reading } from "./readingAPI";
import { type Sensor } from "../sensors/sensorAPI";

interface ReadingModalProps {
    open: boolean;
    isEdit: boolean;
    editingKey: { dateRecordedUtc: string, sensorId: number } | null;
    sensors: Sensor[];
    token?: string;
    onCancel: () => void;
    onSuccess: (reading: Reading, isEdit: boolean, editingKey?: { dateRecordedUtc: string, sensorId: number } | null) => void;
    form: any; // Form instance from parent component
}

const ReadingModal: React.FC<ReadingModalProps> = ({
    open,
    isEdit,
    editingKey,
    sensors,
    token,
    onCancel,
    onSuccess,
    form
}) => {
    const [loading, setLoading] = React.useState(false);

    const handleOk = async () => {
        try {
            setLoading(true);
            const values = await form.validateFields();
            const readingPayload = {
                dateReceivedUtc: new Date().toISOString(),
                dateRecordedUtc: values.dateRecordedUtc.toISOString(),
                sensorId: values.sensorId,
                value: values.value,
            };

            if (isEdit && editingKey) {
                await updateReading(editingKey.dateRecordedUtc, editingKey.sensorId, readingPayload, token);
                const updatedReading = { 
                    ...readingPayload, 
                    sensor: sensors.find(s => s.id === readingPayload.sensorId)! 
                };
                onSuccess(updatedReading, true, editingKey);
                message.success("Reading updated");
            } else {
                const added = await addReading({ ...readingPayload, sensor: null }, token);
                onSuccess(added, false);
                message.success("Reading added");
            }
            
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save reading");
        } finally {
            setLoading(false);
        }
    };

    const handleCancel = () => {
        form.resetFields();
        onCancel();
    };

    return (
        <Modal
            title={isEdit ? "Edit Reading" : "Add Reading"}
            open={open}
            onCancel={handleCancel}
            onOk={handleOk}
            okText="Save"
            confirmLoading={loading}
            destroyOnHidden={true}
        >
            <Form
                layout="vertical"
                form={form}
                initialValues={{
                    value: undefined,
                }}
            >
                <Form.Item
                    label="Date Recorded"
                    name="dateRecordedUtc"
                    rules={[{ required: true, message: "Please select the recorded date" }]}
                >
                    <DatePicker showTime style={{ width: "100%" }} />
                </Form.Item>
                <Form.Item
                    label="Sensor"
                    name="sensorId"
                    rules={[{ required: true, message: "Please select a sensor" }]}
                >
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select a sensor"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {sensors.map(sensor => (
                            <Select.Option key={sensor.id} value={sensor.id}>
                                {sensor.name ?? sensor.id}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>
                <Form.Item label="Value" name="value">
                    <InputNumber style={{ width: "100%" }} />
                </Form.Item>
            </Form>
        </Modal>
    );
};

export default ReadingModal;
