/* eslint-disable @typescript-eslint/no-explicit-any */
import React, { useEffect, useState } from "react";
import { Tooltip, Button, Modal, Input, Form, InputNumber, message, Select } from "antd";
import { DownOutlined, RightOutlined } from "@ant-design/icons";
import { fetchUnits, addUnit, updateUnit, deleteUnit, type Unit } from "../features/units/unitsAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchUnitModels, type UnitModel } from "../features/unitmodels/unitModelAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { getUnitListColumns, unitStatusOptions } from "../features/units/unitListColumns";
import SensorList from "./SensorList";

const UnitList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [units, setUnits] = useState<Unit[]>([]);
    const [companies, setCompanies] = useState<Company[]>([]);
    const [unitModels, setUnitModels] = useState<UnitModel[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);
    const [expandedRowKeys, setExpandedRowKeys] = useState<React.Key[]>([]);

    useEffect(() => {
        loadUnits();
        fetchCompanies(token).then(setCompanies).catch(() => setCompanies([]));
        fetchUnitModels(token).then(setUnitModels).catch(() => setUnitModels([]));
        // eslint-disable-next-line
    }, [token]);

    const loadUnits = async () => {
        setLoading(true);
        try {
            const data = await fetchUnits(token);
            setUnits(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching units.");
            setUnits([]);
        } finally {
            setLoading(false);
        }
    };

    //action button handlers
    const handleAdd = () => {
        setIsEdit(false);
        setEditingId(null);
        form.resetFields();
        setShowModal(true);
    };

    const handleEdit = (record: Unit) => {
        setIsEdit(true);
        setEditingId(record.id);
        const model = unitModels.find(m => m.id === record.unitTypeId);
        form.setFieldsValue({
            unitTypeId: model
                ? { value: model.id, label: `${model.name}` }
                : undefined,
            phoneNumber: record.phoneNumber ?? "",
            pin: record.pin ?? "",
            manufacturerCode: record.manufacturerCode,
            unitCode: record.unitCode ?? "",
            secret: record.secret ?? "",
            status: record.status,
            companyID: record.companyID ?? undefined,
            daysBeforeNotReported: record.daysBeforeNotReported,
            customFieldValues: record.customFieldValues ?? "",
        });
        setShowModal(true);
    };

    const handleDelete = async (record: Unit) => {
        try {
            if (record.id) {
                await deleteUnit(record.id, token);
                setUnits(prev => prev.filter(u => u.id !== record.id));
                message.success("Unit deleted");
            } else {
                message.error("Unit ID is required for deletion");
            }
        } catch (err: any) {
            message.error(err.message || "Failed to delete unit");
        }
    };

    //modal functionality
    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            values.Id = editingId ?? 0; // Ensure Id is set for updates

            if (isEdit && editingId !== null) {
                await updateUnit(editingId, values, token);
                setUnits(prev =>
                    prev.map(u =>
                        u.id === editingId ? { ...u, ...values } : u
                    )
                );
                message.success("Unit updated");
            } else {
                const added = await addUnit(values, token);
                setUnits(prev => [...prev, added]);
                message.success("Unit added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err && typeof err === "object" && "errorFields" in err) {
                return; // Form validation error
            }
            message.error(err.message || "Failed to save unit.");
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingId(null);
        form.resetFields();
    };

    //     setSensorModal({ open: false });
    //     sensorForm.resetFields();
    // };

    // const handleDeleteSensor = async (sensorId: number) => {
    //     try {
    //         await deleteSensor(sensorId, token);
    //         message.success("Sensor deleted");
    //         loadUnits(); // Reload sensors for the unit
    //     } catch (err: any) {
    //         message.error(err.message || "Failed to delete sensor");
    //     }
    // };

    //table functionality
    const handleExpandRow = (record: Unit) => {
        setExpandedRowKeys(keys =>
            keys.includes(record.id)
                ? keys.filter(key => key !== record.id)
                : [...keys, record.id]
        );
    };

    // Sensors table as a ReactNode (function that takes sensors and unit)
    const getSensorsTable = (unit: Unit): React.ReactNode => {
        return(<SensorList unitId={unit.id}></SensorList>);
    };

    const UnitListModal: React.FC = () => (<Modal
        title={isEdit ? "Edit Unit" : "Add Unit"}
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
                unitTypeId: "",
                phoneNumber: "",
                pin: "",
                manufacturerCode: "",
                unitCode: "",
                secret: "",
                unitStatusID: "",
                companyID: undefined,
                daysBeforeNotReported: undefined,
                customFieldValues: "",
            }}
        >
            <Form.Item
                label="Unit Type"
                name="unitTypeId"
                rules={[{ required: true, message: "Please select a unit type" }]}
            >
                <Select
                    showSearch
                    allowClear
                    placeholder="Select a unit type"
                    optionFilterProp="children"
                    filterOption={(input, option) =>
                        typeof option?.children === "string" &&
                        (option.children as string).toLowerCase().includes(input.toLowerCase())
                    }
                >
                    {unitModels.map(model => (
                        <Select.Option key={model.id} value={model.id.toString()}>
                            {model.code} - {model.name}
                        </Select.Option>
                    ))}
                </Select>
            </Form.Item>
            <Form.Item label="Phone Number" name="phoneNumber">
                <Input placeholder="Phone Number" />
            </Form.Item>
            <Form.Item label="PIN" name="pin">
                <Input placeholder="PIN" />
            </Form.Item>
            <Form.Item
                label="Manufacturer Code"
                name="manufacturerCode"
                rules={[{ required: true, message: "Please enter a manufacturer code" }]}
            >
                <Input placeholder="Manufacturer Code" />
            </Form.Item>
            <Form.Item label="Unit Code" name="unitCode">
                <Input placeholder="Unit Code" />
            </Form.Item>
            <Form.Item label="Secret" name="secret">
                <Input placeholder="Secret" />
            </Form.Item>
            <Form.Item
                label="Unit Status"
                name="status"
                rules={[{ required: true, message: "Please select a unit status" }]}
            >
                <Select
                    showSearch
                    allowClear
                    placeholder="Select a unit status"
                    optionFilterProp="children"
                    filterOption={(input, option) =>
                        typeof option?.children === "string" &&
                        (option.children as string).toLowerCase().includes(input.toLowerCase())
                    }
                >
                    {unitStatusOptions.map(status => (
                        <Select.Option key={status.value} value={status.value}>
                            {status.label}
                        </Select.Option>
                    ))}
                </Select>
            </Form.Item>
            <Form.Item label="Company" name="companyID">
                <Select
                    allowClear
                    showSearch
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
            <Form.Item label="Days Before Not Reported" name="daysBeforeNotReported">
                <InputNumber min={0} style={{ width: "100%" }} placeholder="Days Before Not Reported" />
            </Form.Item>
            <Form.Item label="Custom Field Values" name="customFieldValues">
                <Input placeholder="Custom Field Values" />
            </Form.Item>
        </Form>
    </Modal>);

    const customActions = (record: Unit) => (
        <Tooltip title={expandedRowKeys.includes(record.id) ? "Hide Sensors" : "Sensors"}>
            <Button
                icon={expandedRowKeys.includes(record.id) ? <DownOutlined /> : <RightOutlined />}
                size="small"
                style={{ marginLeft: 4, padding: 0, minWidth: 0, width: 28, height: 28 }}
                onClick={() => handleExpandRow(record)}
            />
        </Tooltip>
    );
    
    const unitListColumns = getUnitListColumns({ companies, unitModels });
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <UnitListModal />
            <ExtendedAntDTable<Unit>
                data={units}
                tableColumns={ unitListColumns }
                title="Units"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                customActions={customActions}
                expandable={{
                    expandedRowRender: (unit: Unit) => {
                        return getSensorsTable(unit);
                    },
                    expandedRowKeys,
                    onExpand: (_, record) => handleExpandRow(record),
                    showExpandColumn: false
                }}
                loading={loading}
            />
        </div>
    );
};

export default UnitList;