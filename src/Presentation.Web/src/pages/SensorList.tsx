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
            companyId: record.companyId ?? "",    // Changed from companyID
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
            
            // Normalize values to handle null/undefined
            const normalizedValues = {
                ...values,
                unitId: Number(values.unitId) || null,
                companyId: values.companyId || null,
                alarmId: values.alarmId || null,
                channelType: values.channelType || null,
                lowValue: values.lowValue || null,
                highValue: values.highValue || null,
            };
            
            if (isEdit && editingId !== null) {
                const sensorData = {
                    ...normalizedValues,
                    id: editingId
                };
                
                await updateSensor(editingId, sensorData, token);
                setSensors(prev =>
                    prev.map(s =>
                        s.id === editingId ? { ...s, ...normalizedValues } : s
                    )
                );
                message.success("Sensor updated");
            } else {
                const added = await addSensor(normalizedValues, token);
                setSensors(prev => [...prev, added]);
                message.success("Sensor added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return;
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
                    <div style={{
                        width: '24px',
                        height: '24px',
                        borderRadius: '50%',
                        background: 'linear-gradient(135deg, #1890ff 0%, #40a9ff 100%)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: 'white',
                        fontSize: '12px',
                        fontWeight: 'bold'
                    }}>
                        {isEdit ? "E" : "S"}
                    </div>
                    {isEdit ? "Edit Sensor" : "Add Sensor"}
                </div>
            }
            open={showModal}
            onCancel={handleModalCancel}
            onOk={handleModalOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
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
                }}>×</span>
            }
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
                    companyId: "",
                    alarmId: "",
                }}
            >
                <Form.Item
                    label="Name"
                    name="name"
                    rules={[{ required: true, message: "Please enter a name" }]}
                    style={{ marginBottom: '16px' }}
                >
                    <Input placeholder="Sensor Name" />
                </Form.Item>
                
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                    <Form.Item
                        label="Channel"
                        name="channel"
                        rules={[{ required: true, message: "Please enter a channel" }]}
                        style={{ marginBottom: 0 }}
                    >
                        <InputNumber min={0} style={{ width: "100%" }} placeholder="Channel" />
                    </Form.Item>
                    <Form.Item 
                        label="Channel Type" 
                        name="channelType"
                        style={{ marginBottom: 0 }}
                    >
                        <InputNumber min={0} style={{ width: "100%" }} placeholder="Channel Type" />
                    </Form.Item>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                    <Form.Item 
                        label="Low Value" 
                        name="lowValue"
                        style={{ marginBottom: 0 }}
                    >
                        <InputNumber style={{ width: "100%" }} placeholder="Low Value" />
                    </Form.Item>
                    <Form.Item 
                        label="High Value" 
                        name="highValue"
                        style={{ marginBottom: 0 }}
                    >
                        <InputNumber style={{ width: "100%" }} placeholder="High Value" />
                    </Form.Item>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
                    <Form.Item
                        label="Unit ID"
                        name="unitId"
                        rules={[{ required: true, message: "Please select a unit" }]}
                        style={{ marginBottom: 0 }}
                    >
                        <InputNumber min={0} style={{ width: "100%" }} placeholder="Unit ID" />
                    </Form.Item>
                    
                    <Form.Item 
                        label="Engineering Units" 
                        name="engineeringUnits"
                        style={{ marginBottom: 0 }}
                    >
                        <Input placeholder="Engineering Units" />
                    </Form.Item>
                </div>

                <Form.Item
                    label="Company"
                    name="companyId"
                    rules={[{ required: true, message: "Please select a company" }]}
                    style={{ marginBottom: '16px' }}
                >
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

                <Form.Item
                    label="Alarm"
                    name="alarmId"
                    rules={[{ required: true, message: "Please select an alarm" }]}
                    style={{ marginBottom: 0 }}
                >
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
