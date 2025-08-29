import { useState, useEffect } from 'react';
import { Card, Button, Typography, Space, Divider, Tag, Progress, Tooltip } from 'antd';
import { 
    ToolOutlined, 
    CoffeeOutlined, 
    BugOutlined, 
    RocketOutlined, 
    LoadingOutlined,
    ThunderboltOutlined,
    StarOutlined
} from '@ant-design/icons';

const { Title, Paragraph, Text } = Typography;

interface UnderDevelopmentPageProps {
    pageName?: string;
    estimatedCompletion?: string;
    features?: string[];
}

export const UnderDevelopmentPage: React.FC<UnderDevelopmentPageProps> = ({
    pageName = "This Amazing Feature",
    estimatedCompletion = "Soon™",
    features = ["Mind-blowing functionality", "Revolutionary UX", "Coffee-powered algorithms"]
}) => {
    const [progress, setProgress] = useState(0);
    const [coffeeCount, setCoffeeCount] = useState(0);
    const [loadingDots, setLoadingDots] = useState('');

    useEffect(() => {
        // Simulate progress animation
        const progressTimer = setInterval(() => {
            setProgress(prev => {
                if (prev >= 95) return Math.random() * 30 + 60; // Reset to random between 60-90
                return prev + Math.random() * 5;
            });
        }, 2000);

        // Animate loading dots
        const dotsTimer = setInterval(() => {
            setLoadingDots(prev => {
                if (prev.length >= 3) return '';
                return prev + '.';
            });
        }, 500);

        return () => {
            clearInterval(progressTimer);
            clearInterval(dotsTimer);
        };
    }, []);

    const drinkCoffee = () => {
        setCoffeeCount(prev => prev + 1);
        setProgress(prev => Math.min(prev + 5, 100));
    };

    const excuses = [
        "The code is working perfectly... on my machine",
        "Still debugging why the coffee machine broke",
        "Waiting for Stack Overflow to answer my question",
        "The feature is 90% complete (like all features)",
        "Currently rewriting in the latest JavaScript framework",
        "It works, but the CSS is having an existential crisis",
        "The database is having commitment issues",
        "Teaching the AI to understand business requirements",
        "Convincing the server that 'undefined' is not a function",
        "Negotiating with the APIs that refuse to cooperate"
    ];

    const [currentExcuse, setCurrentExcuse] = useState(excuses[0]);

    const getNewExcuse = () => {
        const randomExcuse = excuses[Math.floor(Math.random() * excuses.length)];
        setCurrentExcuse(randomExcuse);
    };

    return (
        <div className="under-development-container">
            <div className="under-development-content">
                <Card className="development-card">
                    <div className="text-center">
                        <div className="construction-icon">
                            <ToolOutlined style={{ fontSize: '4rem', color: '#faad14' }} />
                        </div>
                        
                        <Title level={2} style={{ color: '#1890ff', marginTop: '1rem' }}>
                            ?? {pageName} Under Construction ??
                        </Title>
                        
                        <Paragraph style={{ fontSize: '1.1rem', color: '#666' }}>
                            Our highly caffeinated developers are working their magic!
                        </Paragraph>

                        <div className="progress-section">
                            <Text strong>Development Progress:</Text>
                            <Progress 
                                percent={Math.round(progress)} 
                                status={progress < 100 ? "active" : "success"}
                                strokeColor={{
                                    '0%': '#108ee9',
                                    '100%': '#87d068',
                                }}
                            />
                        </div>

                        <Divider />

                        <div className="excuse-section">
                            <Card size="small" style={{ backgroundColor: '#f6f6f6', border: '1px dashed #d9d9d9' }}>
                                <Text italic>"{currentExcuse}"</Text>
                                <br />
                                <Button 
                                    type="link" 
                                    size="small" 
                                    onClick={getNewExcuse}
                                    style={{ padding: 0, marginTop: 8 }}
                                >
                                    Get another excuse
                                </Button>
                            </Card>
                        </div>

                        <Divider />

                        <div className="features-section">
                            <Title level={4}>
                                <RocketOutlined /> Coming Soon Features:
                            </Title>
                            <Space direction="vertical" style={{ width: '100%' }}>
                                {features.map((feature, index) => (
                                    <Tag key={index} color="blue" style={{ padding: '4px 8px', fontSize: '14px' }}>
                                        <StarOutlined /> {feature}
                                    </Tag>
                                ))}
                            </Space>
                        </div>

                        <Divider />

                        <div className="stats-section">
                            <Space size="large">
                                <div className="stat-item">
                                    <div style={{ fontSize: '2rem' }}>?</div>
                                    <Text>Coffee Consumed: {coffeeCount}</Text>
                                </div>
                                <div className="stat-item">
                                    <div style={{ fontSize: '2rem' }}>??</div>
                                    <Text>Bugs: 2.7 (it's complicated)</Text>
                                </div>
                                <div className="stat-item">
                                    <div style={{ fontSize: '2rem' }}>?</div>
                                    <Text>ETA: {estimatedCompletion}</Text>
                                </div>
                            </Space>
                        </div>

                        <Divider />

                        <div className="action-section">
                            <Space>
                                <Tooltip title="This might actually help">
                                    <Button 
                                        type="primary" 
                                        icon={<CoffeeOutlined />}
                                        onClick={drinkCoffee}
                                    >
                                        Buy Developer Coffee
                                    </Button>
                                </Tooltip>
                                
                                <Tooltip title="Pretend to fix something">
                                    <Button 
                                        icon={<BugOutlined />}
                                        onClick={() => setProgress(prev => Math.min(prev + 10, 100))}
                                    >
                                        Debug Code
                                    </Button>
                                </Tooltip>
                                
                                <Button 
                                    type="ghost" 
                                    icon={<ThunderboltOutlined />}
                                    onClick={() => setProgress(100)}
                                >
                                    Magic Button
                                </Button>
                            </Space>
                        </div>

                        <div className="loading-section" style={{ marginTop: '2rem' }}>
                            <Text type="secondary">
                                <LoadingOutlined spin /> Loading awesomeness{loadingDots}
                            </Text>
                        </div>

                        <Divider />

                        <Paragraph style={{ color: '#999', fontSize: '0.9rem' }}>
                            ?? <strong>Pro Tip:</strong> Check back later, or refresh the page really fast. 
                            Sometimes that helps. We're not sure why, but it makes everyone feel better.
                        </Paragraph>
                    </div>
                </Card>
            </div>
        </div>
    );
};

export default UnderDevelopmentPage;