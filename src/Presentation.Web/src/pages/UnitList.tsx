/* eslint-disable @typescript-eslint/no-explicit-any */
import React, { useEffect, useState } from "react";
import { Tooltip, Button, Modal, Input, Form, InputNumber, message, Select, Switch } from "antd";
import { DownOutlined, RightOutlined, PlusOutlined } from "@ant-design/icons";
import { fetchUnits, addUnit, updateUnit, deleteUnit, type Unit } from "../features/units/unitsAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchUnitModels, type UnitModel } from "../features/unitmodels/unitModelAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { getUnitListColumns } from "../features/units/unitListColumns";
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
            unitTypeId: model?.id?.toString() || "",
            phoneNumber: record.phoneNumber ?? "",
            pin: record.pin ?? "",
            manufacturerCode: record.manufacturerCode,
            unitCode: record.unitCode ?? "",
            secret: record.secret ?? "",
            status: record.status ?? 0,
            companyID: record.companyId ?? undefined,
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
        return(<SensorList unitId={unit.id}/>);
    };

    //const getSensorsTable = (unit: Unit): React.ReactNode => {
    //    return (
    //        <div style={{
    //            position: 'relative',
    //            width: '100%',
    //            maxWidth: '100%',
    //            height: '200px', // Fixed height instead of minHeight
    //            padding: '16px',
    //            backgroundColor: '#fafafa',
    //            boxSizing: 'border-box',
    //            overflow: 'hidden' // Prevent content from expanding
    //        }}>
    //            <div style={{
    //                position: 'absolute',
    //                top: '16px',
    //                left: '16px',
    //                right: '16px',
    //                bottom: '16px',
    //                overflow: 'auto' // Allow scrolling within fixed bounds
    //            }}>
    //                <SensorList unitId={unit.id} />
    //            </div>
    //        </div>
    //    );
    //};

    const UnitListModal: React.FC = () => {
        const statusValue = Form.useWatch('status', form) || 0;
        
        return (
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
                    <PlusOutlined style={{ color: '#ffffff' }} />
                    {isEdit ? "Edit Unit" : "Add Unit"}
                </div>
            }
            open={showModal}
            onCancel={handleModalCancel}
            onOk={handleModalOk}
            okText="Save"
            confirmLoading={modalLoading}
            destroyOnHidden={true}
            width={520}
            styles={{
                header: {
                    background: '#1890ff',
                    borderRadius: '8px 8px 0 0',
                    padding: '12px 20px',
                    border: '2px solid #1890ff',
                    borderBottom: 'none',
                    marginBottom: '0'
                },
                body: {
                    background: '#ffffff',
                    padding: '16px 20px',
                    border: '2px solid #1890ff',
                    borderTop: 'none',
                    borderBottom: 'none',
                    marginTop: '0'
                },
                footer: {
                    background: '#ffffff',
                    padding: '12px 20px',
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
            size="small"
            initialValues={{
                unitTypeId: "",
                phoneNumber: "",
                pin: "",
                manufacturerCode: "",
                unitCode: "",
                secret: "",
                status: 0, // Default to Active
                companyID: undefined,
                daysBeforeNotReported: undefined,
                customFieldValues: "",
            }}
        >
            {/* Row 1: Manufacturer Code */}
            <Form.Item
                label="Manufacturer Code"
                name="manufacturerCode"
                rules={[{ required: true, message: "Please enter a manufacturer code" }]}
                style={{ marginBottom: 10 }}
            >
                <Input placeholder="Manufacturer Code" />
            </Form.Item>

            {/* Row 2: Unit Type */}
            <Form.Item
                label="Unit Type"
                name="unitTypeId"
                rules={[{ required: true, message: "Please select a unit type" }]}
                style={{ marginBottom: 10 }}
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

            {/* Row 3: Company */}
            <Form.Item label="Company" name="companyID" style={{ marginBottom: 10 }}>
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

            {/* Row 4: Phone Number and PIN */}
            <div style={{ display: 'flex', gap: '12px' }}>
                <Form.Item label="Phone Number" name="phoneNumber" style={{ flex: 1, marginBottom: 10 }}>
                    <Input placeholder="Phone Number" />
                </Form.Item>
                <Form.Item label="PIN" name="pin" style={{ flex: 1, marginBottom: 10 }}>
                    <Input placeholder="PIN" />
                </Form.Item>
            </div>

            {/* Row 5: Unit Code and Secret */}
            <div style={{ display: 'flex', gap: '12px' }}>
                <Form.Item label="Unit Code" name="unitCode" style={{ flex: 1, marginBottom: 10 }}>
                    <Input placeholder="Unit Code" />
                </Form.Item>
                <Form.Item label="Secret" name="secret" style={{ flex: 1, marginBottom: 10 }}>
                    <Input placeholder="Secret" />
                </Form.Item>
            </div>

            {/* Row 6: Days Before Not Reported */}
            <Form.Item label="Days Before Not Reported" name="daysBeforeNotReported" style={{ marginBottom: 10 }}>
                <InputNumber min={0} style={{ width: "100%" }} placeholder="Days" />
            </Form.Item>

            {/* Row 7: Status */}
            <Form.Item
                label="Status"
                name="status"
                style={{ marginBottom: 10 }}
                valuePropName="checked"
                getValueFromEvent={(checked) => checked ? 0 : 1}
                getValueProps={(value) => ({ checked: value === 0 })}
            >
                <Switch
                    size="default"
                    checkedChildren="Active"
                    unCheckedChildren="Inactive"
                    style={{
                        backgroundColor: statusValue === 0 ? '#52c41a' : '#ff4d4f',
                        transform: 'scale(1.2)', // Make it 20% bigger
                        transformOrigin: 'left center', // Scale from left edge to maintain alignment
                        minWidth: '80px' // Ensure minimum width for text
                    }}
                />
            </Form.Item>
            
            {/* Row 8: Custom Field Values */}
            <Form.Item label="Custom Field Values" name="customFieldValues" style={{ marginBottom: 6 }}>
                <Input placeholder="Custom Field Values" />
            </Form.Item>
        </Form>
    </Modal>
    );
};

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
                    expandedRowRender: getSensorsTable,
                    expandedRowKeys,
                    onExpand: (_, record) => handleExpandRow(record),
                    showExpandColumn: false
                }}
                loading={loading}
            />
        </div>
    );

    //return (
    //    <div style={{
    //        padding: "12px 0 12px 30px",
    //        width: '100%',
    //        maxWidth: '100%',
    //        overflow: 'hidden'
    //    }}>
    //        <UnitListModal />
    //        <div style={{
    //            width: '100%',
    //            maxWidth: '100%',
    //            overflow: 'auto'
    //        }}>
    //            <ExtendedAntDTable<Unit>
    //                data={units}
    //                tableColumns={unitListColumns}
    //                title="Units"
    //                onAdd={handleAdd}
    //                onEdit={handleEdit}
    //                onDelete={handleDelete}
    //                customActions={customActions}
    //                expandable={{
    //                    expandedRowRender: (unit: Unit) => getSensorsTable(unit),
    //                    expandedRowKeys,
    //                    onExpand: (_, record) => handleExpandRow(record),
    //                    showExpandColumn: false,
    //                    indentSize: 0
    //                }}
    //                loading={loading}
    //                scroll={{ x: true, y: 400 }} // Change scroll behavior
    //                size="small"
    //                style={{
    //                    width: '100%',
    //                    maxWidth: '100%',
    //                    tableLayout: 'fixed'
    //                }}
    //            />
    //        </div>
    //    </div>
    //);
};

export default UnitList;