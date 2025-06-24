import { useEffect, useState } from "react";
import { Table, Button, Modal, Input, Form, InputNumber, Space, Popconfirm, message, Dropdown, Checkbox, Select } from "antd";
import { PlusOutlined, EditOutlined, DeleteOutlined, DownOutlined } from "@ant-design/icons";
import {
  fetchSensors,
  addSensor,
  updateSensor,
  deleteSensor,
  type Sensor
} from "../features/sensors/sensorAPI";
import { fetchCompanies, type Company } from "../features/companies/companyAPI";
import { useAppSelector } from "../app/hooks";
import { selectAuth } from "../app/store";

const allColumnDefs = [
  { title: "ID", dataIndex: "id", key: "id", width: 100 },
  { title: "Name", dataIndex: "name", key: "name", width: 150 },
  { title: "Channel", dataIndex: "channel", key: "channel", width: 50 },
  { title: "Channel Type", dataIndex: "channelType", key: "channelType", width: 75 },
  { title: "Low Value", dataIndex: "lowValue", key: "lowValue", width: 75 },
  { title: "High Value", dataIndex: "highValue", key: "highValue", width: 75 },
  { title: "Engineering Units", dataIndex: "engineeringUnits", key: "engineeringUnits", width: 80 },
  { title: "Unit ID", dataIndex: "unitId", key: "unitId", width: 120 },
  { title: "Company ID", dataIndex: "companyID", key: "companyID", width: 120 },
];

const SensorList: React.FC = () => {
  const auth = useAppSelector(selectAuth);
  const token = auth?.accessToken;
  const [sensors, setSensors] = useState<Sensor[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [isEdit, setIsEdit] = useState(false);
  const [editingId, setEditingId] = useState<number|null>(null);
  const [form] = Form.useForm();
  const [modalLoading, setModalLoading] = useState(false);

  // Add this state for column visibility
  const [visibleKeys, setVisibleKeys] = useState<string[]>(allColumnDefs.map(col => col.key as string));
  const [columns, setColumns] = useState(allColumnDefs);

  // Add this state for companies
  const [companies, setCompanies] = useState<Company[]>([]);

  useEffect(() => {
    loadSensors();
    fetchCompanies(token)
      .then(setCompanies)
      .catch(() => setCompanies([]));
    // eslint-disable-next-line
  }, [token]);

  useEffect(() => {
    setColumns(
      allColumnDefs.filter(col => visibleKeys.includes(col.key as string))
    );
  }, [visibleKeys]);

  const loadSensors = async () => {
    setLoading(true);
    try {
      const data = await fetchSensors(token);
      setSensors(data);
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
      companyID: record.companyID ?? "",
    });
    setShowModal(true);
  };

  const handleDelete = async (id: number) => {
    try {
      await deleteSensor(id, token);
      setSensors(prev => prev.filter(s => s.id !== id));
      message.success("Sensor deleted");
    } catch (err: any) {
      message.error(err.message || "Failed to delete sensor");
    }
  };

  const handleModalOk = async () => {
    try {
      setModalLoading(true);
      const values = await form.validateFields();
        if (isEdit && editingId !== null) {
            values.UnitId = editingId;
            await updateSensor(editingId, values, token);
            setSensors(prev =>
            prev.map(s =>
                s.id === editingId ? { ...s, ...values } : s
            )
        );
        message.success("Sensor updated");
      } else {
        const added = await addSensor(values, token);
        setSensors(prev => [...prev, added]);
        message.success("Sensor added");
      }
      setShowModal(false);
      setEditingId(null);
      form.resetFields();
    } catch (err: any) {
      if (err.errorFields) return; // Form validation error
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
  const tableColumns = [
    ...columns,
    {
      title: "Actions",
      key: "actions",
      width: 120,
      render: (_: any, record: Sensor) => (
        <Space>
          <Button
            icon={<EditOutlined />}
            size="small"
            onClick={() => handleEdit(record)}
          />
          <Popconfirm
            title="Delete this sensor?"
            onConfirm={() => handleDelete(record.id)}
            okText="Yes"
            cancelText="No"
          >
            <Button
              icon={<DeleteOutlined />}
              size="small"
              danger
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  if (loading) return <div>Loading sensors...</div>;
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
      <h2>Sensors</h2>
      <Button
        type="primary"
        icon={<PlusOutlined />}
        onClick={handleAdd}
        style={{ marginBottom: 16 }}
      >
        Add Sensor
      </Button>
      <Modal
        title={isEdit ? "Edit Sensor" : "Add Sensor"}
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
            name: "",
            channel: 0,
            channelType: undefined,
            lowValue: undefined,
            highValue: undefined,
            engineeringUnits: "",
            unitId: "",
            companyID: "",
          }}
        >
          <Form.Item
            label="Name"
            name="name"
            rules={[{ required: true, message: "Please enter a name" }]}
          >
            <Input placeholder="Sensor Name" />
          </Form.Item>
          <Form.Item
            label="Channel"
            name="channel"
            rules={[{ required: true, message: "Please enter a channel" }]}
          >
            <InputNumber min={0} style={{ width: "100%" }} placeholder="Channel" />
          </Form.Item>
          <Form.Item label="Channel Type" name="channelType">
            <InputNumber min={0} style={{ width: "100%" }} placeholder="Channel Type" />
          </Form.Item>
          <Form.Item label="Low Value" name="lowValue">
            <InputNumber style={{ width: "100%" }} placeholder="Low Value" />
          </Form.Item>
          <Form.Item label="High Value" name="highValue">
            <InputNumber style={{ width: "100%" }} placeholder="High Value" />
          </Form.Item>
          <Form.Item label="Engineering Units" name="engineeringUnits">
            <Input placeholder="Engineering Units" />
          </Form.Item>
          <Form.Item label="Unit ID" name="unitId">
            <Input placeholder="Unit ID" />
          </Form.Item>
          <Form.Item label="Company ID" name="companyID">
            <Select
              showSearch
              allowClear
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
        </Form>
      </Modal>

             <Table<Sensor>
                bordered
                dataSource={sensors}
                columns={tableColumns}
                rowKey="id"
                pagination={false}
                scroll={{ x: "max-content" }}
            />
        </div>
    );
};

export default SensorList;