import { useEffect, useState } from "react";
import { Modal, Input, Form, message } from "antd";
import {
    fetchUnitModels,
    addUnitModel,
    updateUnitModel,
    deleteUnitModel,
    type UnitModel
} from "../features/unitmodels/unitModelAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/ExtendedAntDTable";

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 80 },
    { title: "Code", dataIndex: "code", key: "code", width: 120 },
    { title: "Name", dataIndex: "name", key: "name", width: 150 },
    { title: "Description", dataIndex: "description", key: "description", width: 200 },
];

const UnitModelList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [unitModels, setUnitModels] = useState<UnitModel[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    useEffect(() => {
        loadUnitModels();
        // eslint-disable-next-line
    }, [token]);

    const loadUnitModels = async () => {
        setLoading(true);
        try {
            const data = await fetchUnitModels(token);
            setUnitModels(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching unit models.");
            setUnitModels([]);
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

    const handleEdit = (record: UnitModel) => {
        setIsEdit(true);
        setEditingId(record.id);
        form.setFieldsValue({
            code: record.code,
            name: record.name,
            description: record.description ?? "",
        });
        setShowModal(true);
    };

    const handleDelete = async (id: number) => {
        try {
            await deleteUnitModel(id, token);
            setUnitModels(prev => prev.filter(u => u.id !== id));
            message.success("Unit model deleted");
        } catch (err: any) {
            message.error(err.message || "Failed to delete unit model");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            if (isEdit && editingId !== null) {
                values.Id = editingId;
                await updateUnitModel(editingId, values, token);
                setUnitModels(prev =>
                    prev.map(u =>
                        u.id === editingId ? { ...u, ...values } : u
                    )
                );
                message.success("Unit model updated");
            } else {
                const added = await addUnitModel(values, token);
                setUnitModels(prev => [...prev, added]);
                message.success("Unit model added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save unit model");
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingId(null);
        form.resetFields();
    };


    if (loading) return <div>Loading unit models...</div>;
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    const MainModal: React.FC = () => (
        <Modal
            title={isEdit ? "Edit Unit Model" : "Add Unit Model"}
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
                initialValues={{ code: "", name: "", description: "" }}
            >
                <Form.Item
                    label="Code"
                    name="code"
                    rules={[{ required: true, message: "Please enter a code" }]}
                >
                    <Input placeholder="Code" />
                </Form.Item>
                <Form.Item
                    label="Name"
                    name="name"
                    rules={[{ required: true, message: "Please enter a name" }]}
                >
                    <Input placeholder="Name" />
                </Form.Item>
                <Form.Item label="Description" name="description">
                    <Input placeholder="Description" />
                </Form.Item>
            </Form>
        </Modal>
    );

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal />
            <ExtendedAntDTable<UnitModel>
                data={unitModels}
                tableColumns={allColumnDefs}
                title="Unit Models"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
            />
        </div>
    );
};

export default UnitModelList;