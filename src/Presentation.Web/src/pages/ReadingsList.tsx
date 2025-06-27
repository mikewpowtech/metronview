import { useEffect, useState } from "react";
import {
    Tooltip, Button, Modal, Form, InputNumber, DatePicker,
    Popconfirm, message, Dropdown, Checkbox, Select
} from "antd";
import dayjs from "dayjs";
import { fetchReadings, addReading, updateReading, deleteReading, type Reading } from "../features/readings/readingAPI";
import { fetchSensors, type Sensor } from "../features/sensors/sensorAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";
import { PlusOutlined, EditOutlined, DeleteOutlined, DownOutlined } from "@ant-design/icons";
import { GenericTable } from "../components/GenericTable";

const allColumnDefs = [
  { title: "Date Received", dataIndex: "dateReceivedUtc", key: "dateReceivedUtc", width: 180 },
  { title: "Date Recorded", dataIndex: "dateRecordedUtc", key: "dateRecordedUtc", width: 180 },
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

    // Column visibility state
    const [visibleKeys, setVisibleKeys] = useState<string[]>(
        allColumnDefs.filter(col => col.key !== "id").map(col => col.key as string)
    );

  const [columns, setColumns] = useState(allColumnDefs);

  useEffect(() => {
    loadReadings();
    fetchSensors(token).then(setSensors).catch(() => setSensors([]));
    // eslint-disable-next-line
  }, [token]);

  useEffect(() => {
    setColumns(
      allColumnDefs.filter(col => visibleKeys.includes(col.key as string))
    );
  }, [visibleKeys]);

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

  const handleDelete = async (dateRecordedUtc: string, sensorId: number) => {
    try {
      await deleteReading(dateRecordedUtc, sensorId, token);
      setReadings(prev => prev.filter(r => !(r.dateRecordedUtc === dateRecordedUtc && r.sensor.id === sensorId)));
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

  // Dropdown menu items for columns
  const columnMenuItems = allColumnDefs.map(col => ({
    key: col.key,
    label: (
      <Checkbox
        checked={visibleKeys.includes(col.key as string)}
        onChange={e => {
          const checked = e.target.checked;
          setVisibleKeys(keys =>
            checked
              ? [...keys, col.key as string]
              : keys.filter(k => k !== col.key)
          );
        }}
        disabled={visibleKeys.length === 1 && visibleKeys.includes(col.key as string)}
        style={{ width: "100%", padding: "4px 12px" }}
      >
        {col.title}
      </Checkbox>
    ),
  }));

  // Add the Actions column after filtering
      const actionsColumns = {
          title: "",
          key: "actions",
          align: "center" as const,
          className: "actions-col",
          render: (_: any, record: Reading) => (
              <span className="actions-col-inner">
                  <Tooltip title="edit">
          <Button
            icon={<EditOutlined />}
            size="small"
            onClick={() => handleEdit(record)}
                      />
                  </Tooltip>
          <Popconfirm
            title="Delete this reading?"
            onConfirm={() => handleDelete(record.dateRecordedUtc, record.sensor.id)}
            okText="Yes"
            cancelText="No"
                  >
                      <Tooltip title="delete">
            <Button
              icon={<DeleteOutlined />}
              size="small"
              danger
                          />
            </Tooltip>
          </Popconfirm>
        </span>
      ),
    };

    // Place Actions column first
    const tableColumns = [
        actionsColumns,
        ...columns,
    ];

  if (loading) return <div>Loading readings...</div>;
  if (error) return <div style={{ color: "red" }}>Error: {error}</div>;

  return (
    <div>
      <div style={{ marginBottom: 16 }}>
        <Dropdown menu={{ items: columnMenuItems }} trigger={["click"]}>
          <Button>
            Columns <DownOutlined />
          </Button>
        </Dropdown>
      </div>
      <h2>Readings</h2>
      <Button
        type="primary"
        icon={<PlusOutlined />}
        onClick={handleAdd}
        style={{ marginBottom: 16 }}
      >
        Add Reading
      </Button>
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
      <GenericTable<Reading>
        data={readings}
        columns={tableColumns}
        rowKey={r => `${r.dateRecordedUtc}_${r.sensor.id}`}
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