import { useEffect, useState } from "react";
import { Modal, Input, Form, message, Switch, Select } from "antd";
import {
    fetchAlarms,
    addAlarm,
    updateAlarm,
    deleteAlarm,
    type Alarm
} from "../features/alarms/alarmAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchRecipientSets, type RecipientSet } from "../features/recipientSets/recipientSetAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/ExtendedAntDTable";

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Name", dataIndex: "name", key: "name", width: 200 },
    { title: "Company", dataIndex: "companyId", key: "companyId", width: 150 },
    { title: "RecipientSet", dataIndex: "recipientSetId", key: "recipientSetId", width: 120 },
    { title: "Active", dataIndex: "isActive", key: "isActive", width: 80 },
];

const AlarmList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    // Companies and recipient sets for selection
    const [companies, setCompanies] = useState<Company[]>([]);
    const [recipientSets, setRecipientSets] = useState<RecipientSet[]>([]);

    useEffect(() => {
        loadAlarms();
        fetchCompanies(token)
            .then(setCompanies)
            .catch(() => setCompanies([]));
        fetchRecipientSets(token)
            .then(setRecipientSets)
            .catch(() => setRecipientSets([]));
        // eslint-disable-next-line
    }, [token]);

    const loadAlarms = async () => {
        setLoading(true);
        try {
            const data = await fetchAlarms(token);
            setAlarms(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching alarms.");
            setAlarms([]);
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

    const handleEdit = (record: Alarm) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            name: record.name ?? "",
            companyId: record.companyId ?? "",
            recipientSetId: record.recipientSetId ?? "",
            isActive: record.isActive ?? true,
        });
        setShowModal(true);
    };

    const handleDelete = async (record: Alarm) => {
        try {
            if (record.id) {
                await deleteAlarm(record.id, token);
                setAlarms(prev => prev.filter(a => a.id !== record.id));
                message.success("Alarm deleted");
            } else {
                message.error("Alarm ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete alarm");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            if (isEdit && editingId !== null) {
                await updateAlarm(editingId, { ...values, id: editingId }, token);
                setAlarms(prev =>
                    prev.map(a =>
                        a.id === editingId ? { ...a, ...values } : a
                    )
                );
                message.success("Alarm updated");
            } else {
                const added = await addAlarm(values, token);
                setAlarms(prev => [...prev, added]);
                message.success("Alarm added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save alarm");
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
            title={isEdit ? "Edit Alarm" : "Add Alarm"}
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
                    companyId: "",
                    recipientSetId: "",
                    order: 0,
                    isActive: true,
                }}
            >
                <Form.Item
                    label="Name"
                    name="name"
                    rules={[{ required: true, message: "Please enter a name" }]}
                >
                    <Input placeholder="Alarm Name" />
                </Form.Item>
                <Form.Item
                    label="Company"
                    name="companyId"
                    rules={[{ required: true, message: "Please select a company" }]}
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
                    label="Recipient Set"
                    name="recipientSetId"
                    rules={[{ required: true, message: "Please select a recipient set" }]}
                >
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select a recipient set"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {recipientSets.map(rs => (
                            <Select.Option key={rs.id} value={rs.id}>
                                {rs.name}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>
                <Form.Item
                    label="Active"
                    name="isActive"
                    valuePropName="checked"
                >
                    <Switch />
                </Form.Item>
            </Form>
        </Modal>
    );

    // Column mapping for company name, recipient set name, and active switch
    const columnMapper = (col: any) => {
        if (col.key === "companyId") {
            return {
                ...col,
                render: (companyId: number) => {
                    const company = companies.find(c => c.id === companyId);
                    return company ? company.name : companyId;
                }
            };
        }
        if (col.key === "recipientSetId") {
            return {
                ...col,
                render: (recipientSetId: number) => {
                    const rs = recipientSets.find(r => r.id === recipientSetId);
                    return rs ? rs.name : recipientSetId;
                }
            };
        }
        if (col.key === "isActive") {
            return {
                ...col,
                render: (isActive: boolean) => (
                    <span>{isActive ? "Yes" : "No"}</span>
                )
            };
        }
        return col;
    };

    if (loading) return <div>Loading alarms...</div>;
    if (error) return <div style={{ color: "red" }}>{error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal />
            <ExtendedAntDTable<Alarm>
                data={alarms}
                tableColumns={allColumnDefs}
                title="Alarms"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                columnMapper={columnMapper}
            />
        </div>
    );
};

export default AlarmList;