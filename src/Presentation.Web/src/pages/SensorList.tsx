import React, { useEffect, useState } from "react";
import { message, Form, Button } from "antd";
import {
    fetchSensors,
    addSensor,
    updateSensor,
    deleteSensor,
    type Sensor,
    fetchSensorsByCompany,
    fetchSensorsByAlarm,
    fetchSensorsByUnit
} from "../features/sensors/sensorAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchAlarms, type Alarm } from "../features/alarms/alarmAPI";
import { SensorListModal } from "../features/sensors/SensorListModal";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/NewExtendedAntDTable";
import { getSensorColumns } from "../features/sensors/sensorColumns";
import { PlusOutlined } from "@ant-design/icons";

export interface SensorListProps {
    unitId?: number;
    companyId?: number;
    alarmId?: number;
}

const SensorList: React.FC<SensorListProps> = ({ unitId, companyId, alarmId }) => {
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

    // Add this state for companies and alarms
    const [companies, setCompanies] = useState<Company[]>([]);
    const [alarms, setAlarms] = useState<Alarm[]>([]);

    useEffect(() => {
        loadSensors();
        fetchCompanies(token)
            .then(setCompanies)
            .catch(() => setCompanies([]));
        fetchAlarms(token)
            .then(setAlarms)
            .catch(() => setAlarms([]));
        // eslint-disable-next-line
    }, [token]);

    const loadSensors = async () => {
        setLoading(true);
        try {
            if(unitId){
                const data = await fetchSensorsByUnit(unitId, token);
                setSensors(data);
            }
            else if (companyId) {
                const data = await fetchSensorsByCompany(companyId, token);
                setSensors(data);
            }
            else if (alarmId) {
                const data = await fetchSensorsByAlarm(alarmId, token);
                setSensors(data);
            }else{
            const data = await fetchSensors(token);
            setSensors(data);
            }
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
            companyId: record.companyId ?? "",    // Changed from companyID
            alarmId: record.alarmId ?? "",
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
            
            // Normalize values to handle null/undefined
            const normalizedValues = {
                ...values,
                unitId: Number(values.unitId) || null,
                companyId: values.companyId || null,
                alarmId: values.alarmId || null,
                channelType: values.channelType || null,
                lowValue: values.lowValue || null,
                highValue: values.highValue || null,
            };
            
            if (isEdit && editingId !== null) {
                const sensorData = {
                    ...normalizedValues,
                    id: editingId
                };
                
                await updateSensor(editingId, sensorData, token);
                setSensors(prev =>
                    prev.map(s =>
                        s.id === editingId ? { ...s, ...normalizedValues } : s
                    )
                );
                message.success("Sensor updated");
            } else {
                const added = await addSensor(normalizedValues, token);
                setSensors(prev => [...prev, added]);
                message.success("Sensor added");
            }
            setShowModal(false);
            setEditingId(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return;
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

    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    // If no sensors and this is for a specific unit, show a compact message
    if (!loading && sensors.length === 0 && unitId) {
        return (
            <div style={{
                padding: "16px",
                textAlign: "center",
                backgroundColor: "#fafafa",
                border: "1px solid #f0f0f0",
                borderRadius: "4px",
                margin: "8px 0"
            }}>
                <div style={{
                    color: "#666",
                    fontStyle: "italic",
                    fontSize: "14px",
                    marginBottom: "12px"
                }}>
                    No sensors configured for this unit
                </div>
                <Button
                    type="primary"
                    size="small"
                    icon={<PlusOutlined />}
                    onClick={handleAdd}
                >
                    Add Sensor
                </Button>
                <SensorListModal
                    showModal={showModal}
                    isEdit={isEdit}
                    modalLoading={modalLoading}
                    form={form}
                    companies={companies}
                    alarms={alarms}
                    onOk={handleModalOk}
                    onCancel={handleModalCancel}
                />
            </div>
        );
    }

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <SensorListModal
                showModal={showModal}
                isEdit={isEdit}
                modalLoading={modalLoading}
                form={form}
                companies={companies}
                alarms={alarms}
                onOk={handleModalOk}
                onCancel={handleModalCancel}
            />
            <ExtendedAntDTable<Sensor>
                data={sensors}
                tableColumns={getSensorColumns()}
                title="Sensors"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                loading={loading}
            />
        </div>
    );
};

export default SensorList;
