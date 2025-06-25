import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { List, Card, Spin, Typography, Button, Modal, Form, Input } from "antd";
import { fetchConfigurationUploadsByUnit, addConfigurationUpload, type ConfigurationUpload } from "../features/configurationuploads/configurationUploadAPI";

const { Title } = Typography;

const ConfigurationUploads: React.FC = () => {
  const { unitId } = useParams<{ unitId: string }>();
  const [uploads, setUploads] = useState<ConfigurationUpload[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [modalLoading, setModalLoading] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    if (!unitId) return;
    setLoading(true);
    fetchConfigurationUploadsByUnit(Number(unitId))
      .then(setUploads)
      .catch((err) => setError(err?.message || "Failed to load configuration uploads."))
      .finally(() => setLoading(false));
  }, [unitId]);

  const handleAdd = () => {
    setShowModal(true);
    form.resetFields();
  };

  const handleModalOk = async () => {
    try {
      setModalLoading(true);
      const values = await form.validateFields();
      if (!unitId) return;
      const newUpload = await addConfigurationUpload({
        unitId: Number(unitId),
        configuration: values.configuration,
        dateCreatedUtc: new Date().toISOString(),
        queueingUserName: values.queueingUserName,
      });
      setUploads(prev => [...prev, newUpload]);
      setShowModal(false);
      form.resetFields();
    } catch (err: any) {
      // Optionally show error
    } finally {
      setModalLoading(false);
    }
  };

  if (!unitId) return <div>No unit specified.</div>;
  if (loading) return <Spin />;
  if (error) return <div style={{ color: "red" }}>{error}</div>;

  return (
    <div style={{ maxWidth: 900, margin: "0", paddingLeft: 50, paddingRight: 50 }}>
      <Title level={3} style={{ textAlign: "left" }}>
        Configuration Uploads for Unit #{unitId}
      </Title>
      <Button type="primary" style={{ marginBottom: 16 }} onClick={handleAdd}>
        Add Configuration
      </Button>
      <List
        bordered
        dataSource={uploads}
        renderItem={item => (
          <List.Item>
            <Card style={{ width: "100%", textAlign: "left" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <div>
                  <strong>{item.configuration || `Upload #${item.id}`}</strong>
                  <div style={{ fontSize: 12, color: "#888" }}>
                    Created At: {item.dateCreatedUtc}
                    {item.queueingUserName && <> | By: {item.queueingUserName}</>}
                  </div>
                </div>
                {/* Add more actions or details here if needed */}
              </div>
            </Card>
          </List.Item>
        )}
        locale={{ emptyText: "No configuration uploads found for this unit." }}
      />
      <Button style={{ marginTop: 16 }} onClick={() => window.history.back()}>
        Back
      </Button>
      <Modal
        title="Add Configuration Upload"
        open={showModal}
        onCancel={() => setShowModal(false)}
        onOk={handleModalOk}
        confirmLoading={modalLoading}
        destroyOnClose
      >
        <Form layout="vertical" form={form}>
          <Form.Item
            label="Configuration"
            name="configuration"
            rules={[{ required: true, message: "Please enter a configuration" }]}
          >
            <Input.TextArea rows={6} />
          </Form.Item>
          <Form.Item
            label="Created By"
            name="queueingUserName"
            rules={[{ required: true, message: "Please enter uploader's name" }]}
          >
            <Input />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default ConfigurationUploads;