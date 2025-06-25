import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { List, Card, Spin, Typography, Button } from "antd";
import { fetchConfigurationUploadsByUnit, type ConfigurationUpload } from "../features/configurationuploads/configurationUploadAPI";

const { Title } = Typography;

const ConfigurationUploads: React.FC = () => {
  const { unitId } = useParams<{ unitId: string }>();
  const [uploads, setUploads] = useState<ConfigurationUpload[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!unitId) return;
    setLoading(true);
    fetchConfigurationUploadsByUnit(Number(unitId))
      .then(setUploads)
      .catch((err) => setError(err?.message || "Failed to load configuration uploads."))
      .finally(() => setLoading(false));
  }, [unitId]);

  if (!unitId) return <div>No unit specified.</div>;
  if (loading) return <Spin />;
  if (error) return <div style={{ color: "red" }}>{error}</div>;

  return (
    <div style={{ maxWidth: 700, margin: "0 auto" }}>
      <Title level={3}>Configuration Uploads for Unit #{unitId}</Title>
      <List
        bordered
        dataSource={uploads}
        renderItem={item => (
          <List.Item>
            <Card style={{ width: "100%" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <div>
                  <strong>{item.fileName || `Upload #${item.id}`}</strong>
                  <div style={{ fontSize: 12, color: "#888" }}>
                    Uploaded At: {item.uploadedAtUtc}
                    {item.uploadedBy && <> | By: {item.uploadedBy}</>}
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
    </div>
  );
};

export default ConfigurationUploads;