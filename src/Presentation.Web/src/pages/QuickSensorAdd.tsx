import React, { useState, useEffect } from "react";
import { Button, Form, message } from "antd";
import { SensorListModal } from "../features/sensors/SensorListModal";
import { addSensor, type SensorRequest } from "../features/sensors/sensorAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { fetchAlarms, type Alarm } from "../features/alarms/alarmAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

const QuickSensorAdd: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    
    const [showModal, setShowModal] = useState(false);
    const [modalLoading, setModalLoading] = useState(false);
    const [companies, setCompanies] = useState<Company[]>([]);
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [form] = Form.useForm();

    useEffect(() => {
        Promise.all([
            fetchCompanies(token),
            fetchAlarms(token)
        ]).then(([companiesData, alarmsData]) => {
            setCompanies(companiesData);
            setAlarms(alarmsData);
        });
    }, [token]);

    const handleOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            
            const sensorData: SensorRequest = {
                ...values,
                unitId: Number(values.unitId) || 0,
                companyId: values.companyId || null,
                alarmId: values.alarmId || null,
                channelType: values.channelType || null,
                lowValue: values.lowValue || null,
                highValue: values.highValue || null,
            };
            
            await addSensor(sensorData, token);
            message.success("Sensor added successfully");
            setShowModal(false);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return;
            message.error(err.message || "Failed to add sensor");
        } finally {
            setModalLoading(false);
        }
    };

    return (
        <div>
            <Button 
                type="primary" 
                onClick={() => setShowModal(true)}
            >
                Quick Add Sensor
            </Button>

            <SensorListModal
                showModal={showModal}
                isEdit={false}
                modalLoading={modalLoading}
                form={form}
                companies={companies}
                alarms={alarms}
                onOk={handleOk}
                onCancel={() => {
                    setShowModal(false);
                    form.resetFields();
                }}
            />
        </div>
    );
};

export default QuickSensorAdd;