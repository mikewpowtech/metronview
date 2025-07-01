import { useEffect, useState } from "react";
import { Modal, Form, InputNumber, DatePicker, message, Select } from "antd";
import dayjs from "dayjs";
import { fetchReadings, addReading, updateReading, deleteReading, type Reading } from "../features/readings/readingAPI";
import { fetchSensors, type Sensor } from "../features/sensors/sensorAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { ExtendedAntDTable } from "../components/ExtendedAntDTable";

const allColumnDefs = [
    {
        title: "Date Received",
        dataIndex: "dateReceivedUtc",
        key: "dateReceivedUtc", width: 180,
        render: (value: string) =>
            value ? dayjs(value).format("YYYY-MM-DD HH:mm:ss") : "" },
    {
        title: "Date Recorded",
        dataIndex: "dateRecordedUtc",
        key: "dateRecordedUtc",
        width: 180,
        render: (value: string) =>
            value ? dayjs(value).format("YYYY-MM-DD HH:mm:ss") : "", },
    { title: "Sensor ID", dataIndex: "sensor", key: "sensor", width: 100, render: (sensor: Sensor) => sensor?.id ?? "" },
    { title: "Sensor Name", dataIndex: "sensor", key: "sensor", width: 200, render: (sensor: Sensor) => sensor?.name ?? "" },
    { title: "Value", dataIndex: "value", key: "value", width: 120 },
];

const PAGE_SIZE = 20;

const ReadingsList: React.FC = () => {
    const auth = useAppSelector(selectAuth);
    const token = auth?.accessToken;
    const [readings, setReadings] = useState<Reading[]>([]);
    const [sensors, setSensors] = useState<Sensor[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [editingKey, setEditingKey] = useState<{ dateRecordedUtc: string, sensorId: number } | null>(null);
    const [form] = Form.useForm();
    const [modalLoading, setModalLoading] = useState(false);

    // Paging state
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize, setPageSize] = useState(PAGE_SIZE);

    useEffect(() => {
        loadReadings();
        fetchSensors(token).then(setSensors).catch(() => setSensors([]));
        // eslint-disable-next-line
    }, [token]);

    const loadReadings = async () => {
        setLoading(true);
        try {
            const data = await fetchReadings(token);
            setReadings(data);
            setError(null);
        } catch (err: any) {
            setError(err.message || "An error occurred while fetching readings.");
            setReadings([]);
        } finally {
            setLoading(false);
        }
    };

    const handleAdd = () => {
        setIsEdit(false);
        setEditingKey(null);
        form.resetFields();
        setShowModal(true);
    };

    const handleEdit = (record: Reading) => {
        setIsEdit(true);
        setEditingKey({ dateRecordedUtc: record.dateRecordedUtc, sensorId: record.sensor.id });
        form.setFieldsValue({
            dateReceivedUtc: dayjs(record.dateReceivedUtc),
            dateRecordedUtc: dayjs(record.dateRecordedUtc),
            sensorId: record.sensor.id,
            value: record.value,
        });
        setShowModal(true);
    };

    const handleDelete = async (record: Reading) => {
        try {
            await deleteReading(record, token);
            setReadings(prev => prev.filter(r => !(r.dateRecordedUtc === record.dateRecordedUtc && r.sensor.id === record.sensor.id)));
            message.success("Reading deleted");
        } catch (err: any) {
            message.error(err.message || "Failed to delete reading");
        }
    };

    const handleModalOk = async () => {
        try {
            setModalLoading(true);
            const values = await form.validateFields();
            const readingPayload = {
                dateReceivedUtc: new Date().toISOString(),
                dateRecordedUtc: values.dateRecordedUtc.toISOString(),
                sensorId: values.sensorId,
                value: values.value,
            };
            if (isEdit && editingKey) {
                await updateReading(editingKey.dateRecordedUtc, editingKey.sensorId, readingPayload, token);
                setReadings(prev =>
                    prev.map(r =>
                        r.dateRecordedUtc === editingKey.dateRecordedUtc && r.sensor.id === editingKey.sensorId
                            ? { ...readingPayload, sensor: sensors.find(s => s.id === readingPayload.sensorId)! }
                            : r
                    )
                );
                message.success("Reading updated");
            } else {
                const added = await addReading({ ...readingPayload, sensor: null }, token);
                setReadings(prev => [...prev, added]);
                message.success("Reading added");
            }
            setShowModal(false);
            setEditingKey(null);
            form.resetFields();
        } catch (err: any) {
            if (err.errorFields) return; // Form validation error
            message.error(err.message || "Failed to save reading");
        } finally {
            setModalLoading(false);
        }
    };

    const handleModalCancel = () => {
        setShowModal(false);
        setEditingKey(null);
        form.resetFields();
    };

    const MainModal: React.FC = () => (
        <Modal
            title={isEdit ? "Edit Reading" : "Add Reading"}
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
                    value: undefined,
                }}
            >
                <Form.Item
                    label="Date Recorded"
                    name="dateRecordedUtc"
                    rules={[{ required: true, message: "Please select the recorded date" }]}
                >
                    <DatePicker showTime style={{ width: "100%" }} />
                </Form.Item>
                <Form.Item
                    label="Sensor"
                    name="sensorId"
                    rules={[{ required: true, message: "Please select a sensor" }]}
                >
                    <Select
                        showSearch
                        allowClear
                        placeholder="Select a sensor"
                        optionFilterProp="children"
                        filterOption={(input, option) =>
                            typeof option?.children === "string" &&
                            (option.children as string).toLowerCase().includes(input.toLowerCase())
                        }
                    >
                        {sensors.map(sensor => (
                            <Select.Option key={sensor.id} value={sensor.id}>
                                {sensor.name ?? sensor.id}
                            </Select.Option>
                        ))}
                    </Select>
                </Form.Item>
                <Form.Item label="Value" name="value">
                    <InputNumber style={{ width: "100%" }} />
                </Form.Item>
            </Form>
        </Modal>
    );

    if (loading) return <div>Loading readings...</div>;
    if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <MainModal />
            <ExtendedAntDTable<Reading>
                data={readings}
                tableColumns={allColumnDefs}
                title="Telemetry"
                onAdd={handleAdd}
                onEdit={handleEdit}
                onDelete={handleDelete}
                pagination={{
                    current: currentPage,
                    pageSize: pageSize,
                    total: readings.length,
                    showSizeChanger: true,
                    pageSizeOptions: [10, 20, 50, 100],
                    onChange: (page, size) => {
                        setCurrentPage(page);
                        setPageSize(size);
                    },
                }}
            />
        </div>
    );
};

export default ReadingsList;