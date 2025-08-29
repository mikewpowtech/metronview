import { Result, Button, Space, Typography } from "antd";
import { useNavigate } from "react-router-dom";
import { ToolOutlined } from "@ant-design/icons";

const { Title, Text } = Typography;

interface UnderDevelopmentPageProps {
    featureName?: string;
    estimatedCompletion?: string;
}

export const UnderDevelopmentPage = ({ 
    featureName = "This Amazing Feature",
    estimatedCompletion = "When the coffee runs out"
}: UnderDevelopmentPageProps) => {
    const navigate = useNavigate();

    const handleGoBack = () => {
        navigate(-1);
    };

    const handleGoHome = () => {
        navigate('/dashboard');
    };

    const jokes = [
        "99 little bugs in the code, take one down, patch it around, 117 little bugs in the code!",
        "Why do programmers prefer dark mode? Because light attracts bugs!",
        "There are only 10 types of people: those who understand binary and those who don't.",
        "A SQL query walks into a bar and asks two tables: 'Can I join you?'",
        "Why did the developer go broke? Because they used up all their cache!",
        "How many programmers does it take to change a light bulb? None, that's hardware!",
        "Why do Java developers wear glasses? Because they can't C#!"
    ];

    const randomJoke = jokes[Math.floor(Math.random() * jokes.length)];

    return (
        <div style={{ 
            display: 'flex', 
            flexDirection: 'column', 
            alignItems: 'center', 
            justifyContent: 'center', 
            minHeight: '40vh',
            padding: '20px' 
        }}>
            <Result
                icon={<ToolOutlined style={{ color: '#1890ff', fontSize: '48px' }} />}
                title={
                    <Title level={3} style={{ color: '#1890ff', margin: '8px 0' }}>
                        {featureName} is Under Construction
                    </Title>
                }
                subTitle={
                    <Space direction="vertical" size="small" style={{ textAlign: 'center', maxWidth: '500px' }}>
                        <Text style={{ fontSize: '14px', color: '#666' }}>
                            Our developers are hard at work building this feature.
                        </Text>
                        
                        <div style={{ 
                            background: '#f6ffed', 
                            border: '1px solid #b7eb8f',
                            borderRadius: '6px',
                            padding: '12px',
                            margin: '16px 0'
                        }}>
                            <Text strong style={{ color: '#52c41a' }}>Estimated Completion: </Text>
                            <Text>{estimatedCompletion}</Text>
                        </div>

                        <div style={{ 
                            background: '#f0f5ff', 
                            border: '1px solid #91d5ff',
                            borderRadius: '6px',
                            padding: '12px',
                            margin: '8px 0'
                        }}>
                            <Text style={{ fontSize: '13px', fontStyle: 'italic', color: '#666' }}>
                                {randomJoke}
                            </Text>
                        </div>
                    </Space>
                }
                extra={
                    <Space size="small">
                        <Button size="small" onClick={handleGoBack}>
                            Go Back
                        </Button>
                        <Button type="primary" size="small" onClick={handleGoHome}>
                            Dashboard
                        </Button>
                    </Space>
                }
            />
        </div>
    );
};