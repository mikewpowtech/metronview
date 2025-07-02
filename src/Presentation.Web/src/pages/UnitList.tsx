/* eslint-disable @typescript-eslint/no-explicit-any */
import React, { useEffect, useState } from "react";
import { Tooltip, Button, Modal, Input, Form, InputNumber, message, Select } from "antd";
import { DownOutlined, RightOutlined } from "@ant-design/icons";
import { fetchUnits, addUnit, updateUnit, deleteUnit, type Unit } from "../features/units/unitsAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchUnitModels, type UnitModel } from "../features/unitmodels/unitModelAPI";
import { fetchSensorsByUnit, addSensor, updateSensor, type Sensor } from "../features/sensors/sensorAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/ExtendedAntDTable";

const unitStatusOptions = [
    { value: 0, label: "Active" },
    { value: 1, label: "Inactive" },
    { value: 2, label: "Maintenance" },
    { value: 3, label: "Decommissioned" },
    // Add more statuses as defined in your backend enum
];

const allColumnDefs = [
    { title: "ID", dataIndex: "id", key: "id", width: 100 },
    { title: "Unit Type", dataIndex: "unitTypeId", key: "unitTypeId", width: 120 },
    { title: "Phone Number", dataIndex: "phoneNumber", key: "phoneNumber", width: 140 },
    { title: "PIN", dataIndex: "pin", key: "pin", width: 80 },
    { title: "Manufacturer Code", dataIndex: "manufacturerCode", key: "manufacturerCode", width: 150 },
    { title: "Unit Code", dataIndex: "unitCode", key: "unitCode", width: 120 },
    { title: "Secret", dataIndex: "secret", key: "secret", width: 120 },
    { title: "Status", dataIndex: "status", key: "status", width: 120 },
    { title: "Company", dataIndex: "companyID", key: "companyID", width: 180 },
    { title: "Days Before Not Reported", dataIndex: "daysBeforeNotReported", key: "daysBeforeNotReported", width: 180 },
    { title: "Custom Field Values", dataIndex: "customFieldValues", key: "customFieldValues", width: 180 },
];

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

    // Sensor-related states
    const [sensorModal, setSensorModal] = useState<{ open: boolean, unitId?: number, sensor?: Sensor }>({ open: false });
    const [sensorForm] = Form.useForm();
    const [sensorModalLoading, setSensorModalLoading] = useState(false);
    const [sensorsByUnit, setSensorsByUnit] = useState<Record<number, Sensor[]>>({});

    useEffect(() => {
        loadUnits();
        fetchCompanies(token).then(setCompanies).catch(() => setCompanies([]));
        fetchUnitModels(token).then(setUnitModels).catch(() => setUnitModels([]));
        // eslint-disable-next-line
    }, [token]);

    const columnMapper = (col: any) => {
        if (col.key === "companyID") {
            return {
                ...col,
                render: (_: never, record: Unit) => {
                    if (!record.companyID) return "";
                    const company = companies.find(c => c.id === record.companyID);
                    return company?.name ?? "";
                }
            };
        }
        if (col.key === "unitTypeId") {
            return {
                ...col,
                render: (_: never, record: Unit) => {
                    const model = unitModels.find(m => m.id === record.unitTypeId);
                    return model?.name ?? "not specified";
                }
            };
        }
        if (col.key === "status") {
            return {
                ...col,
                render: (_: never, record: Unit) => {
                    const status = unitStatusOptions.find(opt => opt.value === record.status);
                    return status?.label ?? "Unknown";
                }
            };
        }
        return col;
    }

    const loadUnits = async () => {
        setLoading(true);
        try {
            const data = await fetchUnits(token);
            setUnits(data);
            setError(null);
            const sensorsData = await Promise.all(data.map(unit => fetchSensorsByUnit(unit.id, token)));
            const sensorsMap = data.reduce((acc, unit, index) => {
                acc[unit.id] = sensorsData[index];
                return acc;
            }, {} as Record<string, Sensor[]>);
            setSensorsByUnit(sensorsMap);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching units.");
            setUnits([]);
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

    // Sensor modal handlers
    // const openSensorModal = (unitId: number, sensor?: Sensor) => {
    //     setSensorModal({ open: true, unitId, sensor });
    //     sensorForm.setFieldsValue(sensor ? { ...sensor } : { name: "", channel: 0, channelType: undefined, lowValue: undefined, highValue: undefined, engineeringUnits: "", unitId });
    // };

    const handleSensorModalOk = async () => {
        try {
            setSensorModalLoading(true);
            const values = await sensorForm.validateFields();
            if (sensorModal.sensor) {
                values.Id = sensorModal.sensor.id;
                values.UnitId = editingId;
                await updateSensor(sensorModal.sensor.id, values, token);
                message.success("Sensor updated");
            } else {
                await addSensor({ ...values, unitId: sensorModal.unitId }, token);
                message.success("Sensor added");
            }
            setSensorModal({ open: false });
            sensorForm.resetFields();
            loadUnits(); // Reload sensors for the unit
        } catch (err: any) {
            if (err.errorFields) return;
            message.error(err.message || "Failed to save sensor");
        } finally {
            setSensorModalLoading(false);
        }
    };

    const handleSensorModalCancel = () => {
        setSensorModal({ open: false });
        sensorForm.resetFields();
    };

    // const handleDeleteSensor = async (sensorId: number) => {
    //     try {
    //         await deleteSensor(sensorId, token);
    //         message.success("Sensor deleted");
    //         loadUnits(); // Reload sensors for the unit
    //     } catch (err: any) {
    //         message.error(err.message || "Failed to delete sensor");
    //     }
    // };

    const handleExpandRow = (record: Unit) => {
        setExpandedRowKeys(keys =>
            keys.includes(record.id)
                ? keys.filter(key => key !== record.id)
                : [...keys, record.id]
        );
    };

    const SensorListModal: React.FC = () => (
        <Modal
            title={sensorModal.sensor ? "Edit Sensor" : "Add Sensor"}
            open={sensorModal.open}
            onCancel={handleSensorModalCancel}
            onOk={handleSensorModalOk}
            confirmLoading={sensorModalLoading}
            destroyOnHidden={true}
        >
            <Form layout="vertical" form={sensorForm}>
                <Form.Item label="Name" name="name" rules={[{ required: true, message: "Please enter a name" }]}>
                    <Input />
                </Form.Item>
                <Form.Item label="Channel" name="channel" rules={[{ required: true, message: "Please enter a channel" }]}>
                    <InputNumber min={0} style={{ width: "100%" }} />
                </Form.Item>
                <Form.Item label="Channel Type" name="channelType">
                    <InputNumber min={0} style={{ width: "100%" }} />
                </Form.Item>
                <Form.Item label="Low Value" name="lowValue">
                    <InputNumber style={{ width: "100%" }} />
                </Form.Item>
                <Form.Item label="High Value" name="highValue">
                    <InputNumber style={{ width: "100%" }} />
                </Form.Item>
                <Form.Item label="Engineering Units" name="engineeringUnits">
                    <Input />
                </Form.Item>
            </Form>
        </Modal>);

    // Sensors table as a ReactNode (function that takes sensors and unit)
    const getSensorsTable = (unit: Unit, sensors: Sensor[]): React.ReactNode => (
        <div style={{ padding: "12px 0 12px 10px" }} >
            <ExtendedAntDTable<Sensor>
                title={"Sensors for " + unit.unitCode}
                data={sensors}
                tableColumns={[
                    { title: "Name", dataIndex: "name", key: "name", width: 200 },
                    { title: "Channel", dataIndex: "channel", key: "channel", width: 100 },
                    { title: "Channel Type", dataIndex: "channelType", key: "channelType", width: 200 },
                    { title: "Low Value", dataIndex: "lowValue", key: "lowValue", width: 100 },
                    { title: "High Value", dataIndex: "highValue", key: "highValue", width: 100 },
                    { title: "Engineering Units", dataIndex: "engineeringUnits", key: "engineeringUnits", width: 100 },

                ]}
            />
            <SensorListModal />
        </div>
    )

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

    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <UnitListModal />
            <ExtendedAntDTable<Unit>
                data={units}
                tableColumns={allColumnDefs}
                title="Units"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                columnMapper={columnMapper}
                customActions={customActions}
                expandable={{
                    expandedRowRender: (unit: Unit) => {
                        const sensors = sensorsByUnit[unit.id] || [];
                        setEditingId(unit.id);
                        return sensors.length > 0 ? getSensorsTable(unit, sensors) : null;
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