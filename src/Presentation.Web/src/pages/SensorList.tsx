import { useEffect, useState } from "react";
import { Modal, Input, Form, InputNumber, message, Select } from "antd";
import {
    fetchSensors,
    addSensor,
    updateSensor,
    deleteSensor,
    type Sensor
} from "../features/sensors/sensorAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/ExtendedAntDTable";

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Name", dataIndex: "name", key: "name", width: 150 },
    { title: "Channel", dataIndex: "channel", key: "channel", width: 50 },
    { title: "Channel Type", dataIndex: "channelType", key: "channelType", width: 75 },
    { title: "Low Value", dataIndex: "lowValue", key: "lowValue", width: 75 },
    { title: "High Value", dataIndex: "highValue", key: "highValue", width: 75 },
    { title: "Engineering Units", dataIndex: "engineeringUnits", key: "engineeringUnits", width: 80 },
    { title: "Unit ID", dataIndex: "unitId", key: "unitId", width: 120 },
    { title: "Company ID", dataIndex: "companyID", key: "companyID", width: 120 },
];

const SensorList: React.FC = () => {
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

    // Add this state for companies
    const [companies, setCompanies] = useState<Company[]>([]);

    useEffect(() => {
        loadSensors();
        fetchCompanies(token)
            .then(setCompanies)
            .catch(() => setCompanies([]));
        // eslint-disable-next-line
    }, [token]);


    const loadSensors = async () => {
        setLoading(true);
        try {
            const data = await fetchSensors(token);
            setSensors(data);
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
            destroyOnClose
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
            </Form>
        </Modal>
    );

    if (loading) return <div>Loading sensors...</div>;
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal />
            <ExtendedAntDTable<Sensor>
                data={sensors}
                tableColumns={allColumnDefs}
                title="Sensors"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
            />
        </div>
    );
};

export default SensorList;