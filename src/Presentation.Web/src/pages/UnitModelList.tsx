import { useEffect, useState } from "react";
import { Form, message } from "antd";
import {
    fetchUnitModels,
    addUnitModel,
    updateUnitModel,
    deleteUnitModel,
    type UnitModel
} from "../features/unitmodels/unitModelAPI";
import { UnitModelModal } from "../features/unitmodels/UnitModelModal";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { getColumns } from "../features/unitmodels/unitModelColums";

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
        console.log('handleEdit - Editing unit model record:', record);
        
        // Reset form and immediately set values
        form.resetFields();
        const formValues = {
            code: record.code ?? "",
            name: record.name ?? "",
            description: record.description ?? "",
        };
        console.log('handleEdit - Setting form values:', formValues);
        form.setFieldsValue(formValues);
        
        setShowModal(true);
        
        // Check form values after modal opens
        setTimeout(() => {
            const currentValues = form.getFieldsValue();
            console.log('handleEdit - Form values after modal opens:', currentValues);
        }, 100);
    };

    const handleDelete = async (record: UnitModel) => {
        try {
            if (record.id) {
                await deleteUnitModel(record.id, token);
                setUnitModels(prev => prev.filter(u => u.id !== record.id));
                message.success("Unit model deleted");
            } else {
                message.error("Unit model ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete unit model");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            console.log('Form values when submitting:', values);
            
            if (isEdit && editingId !== null) {
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
            console.error('API Error:', err.response?.data || err.message);
            message.error(err.response?.data?.message || err.response?.data || err.message || "Failed to save unit model");
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingId(null);
        form.resetFields();
    };

    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }}>
            <UnitModelModal
                showModal={showModal}
                isEdit={isEdit}
                modalLoading={modalLoading}
                form={form}
                onOk={handleModalOk}
                onCancel={handleModalCancel}
            />
            <ExtendedAntDTable<UnitModel>
                data={unitModels}
                tableColumns={getColumns()}
                title="Unit Types"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                loading={loading}
            />
        </div>
    );
};

export default UnitModelList;