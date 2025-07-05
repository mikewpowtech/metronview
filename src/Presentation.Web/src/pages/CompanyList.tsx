/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from "react";
import { Modal, Input, Form, message, Select } from "antd";
import {
    fetchCompanies,
    addCompany,
    updateCompany,
    type Company
} from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import type { ColumnsType } from "antd/es/table";

const companyColumnDefinitions: ColumnsType<Company> =  [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Company Name", dataIndex: "name", key: "name", width: 200 },
    { title: "Parent Company", dataIndex: "parentCompanyId", key: "parentCompanyId", width: 200 },
];

const CompanyList: React.FC = () => {

    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [companies, setCompanies] = useState<Company[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    useEffect(() => {
        loadCompanies();
        // eslint-disable-next-line
    }, [token]);

    const loadCompanies = async () => {
        setLoading(true);
        try {
            const data = await fetchCompanies(token);
            setCompanies(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching companies.");
            setCompanies([]);
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

    const handleEdit = (record: Company) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            name: record.name,
            parentCompanyId: record.parentCompanyId ?? undefined,
        });
        setShowModal(true);
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            values.Id = editingId ?? 0; // Ensure Id is set for updates
            if (isEdit && editingId !== null) {
                await updateCompany(editingId, values, token);
                setCompanies(prev =>
                    prev.map(c =>
                        c.id === editingId ? { ...c, ...values } : c
                    )
                );
                message.success("Company updated");
            } else {
                const added = await addCompany(values.name, values.parentCompanyId ?? null, token);
                setCompanies(prev => [...prev, added]);
                message.success("Company added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save company");
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
                        {isEdit ? "E" : "C"}
                    </div>
                    {isEdit ? "Edit Company" : "Add Company"}
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
                    parentCompanyId: undefined,
                }}
            >
                <Form.Item
                    label="Name"
                    name="name"
                    rules={[{ required: true, message: "Please enter a name" }]}
                >
                    <Input placeholder="Company Name" />
                </Form.Item>
                <Form.Item label="Parent Company" name="parentCompanyId">
                    <Select
                        allowClear
                        showSearch
                        placeholder="Select a parent company"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {companies
                            .filter(c => !isEdit || c.id !== editingId) // Prevent selecting self as parent
                            .map(company => (
                                <Select.Option key={company.id} value={company.id}>
                                    {company.name}
                                </Select.Option>
                            ))}
                    </Select>
                </Form.Item>
            </Form>
        </Modal>
    )

    const columnMapper = (col: any) => {
        return col.key === "parentCompanyId"
            ? {
                ...col,
                render: (parentCompanyId: number | null) => {
                    if (!parentCompanyId) return "";
                    const parent = companies.find(c => c.id === parentCompanyId);
                    return parent ? parent.name : parentCompanyId;
                }
            }
            : col
    }

    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal/>
            <ExtendedAntDTable<Company>
                data={companies}
                tableColumns={companyColumnDefinitions}
                title="Company"
                onAdd={handleAdd}
                onEdit={handleEdit}
                columnMapper={columnMapper}
                loading={loading}
            />
        </div>
    );

};

export default CompanyList;