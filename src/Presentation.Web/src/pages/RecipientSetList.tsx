import { useEffect, useState } from "react";
import { Modal, Input, Form, message, Select } from "antd";
import {
    fetchRecipientSets,
    addRecipientSet,
    updateRecipientSet,
    deleteRecipientSet,
    type RecipientSet
} from "../features/recipientSets/recipientSetAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/ExtendedAntDTable";

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 80 },
    { title: "Name", dataIndex: "name", key: "name", width: 200 },
    { title: "Company", dataIndex: "companyId", key: "companyId", width: 150 },
];

const RecipientSetList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [recipientSets, setRecipientSets] = useState<RecipientSet[]>([]);
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
        loadRecipientSets();
        fetchCompanies(token)
            .then(setCompanies)
            .catch(() => setCompanies([]));
        // eslint-disable-next-line
    }, [token]);

    const loadRecipientSets = async () => {
        setLoading(true);
        try {
            const data = await fetchRecipientSets(token);
            setRecipientSets(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching recipient sets.");
            setRecipientSets([]);
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

    const handleEdit = (record: RecipientSet) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            name: record.name ?? "",
            companyId: record.companyId ?? "",
        });
        setShowModal(true);
    };

    const handleDelete = async (record: RecipientSet) => {
        try {
            if (record.id) {
                await deleteRecipientSet(record.id, token);
                setRecipientSets(prev => prev.filter(r => r.id !== record.id));
                message.success("Recipient set deleted");
            } else {
                message.error("Recipient set ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete recipient set");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            if (isEdit && editingId !== null) {
                await updateRecipientSet(editingId, { ...values, id: editingId }, token);
                setRecipientSets(prev =>
                    prev.map(r =>
                        r.id === editingId ? { ...r, ...values } : r
                    )
                );
                message.success("Recipient set updated");
            } else {
                const added = await addRecipientSet(values, token);
                setRecipientSets(prev => [...prev, added]);
                message.success("Recipient set added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save recipient set");
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
            title={isEdit ? "Edit Recipient Set" : "Add Recipient Set"}
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
                }}
            >
                <Form.Item
                    label="Name"
                    name="name"
                    rules={[{ required: true, message: "Please enter a name" }]}
                >
                    <Input placeholder="Recipient Set Name" />
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
            </Form>
        </Modal>
    );

    // Column mapping for company name
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
        return col;
    };

    if (loading) return <div>Loading recipient sets...</div>;
    if (error) return <div style={{ color: "red" }}>{error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal />
            <ExtendedAntDTable<RecipientSet>
                data={recipientSets}
                tableColumns={allColumnDefs}
                title="Recipient Sets"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                columnMapper={columnMapper}
            />
        </div>
    );
};

export default RecipientSetList;