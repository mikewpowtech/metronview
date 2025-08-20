import { useState } from 'react';
import { Form, message } from 'antd';
import {
    addSensor,
    updateSensor,
    type Sensor,
    type SensorRequest
} from '../features/sensors/sensorAPI';

interface UseSensorModalProps {
    token?: string;
    onSensorAdded?: (sensor: Sensor) => void;
    onSensorUpdated?: (sensor: Sensor) => void;
}

export const useSensorModal = ({ token, onSensorAdded, onSensorUpdated }: UseSensorModalProps) => {
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingId, setEditingId] = useState<number | null>(null);
    const [modalLoading, setModalLoading] = useState(false);
    const [form] = Form.useForm();

    const openAddModal = () => {
        setIsEdit(false);
        setEditingId(null);
        form.resetFields();
        setShowModal(true);
    };

    const openEditModal = (sensor: Sensor) => {
        setIsEdit(true);
        setEditingId(sensor.id);
        form.setFieldsValue({
            name: sensor.name ?? "",
            channel: sensor.channel,
            channelType: sensor.channelType,
            lowValue: sensor.lowValue,
            highValue: sensor.highValue,
            engineeringUnits: sensor.engineeringUnits ?? "",
            unitId: sensor.unitId,
            companyId: sensor.companyId,
            alarmId: sensor.alarmId,
        });
        setShowModal(true);
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            
            const normalizedValues: SensorRequest = {
                ...values,
                unitId: Number(values.unitId) || 0,
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
                const updatedSensor = { ...sensorData } as Sensor;
                onSensorUpdated?.(updatedSensor);
                message.success("Sensor updated successfully");
            } else {
                const newSensor = await addSensor(normalizedValues, token);
                onSensorAdded?.(newSensor);
                message.success("Sensor added successfully");
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

    return {
        // Modal state
        showModal,
        isEdit,
        modalLoading,
        form,
        
        // Modal actions
        openAddModal,
        openEditModal,
        handleModalOk,
        handleModalCancel,
    };
};