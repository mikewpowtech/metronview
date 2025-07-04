import React, { useEffect, useState } from "react";
import { Modal, Input, Form, InputNumber, message, Select } from "antd";
import {
    fetchSensors,
    addSensor,
    updateSensor,
    deleteSensor,
    type Sensor,
    fetchSensorsByCompany,
    fetchSensorsByAlarm,
    fetchSensorsByUnit
} from "../features/sensors/sensorAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchAlarms, type Alarm } from "../features/alarms/alarmAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { getSensorColumns } from "../features/sensors/sensorColumns";

export interface SensorListProps {
    unitId?: number;
    companyId?: number;
    alarmId?: number;
}

const SensorList: React.FC<SensorListProps> = ({ unitId, companyId, alarmId }) => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [sensors, setSensors] = useState<Sensor[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    // Add this state for companies and alarms
    const [companies, setCompanies] = useState<Company[]>([]);
    const [alarms, setAlarms] = useState<Alarm[]>([]);

    useEffect(() => {
        loadSensors();
        fetchCompanies(token)
            .then(setCompanies)
            .catch(() => setCompanies([]));
        fetchAlarms(token)
            .then(setAlarms)
            .catch(() => setAlarms([]));
        // eslint-disable-next-line
    }, [token]);

    const loadSensors = async () => {
        setLoading(true);
        try {
            if(unitId){
                const data = await fetchSensorsByUnit(unitId, token);
                setSensors(data);
            }
            else if (companyId) {
                const data = await fetchSensorsByCompany(companyId, token);
                setSensors(data);
            }
            else if (alarmId) {
                const data = await fetchSensorsByAlarm(alarmId, token);
                setSensors(data);
            }else{
            const data = await fetchSensors(token);
            setSensors(data);
            }
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching sensors.");
            setSensors([]);
        } finally {
            setLoading(false);
        }
    };

    const handleAdd = () => {
        setIsEdit(false);
        setEditingId(null);
        form.resetFields();
        setShowModal(true);
    };

    const handleEdit = (record: Sensor) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            name: record.name ?? "",
            channel: record.channel,
            channelType: record.channelType,
            lowValue: record.lowValue,
            highValue: record.highValue,
            engineeringUnits: record.engineeringUnits ?? "",
            unitId: record.unitId ?? "",
            companyID: record.companyID ?? "",
            alarmId: record.alarmId ?? "",
        });
        setShowModal(true);
    };

    const handleDelete = async (record: Sensor) => {
        try {
            if (record.id) {
                await deleteSensor(record.id, token);
                setSensors(prev => prev.filter(s => s.id !== record.id));
                message.success("Sensor deleted");
            } else {
                message.error("Sensor ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete sensor");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            if (isEdit && editingId !== null) {
                values.UnitId = editingId;
                await updateSensor(editingId, values, token);
                setSensors(prev =>
                    prev.map(s =>
                        s.id === editingId ? { ...s, ...values } : s
                    )
                );
                message.success("Sensor updated");
            } else {
                const added = await addSensor(values, token);
                setSensors(prev => [...prev, added]);
                message.success("Sensor added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save sensor");
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingId(null);
        form.resetFields();
    };

    const MainModal: React.FC = () => (
        <Modal
            title={isEdit ? "Edit Sensor" : "Add Sensor"}
            open={showModal}
            onCancel={handleModalCancel}
            onOk={handleModalOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
        >
            <Form
                layout="vertical"
                form={form}
                initialValues={{
                    name: "",
                    channel: 0,
                    channelType: undefined,
                    lowValue: undefined,
                    highValue: undefined,
                    engineeringUnits: "",
                    unitId: "",
                    companyID: "",
                    alarmId: "",
                }}
            >
                <Form.Item
                    label="Name"
                    name="name"
                    rules={[{ required: true, message: "Please enter a name" }]}
                >
                    <Input placeholder="Sensor Name" />
                </Form.Item>
                <Form.Item
                    label="Channel"
                    name="channel"
                    rules={[{ required: true, message: "Please enter a channel" }]}
                >
                    <InputNumber min={0} style={{ width: "100%" }} placeholder="Channel" />
                </Form.Item>
                <Form.Item label="Channel Type" name="channelType">
                    <InputNumber min={0} style={{ width: "100%" }} placeholder="Channel Type" />
                </Form.Item>
                <Form.Item label="Low Value" name="lowValue">
                    <InputNumber style={{ width: "100%" }} placeholder="Low Value" />
                </Form.Item>
                <Form.Item label="High Value" name="highValue">
                    <InputNumber style={{ width: "100%" }} placeholder="High Value" />
                </Form.Item>
                <Form.Item label="Engineering Units" name="engineeringUnits">
                    <Input placeholder="Engineering Units" />
                </Form.Item>
                <Form.Item label="Unit ID" name="unitId">
                    <Input placeholder="Unit ID" />
                </Form.Item>
                <Form.Item label="Company ID" name="companyID">
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select a company"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {companies.map(company => (
                            <Select.Option key={company.id} value={company.id}>
                                {company.name}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>
                <Form.Item label="Alarm ID" name="alarmId">
                    <Select
                        showSearch
                        allowClear
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
            </Form>
        </Modal>
    );

    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal />
            <ExtendedAntDTable<Sensor>
                data={sensors}
                tableColumns={getSensorColumns()}
                title="Sensors"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                loading={loading}
            />
        </div>
    );
};

export default SensorList;
