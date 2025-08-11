import { useEffect, useState } from "react";
import { Modal, Input, Form, message, Switch, InputNumber, Select } from "antd";
import {
    fetchRecipients,
    addRecipient,
    updateRecipient,
    deleteRecipient,
    type Recipient
} from "../features/recipients/recipientAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/ExtendedAntDTable";

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 80 },
    { title: "Name", dataIndex: "name", key: "name", width: 180 },
    { title: "Email", dataIndex: "email", key: "email", width: 180 },
    { title: "SMS", dataIndex: "sms", key: "sms", width: 120 },
    { title: "Web Service Root", dataIndex: "webServiceRoot", key: "webServiceRoot", width: 180 },
    { title: "Company", dataIndex: "companyId", key: "companyId", width: 120 },
    { title: "Enabled", dataIndex: "isEnabled", key: "isEnabled", width: 80 },
];

const RecipientList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [recipients, setRecipients] = useState<Recipient[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    // Companies for company selection
    const [companies, setCompanies] = useState<Company[]>([]);

    useEffect(() => {
        loadRecipients();
        fetchCompanies(token)
            .then(setCompanies)
            .catch(() => setCompanies([]));
        // eslint-disable-next-line
    }, [token]);

    const loadRecipients = async () => {
        setLoading(true);
        try {
            const data = await fetchRecipients(token);
            setRecipients(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching recipients.");
            setRecipients([]);
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

    const handleEdit = (record: Recipient) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            name: record.name ?? "",
            email: record.email ?? "",
            sms: record.sms ?? "",
            webServiceRoot: record.webServiceRoot ?? "",
            companyId: record.companyId ?? "",
            unitId: record.unitId ?? "",
            isEnabled: record.isEnabled ?? true,
        });
        setShowModal(true);
    };

    const handleDelete = async (record: Recipient) => {
        try {
            if (record.id) {
                await deleteRecipient(record.id, token);
                setRecipients(prev => prev.filter(r => r.id !== record.id));
                message.success("Recipient deleted");
            } else {
                message.error("Recipient ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete recipient");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            if (isEdit && editingId !== null) {
                await updateRecipient(editingId, { ...values, id: editingId }, token);
                setRecipients(prev =>
                    prev.map(r =>
                        r.id === editingId ? { ...r, ...values } : r
                    )
                );
                message.success("Recipient updated");
            } else {
                const added = await addRecipient(values, token);
                setRecipients(prev => [...prev, added]);
                message.success("Recipient added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save recipient");
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
            title={isEdit ? "Edit Recipient" : "Add Recipient"}
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
                    email: "",
                    sms: "",
                    webServiceRoot: "",
                    companyId: "",
                    unitId: "",
                    isEnabled: true,
                }}
            >
                <Form.Item
                    label="Name"
                    name="name"
                    rules={[{ required: true, message: "Please enter a name" }]}
                >
                    <Input placeholder="Recipient Name" />
                </Form.Item>
                <Form.Item
                    label="Email"
                    name="email"
                >
                    <Input placeholder="Email" />
                </Form.Item>
                <Form.Item
                    label="SMS"
                    name="sms"
                >
                    <Input placeholder="SMS" />
                </Form.Item>
                <Form.Item
                    label="Web Service Root"
                    name="webServiceRoot"
                >
                    <Input placeholder="Web Service Root" />
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
                    label="Unit ID"
                    name="unitId"
                >
                    <InputNumber min={0} style={{ width: "100%" }} placeholder="Unit ID" />
                </Form.Item>
                <Form.Item
                    label="Enabled"
                    name="isEnabled"
                    valuePropName="checked"
                >
                    <Switch />
                </Form.Item>
            </Form>
        </Modal>
    );

    // Column mapping for company name and enabled switch
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
        if (col.key === "isEnabled") {
            return {
                ...col,
                render: (isEnabled: boolean) => (
                    <span>{isEnabled ? "Yes" : "No"}</span>
                )
            };
        }
        return col;
    };

    if (loading) return <div>Loading recipients...</div>;
    if (error) return <div style={{ color: "red" }}>{error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal />
            <ExtendedAntDTable<Recipient>
                data={recipients}
                tableColumns={allColumnDefs}
                title="Recipients"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                columnMapper={columnMapper}
            />
        </div>
    );
};

export default RecipientList;